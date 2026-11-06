// Creates, on a fresh Vaultwarden, a test account holding the data expected by SharpWarden.Tests/SharpWardenTest.cs,
// then writes the matching test configuration file (see SharpWarden.Tests/TestConfig.cs).
//
// Usage: dotnet run --project SharpWarden.Tests.Seed -- <vaultwarden url> <output config file> [argon2id|pbkdf2]
// The server must allow sign-ups (SIGNUPS_ALLOWED=true) and have no SMTP configured.

using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using SharpWarden;
using SharpWarden.BitWardenDatabaseSession.Models.CipherItem;
using SharpWarden.BitWardenDatabaseSession.Services;
using SharpWarden.BitWardenWebSession;
using SharpWarden.WebClient.Services;
using SharpWarden.Tests;

if (args.Length < 2)
{
    Console.Error.WriteLine("Usage: SharpWarden.Tests.Seed <vaultwarden url> <output config file> [argon2id|pbkdf2]");
    return 1;
}

var hostUrl = args[0].TrimEnd('/');
var outputPath = args[1];
var kdf = args.Length > 2 && args[2].Equals("pbkdf2", StringComparison.OrdinalIgnoreCase)
    ? KdfParameters.Pbkdf2(600000)
    : KdfParameters.Argon2id(3, 64, 4);

var email = $"sharpwarden-{Guid.NewGuid():N}@example.com";
var password = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24));
var passwordBytes = Encoding.UTF8.GetBytes(password);

using var http = new HttpClient { BaseAddress = new Uri(hostUrl + "/") };
await WaitForServerAsync(http);

// 1. Register the account, encrypted client side like the Bitwarden clients do.
Console.WriteLine($"Registering {email} ({kdf})...");
var masterKey = BitWardenCipherService.ComputeMasterKey(Encoding.UTF8.GetBytes(email), passwordBytes, kdf);
var masterPasswordHash = Convert.ToBase64String(BitWardenCipherService.ComputeUserPasswordHash(masterKey, passwordBytes));
var stretchedEncKey = BitWardenCipherService.HKDFSHA256(masterKey, "enc"u8.ToArray(), 32);
var stretchedMacKey = BitWardenCipherService.HKDFSHA256(masterKey, "mac"u8.ToArray(), 32);

var userKey = RandomNumberGenerator.GetBytes(64);
using var rsa = RSA.Create(2048);
var publicKey = rsa.ExportSubjectPublicKeyInfo();

// Without SMTP, Vaultwarden returns the email verification token directly.
var verificationToken = (await PostAsync(http, "identity/accounts/register/send-verification-email", new JObject
{
    ["email"] = email,
    ["name"] = "SharpWarden Tests",
    ["receiveMarketingEmails"] = false,
})).Trim().Trim('"');

await PostAsync(http, "identity/accounts/register/finish", new JObject
{
    ["email"] = email,
    ["emailVerificationToken"] = verificationToken,
    ["masterPasswordHash"] = masterPasswordHash,
    ["key"] = BitWardenCipherService.EncryptWithAesCbc256HmacSha256Base64(userKey, stretchedEncKey, stretchedMacKey),
    ["kdf"] = (int)kdf.Type,
    ["kdfIterations"] = kdf.Iterations,
    ["kdfMemory"] = kdf.Memory,
    ["kdfParallelism"] = kdf.Parallelism,
    ["keys"] = new JObject
    {
        ["publicKey"] = Convert.ToBase64String(publicKey),
        ["encryptedPrivateKey"] = BitWardenCipherService.EncryptWithAesCbc256HmacSha256Base64(rsa.ExportPkcs8PrivateKey(), userKey[..32], userKey[32..]),
    },
});

// 2. Log in with SharpWarden.
using var scope = BitWardenHelper.CreateSessionScope(hostUrl, hostUrl + "/notifications");
var webClient = scope.ServiceProvider.GetRequiredService<IWebClientService>();
var vault = scope.ServiceProvider.GetRequiredService<IVaultService>();
var userCrypto = scope.ServiceProvider.GetRequiredService<IUserCryptoService>();
var organizationCryptoFactory = scope.ServiceProvider.GetRequiredService<IOrganizationCryptoFactoryService>();

await webClient.PreloginAsync(email);
await webClient.AuthenticateAsync(password);
http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", webClient.GetWebSession().AccessToken);

// 3. API key, used by the tests to log in.
var apiKey = JObject.Parse(await PostAsync(http, "api/accounts/api-key", new JObject { ["masterPasswordHash"] = masterPasswordHash })).Value<string>("apiKey");

// 4. Organization (with its default collection), its key encrypted with the user public key.
Console.WriteLine("Creating the organization...");
var organizationKey = RandomNumberGenerator.GetBytes(64);
var organization = JObject.Parse(await PostAsync(http, "api/organizations", new JObject
{
    ["name"] = "SharpWarden Tests",
    ["billingEmail"] = email,
    ["key"] = BitWardenCipherService.EncryptWithRsa2048OaepSha1Base64(organizationKey, publicKey),
    ["collectionName"] = BitWardenCipherService.EncryptStringWithAesCbc256HmacSha256Base64("SharpWardenTestCollection", organizationKey[..32], organizationKey[32..]),
    ["planType"] = 0,
}));
var organizationId = Guid.Parse(organization.Value<string>("id"));

// 5. Load the vault to get the user and organization keys.
var database = await webClient.GetDatabaseAsync();
vault.LoadBitWardenDatabase(passwordBytes, webClient.UserKdf, database);
var collectionId = database.Collections.First(e => e.OrganizationId == organizationId).Id.Value;
var organizationCrypto = organizationCryptoFactory.GetOrganizationCryptoService(organizationId);

// 6. The data checked by the read-only tests (_01xx and _02xx).
Console.WriteLine("Creating the test data...");
EncryptedString Encrypt(IUserCryptoService crypto, string value) => new(crypto) { ClearString = value };
EncryptedString User(string value) => Encrypt(userCrypto, value);

var folder = await webClient.CreateFolderAsync(User("SharpWardenTestFolder").CipherString);

async Task<Guid> CreateAsync(CipherItemModel item, Action<CipherItemModel> fill)
{
    fill(item);
    return (await webClient.CreateCipherItemAsync(item)).Id.Value;
}

var userItemId = await CreateAsync(new CipherItemModel(userCrypto) { Name = User("SharpWardenTestItem"), Reprompt = CipherRepromptType.Password }, item =>
{
    var login = item.CreateLogin();
    login.Username = User("TestUserName");
    login.Password = User("TestPassword");
});

var userItemInFolderId = await CreateAsync(new CipherItemModel(userCrypto) { Name = User("SharpWardenTestItemInFolder"), FolderId = folder.Id }, item =>
{
    var login = item.CreateLogin();
    login.Username = User("FolderUser");
    login.Password = User("FolderPassword");
});

var userItemLoginId = await CreateAsync(new CipherItemModel(userCrypto) { Name = User("SharpWardenTestLogin"), Notes = User("LoginNotes") }, item =>
{
    var login = item.CreateLogin();
    login.Username = User("LoginUsername");
    login.Password = User("LoginPassword");
    foreach (var (uri, match) in new (string, UriMatchType?)[]
    {
        ("LoginUrlDefault", null),
        ("LoginUrlBaseDomain", UriMatchType.Domain),
        ("LoginUrlHost", UriMatchType.Host),
        ("LoginUrlStartsWith", UriMatchType.BeginWith),
        ("LoginUrlRegularExpression", UriMatchType.Regex),
        ("LoginUrlExact", UriMatchType.Exact),
        ("LoginUrlNever", UriMatchType.Never),
    })
        login.Uris.Add(new UriModel(userCrypto) { Uri = User(uri), Match = match });
});

var userItemCardId = await CreateAsync(new CipherItemModel(userCrypto) { Name = User("SharpWardenTestCard"), Notes = User("CardNotes") }, item =>
{
    var card = item.CreateCard();
    card.CardholderName = User("CardholderName");
    card.Number = User("CardNumber");
    card.ExpYear = User("2025");
    card.ExpMonth = User("6");
    card.Code = User("CardSecurityCode");
    card.Brand = User("Visa");
});

var userItemIdentityId = await CreateAsync(new CipherItemModel(userCrypto) { Name = User("SharpWardenTestIdentity"), Notes = User("IdentityNotes") }, item =>
{
    var identity = item.CreateIdentity();
    identity.Address1 = User("IdentityAddress1");
    identity.Address2 = User("IdentityAddress2");
    identity.Address3 = User("IdentityAddress3");
    identity.City = User("IdentityCity");
    identity.State = User("IdentityState");
    identity.PostalCode = User("IdentityPostalCode");
    identity.Country = User("IdentityCountry");
    identity.FirstName = User("IdentityFirstName");
    identity.MiddleName = User("IdentityMiddleName");
    identity.LastName = User("IdentityLastName");
    identity.Username = User("IdentityUserName");
    identity.Company = User("IdentityCompany");
    identity.SecuritySocialNumber = User("IdentitySocialSecurityNumber");
    identity.PassportNumber = User("IdentityPassportNumber");
    identity.LicenseNumber = User("IdentityLicenseNumber");
    identity.Email = User("IdentityMail");
    identity.Phone = User("IdentityPhone");
});

var userItemSecureNoteId = await CreateAsync(new CipherItemModel(userCrypto) { Name = User("SharpWardenTestSecureNote"), Notes = User("SecureNoteNotes") }, item =>
{
    item.CreateSecureNote().Type = SecureNoteType.Generic;
});

var userItemSSHKeyId = await CreateAsync(new CipherItemModel(userCrypto) { Name = User("SharpWardenTestSSHKey"), Notes = User("SSHKeyNotes") }, item =>
{
    var sshKey = item.CreateSshKey();
    sshKey.KeyFingerprint = User("SHA256:dbBC+ec5IlSfcufRZnalIk2q5ISTup3nff6mdjAccVI");
    sshKey.PublicKey = User("ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIKoMZebfc4izVVC0pjt9KNAtpVQ15X1MgpdzjloRAP3N");
    sshKey.PrivateKey = User("-----BEGIN OPENSSH PRIVATE KEY-----\nb3BlbnNzaC1rZXktdjEAAAAABG5vbmUAAAAEbm9uZQAAAAAAAAABAAAAMwAAAAtzc2gtZW\nQyNTUxOQAAACCqDGXm33OIs1VQtKY7fSjQLaVUNeV9TIKXc45aEQD9zQAAAIhBcXK7QXFy\nuwAAAAtzc2gtZWQyNTUxOQAAACCqDGXm33OIs1VQtKY7fSjQLaVUNeV9TIKXc45aEQD9zQ\nAAAEAZBznOkm7Q61yxXzyWOmVw+OPn7eKOBpcf4cELcPnPnKoMZebfc4izVVC0pjt9KNAt\npVQ15X1MgpdzjloRAP3NAAAAAAECAwQF\n-----END OPENSSH PRIVATE KEY-----\n");
});

var userItemCustomFieldsId = await CreateAsync(new CipherItemModel(userCrypto) { Name = User("SharpWardenCustomFields"), Notes = User("CustomFields") }, item =>
{
    item.CreateLogin();
    item.Fields =
    [
        new CustomFieldModel(userCrypto) { Name = User("TextField"), Value = User("TextFieldValue"), Type = CustomFieldType.Text },
        new CustomFieldModel(userCrypto) { Name = User("HiddenField"), Value = User("HiddenFieldValue"), Type = CustomFieldType.Hidden },
        new CustomFieldModel(userCrypto) { Name = User("CheckBoxField"), Value = User("true"), Type = CustomFieldType.Boolean },
        new CustomFieldModel(userCrypto) { Name = User("LinkedFieldLoginUserName"), Type = CustomFieldType.Linked, LinkedId = CustomFieldLinkType.LoginUsername },
        new CustomFieldModel(userCrypto) { Name = User("LinkedFieldLoginPassword"), Type = CustomFieldType.Linked, LinkedId = CustomFieldLinkType.LoginPassword },
    ];
});

var organizationItemId = await CreateAsync(new CipherItemModel(organizationCrypto)
{
    Name = Encrypt(organizationCrypto, "SharpWardenOrganizationEntry"),
    OrganizationId = organizationId,
    CollectionsIds = [collectionId],
}, item =>
{
    var login = item.CreateLogin();
    login.Username = Encrypt(organizationCrypto, "OrganizationUsername");
    login.Password = Encrypt(organizationCrypto, "OrganizationPassword");
    login.Totp = Encrypt(organizationCrypto, "OrganizationAuthenticatorKey");
});

// 7. Write the configuration used by the tests.
new TestConfig
{
    HostUrl = hostUrl,
    NotificationUrl = hostUrl + "/notifications",
    Account = new TestConfig.TestAccount
    {
        User = $"user.{webClient.UserId}",
        Secret = apiKey,
        Password = password,
    },
    UserItemId = userItemId.ToString(),
    UserFolderId = folder.Id.Value.ToString(),
    UserItemInFolderId = userItemInFolderId.ToString(),
    UserItemLoginId = userItemLoginId.ToString(),
    UserItemCardId = userItemCardId.ToString(),
    UserItemIdentityId = userItemIdentityId.ToString(),
    UserItemSecureNoteId = userItemSecureNoteId.ToString(),
    UserItemSSHKeyId = userItemSSHKeyId.ToString(),
    UserItemCustomFieldsId = userItemCustomFieldsId.ToString(),
    OrganizationId = organizationId.ToString(),
    OrganizationCollectionId = collectionId.ToString(),
    OrganizationItemId = organizationItemId.ToString(),
}.Save(outputPath);

Console.WriteLine($"Test configuration written to {Path.GetFullPath(outputPath)}");
return 0;

static async Task WaitForServerAsync(HttpClient http)
{
    for (var attempt = 0; attempt < 60; ++attempt)
    {
        try
        {
            using var response = await http.GetAsync("alive");
            if (response.IsSuccessStatusCode)
                return;
        }
        catch (HttpRequestException)
        {
            // Not listening yet.
        }

        await Task.Delay(TimeSpan.FromSeconds(1));
    }

    throw new TimeoutException("Vaultwarden did not start.");
}

static async Task<string> PostAsync(HttpClient http, string path, JObject body)
{
    using var response = await http.PostAsync(path, new StringContent(body.ToString(), Encoding.UTF8, "application/json"));
    var content = await response.Content.ReadAsStringAsync();
    if (!response.IsSuccessStatusCode)
        throw new HttpRequestException($"POST {path} failed: {(int)response.StatusCode} {content}");

    return content;
}

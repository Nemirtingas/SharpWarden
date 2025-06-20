using Microsoft.Extensions.DependencyInjection;
using SharpWarden.BitWardenDatabaseSession.Models.CipherItem;
using SharpWarden.BitWardenDatabaseSession.Services;
using System.Text;

namespace Tests;

[TestClass]
public sealed class SharpWardenTest
{
    const string SharpWardenTestItemId = "1de1488c-f980-446a-bb5a-b30201265744";
    const string SharpWardenTestFolderId = "01c15505-4683-48c2-a3fb-b302013ba7a0";
    const string SharpWardenTestItemInFolderId = "348270df-0c8e-4e05-8997-b30201427190";
    const string TestItemLoginId = "a65b2d2d-cd0a-4640-a778-b302014444c9";
    const string TestItemCardId = "1db665ae-2e24-426c-83d8-b302014703a5";
    const string TestItemIdentityId = "e5f3057c-1728-41a6-96dd-b302014a783c";
    const string TestItemSecureNoteId = "be880b58-d562-4c4a-8abf-b302015348b6";
    const string TestItemSSHKeyId = "5221df17-19e4-408b-b27f-b30201547789";

    private static IServiceScope DatabaseSessionScope;
    private static SharpWarden.WebClient.WebClient VaultWebClient;
    private static IVaultService VaultService;
    private static IUserCryptoService UserCryptoService;
    private static string TestAccountEmail;
    private static string TestAccountPassword;


    static SharpWardenTest()
    {
        DatabaseSessionScope = SharpWarden.BitWardenHelper.CreateSessionScope("https://vault.bitwarden.eu");
        VaultWebClient = DatabaseSessionScope.ServiceProvider.GetRequiredService<SharpWarden.WebClient.WebClient>();
        VaultService = DatabaseSessionScope.ServiceProvider.GetRequiredService<IVaultService>();
        UserCryptoService = DatabaseSessionScope.ServiceProvider.GetRequiredService<IUserCryptoService>();
        TestAccountEmail = Environment.GetEnvironmentVariable("SHARP_WARDEN_TEST_ACCOUNT_NAME");
        TestAccountPassword = Environment.GetEnvironmentVariable("SHARP_WARDEN_TEST_ACCOUNT_PASSWORD");
    }

    [TestMethod]
    public async Task _0001_TestCredentialsLogin()
    {
        Assert.IsFalse(string.IsNullOrWhiteSpace(TestAccountEmail));
        Assert.IsFalse(string.IsNullOrWhiteSpace(TestAccountPassword));

        await VaultWebClient.PreloginAsync(TestAccountEmail);

        // Authenticating with credentials triggers a mail notification.
        // TODO: API key authentication
        await VaultWebClient.AuthenticateAsync(TestAccountPassword);
        //await VaultWebClient.AuthenticateWithRefreshTokenAsync(refreshToken);

        
    }

    [TestMethod]
    public async Task _0002_TestLoadDatabaseAsync()
    {
        VaultService.LoadBitWardenDatabase(
            Encoding.UTF8.GetBytes(TestAccountPassword),
            VaultWebClient.UserKdfIterations,
            await VaultWebClient.GetDatabaseAsync());
    }

    [TestMethod]
    public async Task _0003_TestCipherItemAsync()
    {
        var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(SharpWardenTestItemId));

        Assert.IsNotNull(item);
        Assert.AreEqual(item.Id.ToString(), SharpWardenTestItemId);
        Assert.AreEqual(item.Name.ClearString, "SharpWardenTestItem");
        Assert.AreEqual(item.Login.Username.ClearString, "TestUserName");
        Assert.AreEqual(item.Login.Password.ClearString, "TestPassword");
    }

    [TestMethod]
    public async Task _0004_TestFolderItemAsync()
    {
        var folder = VaultService.GetBitWardenDatabase().Folders.Find(e => e.Id == Guid.Parse(SharpWardenTestFolderId));

        Assert.IsNotNull(folder);
        Assert.AreEqual(folder.Id.ToString(), SharpWardenTestFolderId);
        Assert.AreEqual(folder.Name.ClearString, "SharpWardenTestFolder");
    }

    [TestMethod]
    public async Task _0005_TestItemInFolderAsync()
    {
        var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(SharpWardenTestItemInFolderId));

        Assert.IsNotNull(item);
        Assert.AreEqual(item.Id.ToString(), SharpWardenTestItemInFolderId);
        Assert.AreEqual(item.Name.ClearString, "SharpWardenTestItemInFolder");
        Assert.AreEqual(item.Login.Username.ClearString, "FolderUser");
        Assert.AreEqual(item.Login.Password.ClearString, "FolderPassword");
        Assert.AreEqual(item.FolderId, Guid.Parse(SharpWardenTestFolderId));
    }

    [TestMethod]
    public async Task _0006_TestItemLoginAsync()
    {
        var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(TestItemLoginId));

        Assert.IsNotNull(item);
        Assert.AreEqual(item.Id.ToString(), TestItemLoginId);
        Assert.AreEqual(item.ItemType, CipherItemType.Login);
        Assert.AreEqual(item.Notes.ClearString, "LoginNotes");

        Assert.AreEqual(item.Login.Username.ClearString, "LoginUsername");
        Assert.AreEqual(item.Login.Password.ClearString, "LoginPassword");

        Assert.AreEqual(item.Login.Uris[0].Uri.ClearString, "LoginUrlDefault");
        Assert.AreEqual(item.Login.Uris[0].Match, null);

        Assert.AreEqual(item.Login.Uris[1].Uri.ClearString, "LoginUrlBaseDomain");
        Assert.AreEqual(item.Login.Uris[1].Match, UriMatchType.Domain);

        Assert.AreEqual(item.Login.Uris[2].Uri.ClearString, "LoginUrlHost");
        Assert.AreEqual(item.Login.Uris[2].Match, UriMatchType.Host);

        Assert.AreEqual(item.Login.Uris[3].Uri.ClearString, "LoginUrlStartsWith");
        Assert.AreEqual(item.Login.Uris[3].Match, UriMatchType.BeginWith);

        Assert.AreEqual(item.Login.Uris[4].Uri.ClearString, "LoginUrlRegularExpression");
        Assert.AreEqual(item.Login.Uris[4].Match, UriMatchType.Regex);

        Assert.AreEqual(item.Login.Uris[5].Uri.ClearString, "LoginUrlExact");
        Assert.AreEqual(item.Login.Uris[5].Match, UriMatchType.Exact);

        Assert.AreEqual(item.Login.Uris[6].Uri.ClearString, "LoginUrlNever");
        Assert.AreEqual(item.Login.Uris[6].Match, UriMatchType.Never);
    }

    [TestMethod]
    public async Task _0007_TestItemCardAsync()
    {
        var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(TestItemCardId));

        Assert.IsNotNull(item);
        Assert.AreEqual(item.Id.ToString(), TestItemCardId);
        Assert.AreEqual(item.ItemType, CipherItemType.Card);
        Assert.AreEqual(item.Notes.ClearString, "CardNotes");

        Assert.AreEqual(item.Card.CardholderName.ClearString, "CardholderName");
        Assert.AreEqual(item.Card.Number.ClearString, "CardNumber");
        Assert.AreEqual(item.Card.ExpYear.ClearString, "2025");
        Assert.AreEqual(item.Card.ExpMonth.ClearString, "6");
        Assert.AreEqual(item.Card.Code.ClearString, "CardSecurityCode");
        Assert.AreEqual(item.Card.Brand.ClearString, "Visa");
    }

    [TestMethod]
    public async Task _0008_TestItemIdentityAsync()
    {
        var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(TestItemIdentityId));

        Assert.IsNotNull(item);
        Assert.AreEqual(item.Id.ToString(), TestItemIdentityId);
        Assert.AreEqual(item.ItemType, CipherItemType.Identity);
        Assert.AreEqual(item.Notes.ClearString, "IdentityNotes");

        Assert.AreEqual(item.Identity.Address1.ClearString, "IdentityAddress1");
        Assert.AreEqual(item.Identity.Address2.ClearString, "IdentityAddress2");
        Assert.AreEqual(item.Identity.Address3.ClearString, "IdentityAddress3");
        Assert.AreEqual(item.Identity.City.ClearString, "IdentityCity");
        Assert.AreEqual(item.Identity.State.ClearString, "IdentityState");
        Assert.AreEqual(item.Identity.PostalCode.ClearString, "IdentityPostalCode");
        Assert.AreEqual(item.Identity.Country.ClearString, "IdentityCountry");
        Assert.AreEqual(item.Identity.FirstName.ClearString, "IdentityFirstName");
        Assert.AreEqual(item.Identity.MiddleName.ClearString, "IdentityMiddleName");
        Assert.AreEqual(item.Identity.LastName.ClearString, "IdentityLastName");
        Assert.AreEqual(item.Identity.Username.ClearString, "IdentityUserName");
        Assert.AreEqual(item.Identity.Company.ClearString, "IdentityCompany");
        Assert.AreEqual(item.Identity.SSN.ClearString, "IdentitySocialSecurityNumber");
        Assert.AreEqual(item.Identity.PassportNumber.ClearString, "IdentityPassportNumber");
        Assert.AreEqual(item.Identity.LicenseNumber.ClearString, "IdentityLicenseNumber");
        Assert.AreEqual(item.Identity.Email.ClearString, "IdentityMail");
        Assert.AreEqual(item.Identity.Phone.ClearString, "IdentityPhone");
    }

    [TestMethod]
    public async Task _0009_TestItemSecureNoteAsync()
    {
        var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(TestItemSecureNoteId));

        Assert.IsNotNull(item);
        Assert.AreEqual(item.Id.ToString(), TestItemSecureNoteId);
        Assert.AreEqual(item.ItemType, CipherItemType.SecureNote);
        Assert.AreEqual(item.Notes.ClearString, "SecureNoteNotes");

        Assert.AreEqual(item.SecureNote.Type, SecureNoteType.Generic);
    }

    [TestMethod]
    public async Task _0010_TestItemSSHKeyAsync()
    {
        var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(TestItemSSHKeyId));

        Assert.IsNotNull(item);
        Assert.AreEqual(item.Id.ToString(), TestItemSSHKeyId);
        Assert.AreEqual(item.ItemType, CipherItemType.SSHKey);
        Assert.AreEqual(item.Notes.ClearString, "SSHKeyNotes");

        Assert.AreEqual(item.SSHKey.KeyFingerprint.ClearString, "SHA256:dbBC+ec5IlSfcufRZnalIk2q5ISTup3nff6mdjAccVI");
        Assert.AreEqual(item.SSHKey.PublicKey.ClearString, "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIKoMZebfc4izVVC0pjt9KNAtpVQ15X1MgpdzjloRAP3N");
        Assert.AreEqual(item.SSHKey.PrivateKey.ClearString, "-----BEGIN OPENSSH PRIVATE KEY-----\nb3BlbnNzaC1rZXktdjEAAAAABG5vbmUAAAAEbm9uZQAAAAAAAAABAAAAMwAAAAtzc2gtZW\nQyNTUxOQAAACCqDGXm33OIs1VQtKY7fSjQLaVUNeV9TIKXc45aEQD9zQAAAIhBcXK7QXFy\nuwAAAAtzc2gtZWQyNTUxOQAAACCqDGXm33OIs1VQtKY7fSjQLaVUNeV9TIKXc45aEQD9zQ\nAAAEAZBznOkm7Q61yxXzyWOmVw+OPn7eKOBpcf4cELcPnPnKoMZebfc4izVVC0pjt9KNAt\npVQ15X1MgpdzjloRAP3NAAAAAAECAwQF\n-----END OPENSSH PRIVATE KEY-----\n");
    }
}

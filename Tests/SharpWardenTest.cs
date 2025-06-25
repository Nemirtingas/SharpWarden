using Microsoft.Extensions.DependencyInjection;
using SharpWarden.BitWardenDatabaseSession.Models.CipherItem;
using SharpWarden.BitWardenDatabaseSession.Services;
using System.Text;

namespace Tests;

[TestClass]
public sealed class SharpWardenTest
{
    const string TestUserItemId = "1de1488c-f980-446a-bb5a-b30201265744";
    const string TestUserFolderId = "01c15505-4683-48c2-a3fb-b302013ba7a0";
    const string TestUserItemInFolderId = "348270df-0c8e-4e05-8997-b30201427190";
    const string TestUserItemLoginId = "a65b2d2d-cd0a-4640-a778-b302014444c9";
    const string TestUserItemCardId = "1db665ae-2e24-426c-83d8-b302014703a5";
    const string TestUserItemIdentityId = "e5f3057c-1728-41a6-96dd-b302014a783c";
    const string TestUserItemSecureNoteId = "be880b58-d562-4c4a-8abf-b302015348b6";
    const string TestUserItemSSHKeyId = "5221df17-19e4-408b-b27f-b30201547789";
    const string TestUserItemCustomFieldsId = "c59db8cb-3036-49cb-a16d-b307006c213d";

    const string TestOrganizationId = "9dab2223-5ee2-480d-ace2-b3020126075b";
    const string TestOrganizationItemId = "2c94bf3f-fcfe-48eb-b19c-b307006b69be";

    private static IServiceScope DatabaseSessionScope;
    private static SharpWarden.WebClient.WebClient VaultWebClient;
    private static IVaultService VaultService;
    private static IUserCryptoService UserCryptoService;
    private static string TestAccountUser;
    private static string TestAccountSecret;
    private static string TestAccountPassword;


    static SharpWardenTest()
    {
        DatabaseSessionScope = SharpWarden.BitWardenHelper.CreateSessionScope(SharpWarden.WebClient.WebClient.BitWardenEUHostUrl);
        VaultWebClient = DatabaseSessionScope.ServiceProvider.GetRequiredService<SharpWarden.WebClient.WebClient>();
        VaultService = DatabaseSessionScope.ServiceProvider.GetRequiredService<IVaultService>();
        UserCryptoService = DatabaseSessionScope.ServiceProvider.GetRequiredService<IUserCryptoService>();
        TestAccountUser = Environment.GetEnvironmentVariable("SHARP_WARDEN_TEST_ACCOUNT_USER");
        TestAccountSecret = Environment.GetEnvironmentVariable("SHARP_WARDEN_TEST_ACCOUNT_SECRET");
        TestAccountPassword = Environment.GetEnvironmentVariable("SHARP_WARDEN_TEST_ACCOUNT_PASSWORD");
    }

    // This test works, but you need to send it back the OTP code, which is obviously impossible in github actions.
    //[TestMethod]
    //public async Task _0001_TestCredentialsLogin()
    //{
    //    Assert.IsFalse(string.IsNullOrWhiteSpace(TestAccountUser));
    //    Assert.IsFalse(string.IsNullOrWhiteSpace(TestAccountPassword));
    //
    //    await VaultWebClient.PreloginAsync(TestAccountUser);
    //
    //    // Authenticating with credentials triggers a mail notification.
    //    await VaultWebClient.AuthenticateAsync(TestAccountPassword);
    //}

    [TestMethod]
    public async Task _0001_TestApiKeyLogin()
    {
        Assert.IsFalse(string.IsNullOrWhiteSpace(TestAccountUser));
        Assert.IsFalse(string.IsNullOrWhiteSpace(TestAccountSecret));
    
        await VaultWebClient.AuthenticateWithApiKeyAsync(TestAccountUser, TestAccountSecret);
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
    public async Task _1003_TestUserCipherItemAsync()
    {
        var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(TestUserItemId));

        Assert.IsNotNull(item);
        Assert.AreEqual(item.Id.ToString(), TestUserItemId);
        Assert.AreEqual(item.Reprompt, CipherRepromptType.Password);
        Assert.AreEqual(item.Name.ClearString, "SharpWardenTestItem");
        Assert.AreEqual(item.Login.Username.ClearString, "TestUserName");
        Assert.AreEqual(item.Login.Password.ClearString, "TestPassword");
    }

    [TestMethod]
    public async Task _1004_TestUserFolderItemAsync()
    {
        var folder = VaultService.GetBitWardenDatabase().Folders.Find(e => e.Id == Guid.Parse(TestUserFolderId));

        Assert.IsNotNull(folder);
        Assert.AreEqual(folder.Id.ToString(), TestUserFolderId);
        Assert.AreEqual(folder.Name.ClearString, "SharpWardenTestFolder");
    }

    [TestMethod]
    public async Task _1005_TestUserItemInFolderAsync()
    {
        var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(TestUserItemInFolderId));

        Assert.IsNotNull(item);
        Assert.AreEqual(item.Id.ToString(), TestUserItemInFolderId);
        Assert.AreEqual(item.Name.ClearString, "SharpWardenTestItemInFolder");
        Assert.AreEqual(item.Login.Username.ClearString, "FolderUser");
        Assert.AreEqual(item.Login.Password.ClearString, "FolderPassword");
        Assert.AreEqual(item.FolderId, Guid.Parse(TestUserFolderId));
    }

    [TestMethod]
    public async Task _1006_TestUserItemLoginAsync()
    {
        var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(TestUserItemLoginId));

        Assert.IsNotNull(item);
        Assert.AreEqual(item.Id.ToString(), TestUserItemLoginId);
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
    public async Task _1007_TestUserItemCardAsync()
    {
        var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(TestUserItemCardId));

        Assert.IsNotNull(item);
        Assert.AreEqual(item.Id.ToString(), TestUserItemCardId);
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
    public async Task _1008_TestUserItemIdentityAsync()
    {
        var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(TestUserItemIdentityId));

        Assert.IsNotNull(item);
        Assert.AreEqual(item.Id.ToString(), TestUserItemIdentityId);
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
    public async Task _1009_TestUserItemSecureNoteAsync()
    {
        var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(TestUserItemSecureNoteId));

        Assert.IsNotNull(item);
        Assert.AreEqual(item.Id.ToString(), TestUserItemSecureNoteId);
        Assert.AreEqual(item.ItemType, CipherItemType.SecureNote);
        Assert.AreEqual(item.Notes.ClearString, "SecureNoteNotes");

        Assert.AreEqual(item.SecureNote.Type, SecureNoteType.Generic);
    }

    [TestMethod]
    public async Task _1010_TestUserItemSSHKeyAsync()
    {
        var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(TestUserItemSSHKeyId));

        Assert.IsNotNull(item);
        Assert.AreEqual(item.Id.ToString(), TestUserItemSSHKeyId);
        Assert.AreEqual(item.ItemType, CipherItemType.SSHKey);
        Assert.AreEqual(item.Notes.ClearString, "SSHKeyNotes");

        Assert.AreEqual(item.SSHKey.KeyFingerprint.ClearString, "SHA256:dbBC+ec5IlSfcufRZnalIk2q5ISTup3nff6mdjAccVI");
        Assert.AreEqual(item.SSHKey.PublicKey.ClearString, "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIKoMZebfc4izVVC0pjt9KNAtpVQ15X1MgpdzjloRAP3N");
        Assert.AreEqual(item.SSHKey.PrivateKey.ClearString, "-----BEGIN OPENSSH PRIVATE KEY-----\nb3BlbnNzaC1rZXktdjEAAAAABG5vbmUAAAAEbm9uZQAAAAAAAAABAAAAMwAAAAtzc2gtZW\nQyNTUxOQAAACCqDGXm33OIs1VQtKY7fSjQLaVUNeV9TIKXc45aEQD9zQAAAIhBcXK7QXFy\nuwAAAAtzc2gtZWQyNTUxOQAAACCqDGXm33OIs1VQtKY7fSjQLaVUNeV9TIKXc45aEQD9zQ\nAAAEAZBznOkm7Q61yxXzyWOmVw+OPn7eKOBpcf4cELcPnPnKoMZebfc4izVVC0pjt9KNAt\npVQ15X1MgpdzjloRAP3NAAAAAAECAwQF\n-----END OPENSSH PRIVATE KEY-----\n");
    }

    [TestMethod]
    public async Task _1011_TestUserItemCustomFieldsAsync()
    {
        var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(TestUserItemCustomFieldsId));

        Assert.IsNotNull(item);
        Assert.AreEqual(item.Id.ToString(), TestUserItemCustomFieldsId);
        Assert.AreEqual(item.Name.ClearString, "SharpWardenCustomFields");
        Assert.AreEqual(item.ItemType, CipherItemType.Login);
        Assert.AreEqual(item.Notes.ClearString, "CustomFields");

        Assert.AreEqual(item.Fields.Count, 5);

        Assert.AreEqual(item.Fields[0].Name.ClearString, "TextField");
        Assert.AreEqual(item.Fields[0].Value.ClearString, "TextFieldValue");
        Assert.AreEqual(item.Fields[0].Type, CustomFieldType.Text);

        Assert.AreEqual(item.Fields[1].Name.ClearString, "HiddenField");
        Assert.AreEqual(item.Fields[1].Value.ClearString, "HiddenFieldValue");
        Assert.AreEqual(item.Fields[1].Type, CustomFieldType.Hidden);

        Assert.AreEqual(item.Fields[2].Name.ClearString, "CheckBoxField");
        Assert.AreEqual(item.Fields[2].Value.ClearString, "true");
        Assert.AreEqual(item.Fields[2].Type, CustomFieldType.Boolean);

        Assert.AreEqual(item.Fields[3].Name.ClearString, "LinkedFieldLoginUserName");
        Assert.AreEqual(item.Fields[3].LinkedId, CustomFieldLinkType.LoginUsername);
        Assert.AreEqual(item.Fields[3].Type, CustomFieldType.Linked);

        Assert.AreEqual(item.Fields[4].Name.ClearString, "LinkedFieldLoginPassword");
        Assert.AreEqual(item.Fields[4].LinkedId, CustomFieldLinkType.LoginPassword);
        Assert.AreEqual(item.Fields[4].Type, CustomFieldType.Linked);
    }

    [TestMethod]
    public async Task _2001_TestOrganizationCipherItemAsync()
    {
        var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(TestOrganizationItemId) && e.OrganizationId == Guid.Parse(TestOrganizationId));

        Assert.IsNotNull(item);
        Assert.AreEqual(item.Id.ToString(), TestOrganizationItemId);
        Assert.AreEqual(item.Name.ClearString, "SharpWardenOrganizationEntry");
        Assert.AreEqual(item.Login.Username.ClearString, "OrganizationUsername");
        Assert.AreEqual(item.Login.Password.ClearString, "OrganizationPassword");
        Assert.AreEqual(item.Login.TOTP.ClearString, "OrganizationAuthenticatorKey");
    }
}

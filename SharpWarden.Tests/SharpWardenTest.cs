using Microsoft.Extensions.DependencyInjection;
using SharpWarden;
using SharpWarden.BitWardenDatabaseSession.Models.CipherItem;
using SharpWarden.BitWardenDatabaseSession.Models.CollectionItem;
using SharpWarden.BitWardenDatabaseSession.Models.FolderItem;
using SharpWarden.BitWardenDatabaseSession.Services;
using SharpWarden.NotificationClient;
using SharpWarden.NotificationClient.Models;
using SharpWarden.NotificationClient.Services;
using SharpWarden.WebClient.Services;
using System.Text;

namespace SharpWarden.Tests;

[TestClass]
public sealed class SharpWardenTest
{
    // Server and test data ids: see TestConfig (TestConfig.json for the Bitwarden EU account,
    // or the file written by SharpWarden.Tests.Seed for a local Vaultwarden).
    private static readonly TestConfig Config = TestConfig.Load();

    static readonly string TestUserItemId = Config.UserItemId;
    static readonly string TestUserFolderId = Config.UserFolderId;
    static readonly string TestUserItemInFolderId = Config.UserItemInFolderId;
    static readonly string TestUserItemLoginId = Config.UserItemLoginId;
    static readonly string TestUserItemCardId = Config.UserItemCardId;
    static readonly string TestUserItemIdentityId = Config.UserItemIdentityId;
    static readonly string TestUserItemSecureNoteId = Config.UserItemSecureNoteId;
    static readonly string TestUserItemSSHKeyId = Config.UserItemSSHKeyId;
    static readonly string TestUserItemCustomFieldsId = Config.UserItemCustomFieldsId;

    static readonly string TestOrganizationId = Config.OrganizationId;
    static readonly string TestOrganizationCollectionId = Config.OrganizationCollectionId;
    static readonly string TestOrganizationItemId = Config.OrganizationItemId;

    private static IServiceScope DatabaseSessionScope;
    private static IWebClientService VaultWebClient;
    private static IVaultService VaultService;
    private static IUserCryptoService UserCryptoService;
    private static IOrganizationCryptoFactoryService OrganizationCryptoFactoryService;
    private static string TestAccountUser;
    private static string TestAccountSecret;
    private static string TestAccountPassword;


    static SharpWardenTest()
    {
        TestAccountUser = GetSetting("SHARP_WARDEN_TEST_ACCOUNT_USER", Config.Account?.User);
        TestAccountSecret = GetSetting("SHARP_WARDEN_TEST_ACCOUNT_SECRET", Config.Account?.Secret);
        TestAccountPassword = GetSetting("SHARP_WARDEN_TEST_ACCOUNT_PASSWORD", Config.Account?.Password);

        DatabaseSessionScope = SharpWarden.BitWardenHelper.CreateSessionScope(Config.HostUrl, Config.NotificationUrl, GetTestDeviceId());
        VaultWebClient = DatabaseSessionScope.ServiceProvider.GetRequiredService<IWebClientService>();
        VaultService = DatabaseSessionScope.ServiceProvider.GetRequiredService<IVaultService>();
        UserCryptoService = DatabaseSessionScope.ServiceProvider.GetRequiredService<IUserCryptoService>();
        OrganizationCryptoFactoryService = DatabaseSessionScope.ServiceProvider.GetRequiredService<IOrganizationCryptoFactoryService>();
    }

    // Environment variable when set (CI secrets), otherwise the value from the configuration file.
    private static string GetSetting(string environmentVariable, string? configValue)
    {
        var value = Environment.GetEnvironmentVariable(environmentVariable);
        return string.IsNullOrWhiteSpace(value) ? configValue! : value;
    }

    /// <summary>
    /// Stable device id, so the server does not see a new device (and send a new device mail) on every run.
    /// Uses SHARP_WARDEN_TEST_DEVICE_ID if set, otherwise derives it from the test account credentials
    /// (it must not be hardcoded: a known device id skips the new device verification).
    /// </summary>
    private static Guid GetTestDeviceId()
    {
        if (Guid.TryParse(Environment.GetEnvironmentVariable("SHARP_WARDEN_TEST_DEVICE_ID"), out var deviceId))
            return deviceId;

        var hash = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes($"SharpWarden.Tests.DeviceId|{TestAccountUser}|{TestAccountSecret}"));
        return new Guid(hash.AsSpan(0, 16));
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
            VaultWebClient.UserKdf,
            await VaultWebClient.GetDatabaseAsync());
    }

    [TestMethod]
    public async Task _0103_TestUserCipherItemAsync()
    {
        var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(TestUserItemId));

        Assert.IsNotNull(item);
        Assert.AreEqual(TestUserItemId, item.Id.ToString());
        Assert.AreEqual(CipherRepromptType.Password, item.Reprompt);
        Assert.AreEqual("SharpWardenTestItem", item.Name.ClearString);
        Assert.AreEqual("TestUserName", item.Login.Username.ClearString);
        Assert.AreEqual("TestPassword", item.Login.Password.ClearString);
    }

    [TestMethod]
    public async Task _0104_TestUserFolderItemAsync()
    {
        var folder = VaultService.GetBitWardenDatabase().Folders.Find(e => e.Id == Guid.Parse(TestUserFolderId));

        Assert.IsNotNull(folder);
        Assert.AreEqual(TestUserFolderId, folder.Id.ToString());
        Assert.AreEqual("SharpWardenTestFolder", folder.Name.ClearString);
    }

    [TestMethod]
    public async Task _0105_TestUserItemInFolderAsync()
    {
        var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(TestUserItemInFolderId));

        Assert.IsNotNull(item);
        Assert.AreEqual(TestUserItemInFolderId, item.Id.ToString());
        Assert.AreEqual("SharpWardenTestItemInFolder", item.Name.ClearString);
        Assert.AreEqual("FolderUser", item.Login.Username.ClearString);
        Assert.AreEqual("FolderPassword", item.Login.Password.ClearString);
        Assert.AreEqual(item.FolderId, Guid.Parse(TestUserFolderId));
    }

    [TestMethod]
    public async Task _0106_TestUserItemLoginAsync()
    {
        var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(TestUserItemLoginId));

        Assert.IsNotNull(item);
        Assert.AreEqual(TestUserItemLoginId, item.Id.ToString());
        Assert.AreEqual(CipherItemType.Login, item.ItemType);
        Assert.AreEqual("LoginNotes", item.Notes.ClearString);

        Assert.AreEqual("LoginUsername", item.Login.Username.ClearString);
        Assert.AreEqual("LoginPassword", item.Login.Password.ClearString);

        Assert.AreEqual("LoginUrlDefault", item.Login.Uris[0].Uri.ClearString);
        Assert.AreEqual(null, item.Login.Uris[0].Match);

        Assert.AreEqual("LoginUrlBaseDomain", item.Login.Uris[1].Uri.ClearString);
        Assert.AreEqual(UriMatchType.Domain, item.Login.Uris[1].Match);

        Assert.AreEqual("LoginUrlHost", item.Login.Uris[2].Uri.ClearString);
        Assert.AreEqual(UriMatchType.Host, item.Login.Uris[2].Match);

        Assert.AreEqual("LoginUrlStartsWith", item.Login.Uris[3].Uri.ClearString);
        Assert.AreEqual(UriMatchType.BeginWith, item.Login.Uris[3].Match);

        Assert.AreEqual("LoginUrlRegularExpression", item.Login.Uris[4].Uri.ClearString);
        Assert.AreEqual(UriMatchType.Regex, item.Login.Uris[4].Match);

        Assert.AreEqual("LoginUrlExact", item.Login.Uris[5].Uri.ClearString);
        Assert.AreEqual(UriMatchType.Exact, item.Login.Uris[5].Match);

        Assert.AreEqual("LoginUrlNever", item.Login.Uris[6].Uri.ClearString);
        Assert.AreEqual(UriMatchType.Never, item.Login.Uris[6].Match);
    }

    [TestMethod]
    public async Task _0107_TestUserItemCardAsync()
    {
        var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(TestUserItemCardId));

        Assert.IsNotNull(item);
        Assert.AreEqual(TestUserItemCardId, item.Id.ToString());
        Assert.AreEqual(CipherItemType.Card, item.ItemType);
        Assert.AreEqual("CardNotes", item.Notes.ClearString);

        Assert.AreEqual("CardholderName", item.Card.CardholderName.ClearString);
        Assert.AreEqual("CardNumber", item.Card.Number.ClearString);
        Assert.AreEqual("2025", item.Card.ExpYear.ClearString);
        Assert.AreEqual("6", item.Card.ExpMonth.ClearString);
        Assert.AreEqual("CardSecurityCode", item.Card.Code.ClearString);
        Assert.AreEqual("Visa", item.Card.Brand.ClearString);
    }

    [TestMethod]
    public async Task _0108_TestUserItemIdentityAsync()
    {
        var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(TestUserItemIdentityId));

        Assert.IsNotNull(item);
        Assert.AreEqual(TestUserItemIdentityId, item.Id.ToString());
        Assert.AreEqual(CipherItemType.Identity, item.ItemType);
        Assert.AreEqual("IdentityNotes", item.Notes.ClearString);

        Assert.AreEqual("IdentityAddress1", item.Identity.Address1.ClearString);
        Assert.AreEqual("IdentityAddress2", item.Identity.Address2.ClearString);
        Assert.AreEqual("IdentityAddress3", item.Identity.Address3.ClearString);
        Assert.AreEqual("IdentityCity", item.Identity.City.ClearString);
        Assert.AreEqual("IdentityState", item.Identity.State.ClearString);
        Assert.AreEqual("IdentityPostalCode", item.Identity.PostalCode.ClearString);
        Assert.AreEqual("IdentityCountry", item.Identity.Country.ClearString);
        Assert.AreEqual("IdentityFirstName", item.Identity.FirstName.ClearString);
        Assert.AreEqual("IdentityMiddleName", item.Identity.MiddleName.ClearString);
        Assert.AreEqual("IdentityLastName", item.Identity.LastName.ClearString);
        Assert.AreEqual("IdentityUserName", item.Identity.Username.ClearString);
        Assert.AreEqual("IdentityCompany", item.Identity.Company.ClearString);
        Assert.AreEqual("IdentitySocialSecurityNumber", item.Identity.SecuritySocialNumber.ClearString);
        Assert.AreEqual("IdentityPassportNumber", item.Identity.PassportNumber.ClearString);
        Assert.AreEqual("IdentityLicenseNumber", item.Identity.LicenseNumber.ClearString);
        Assert.AreEqual("IdentityMail", item.Identity.Email.ClearString);
        Assert.AreEqual("IdentityPhone", item.Identity.Phone.ClearString);
    }

    [TestMethod]
    public async Task _0109_TestUserItemSecureNoteAsync()
    {
        var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(TestUserItemSecureNoteId));

        Assert.IsNotNull(item);
        Assert.AreEqual(TestUserItemSecureNoteId, item.Id.ToString());
        Assert.AreEqual(CipherItemType.SecureNote, item.ItemType);
        Assert.AreEqual("SecureNoteNotes", item.Notes.ClearString);

        Assert.AreEqual(SecureNoteType.Generic, item.SecureNote.Type);
    }

    [TestMethod]
    public async Task _0110_TestUserItemSSHKeyAsync()
    {
        var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(TestUserItemSSHKeyId));

        Assert.IsNotNull(item);
        Assert.AreEqual(TestUserItemSSHKeyId, item.Id.ToString());
        Assert.AreEqual(CipherItemType.SshKey, item.ItemType);
        Assert.AreEqual("SSHKeyNotes", item.Notes.ClearString);

        Assert.AreEqual("SHA256:dbBC+ec5IlSfcufRZnalIk2q5ISTup3nff6mdjAccVI", item.SshKey.KeyFingerprint.ClearString);
        Assert.AreEqual("ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIKoMZebfc4izVVC0pjt9KNAtpVQ15X1MgpdzjloRAP3N", item.SshKey.PublicKey.ClearString);
        Assert.AreEqual("-----BEGIN OPENSSH PRIVATE KEY-----\nb3BlbnNzaC1rZXktdjEAAAAABG5vbmUAAAAEbm9uZQAAAAAAAAABAAAAMwAAAAtzc2gtZW\nQyNTUxOQAAACCqDGXm33OIs1VQtKY7fSjQLaVUNeV9TIKXc45aEQD9zQAAAIhBcXK7QXFy\nuwAAAAtzc2gtZWQyNTUxOQAAACCqDGXm33OIs1VQtKY7fSjQLaVUNeV9TIKXc45aEQD9zQ\nAAAEAZBznOkm7Q61yxXzyWOmVw+OPn7eKOBpcf4cELcPnPnKoMZebfc4izVVC0pjt9KNAt\npVQ15X1MgpdzjloRAP3NAAAAAAECAwQF\n-----END OPENSSH PRIVATE KEY-----\n", item.SshKey.PrivateKey.ClearString);
    }

    [TestMethod]
    public async Task _0111_TestUserItemCustomFieldsAsync()
    {
        var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(TestUserItemCustomFieldsId));

        Assert.IsNotNull(item);
        Assert.AreEqual(TestUserItemCustomFieldsId, item.Id.ToString());
        Assert.AreEqual("SharpWardenCustomFields", item.Name.ClearString);
        Assert.AreEqual(CipherItemType.Login, item.ItemType);
        Assert.AreEqual("CustomFields", item.Notes.ClearString);

        Assert.AreEqual(5, item.Fields.Count);

        Assert.AreEqual("TextField", item.Fields[0].Name.ClearString);
        Assert.AreEqual("TextFieldValue", item.Fields[0].Value.ClearString);
        Assert.AreEqual(CustomFieldType.Text, item.Fields[0].Type);

        Assert.AreEqual("HiddenField", item.Fields[1].Name.ClearString);
        Assert.AreEqual("HiddenFieldValue", item.Fields[1].Value.ClearString);
        Assert.AreEqual(CustomFieldType.Hidden, item.Fields[1].Type);

        Assert.AreEqual("CheckBoxField", item.Fields[2].Name.ClearString);
        Assert.AreEqual("true", item.Fields[2].Value.ClearString);
        Assert.AreEqual(CustomFieldType.Boolean, item.Fields[2].Type);

        Assert.AreEqual("LinkedFieldLoginUserName", item.Fields[3].Name.ClearString);
        Assert.AreEqual(CustomFieldLinkType.LoginUsername, item.Fields[3].LinkedId);
        Assert.AreEqual(CustomFieldType.Linked, item.Fields[3].Type);

        Assert.AreEqual("LinkedFieldLoginPassword", item.Fields[4].Name.ClearString);
        Assert.AreEqual(CustomFieldLinkType.LoginPassword, item.Fields[4].LinkedId);
        Assert.AreEqual(CustomFieldType.Linked, item.Fields[4].Type);
    }

    [TestMethod]
    public async Task _0201_TestOrganizationCipherItemAsync()
    {
        var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(TestOrganizationItemId) && e.OrganizationId == Guid.Parse(TestOrganizationId));

        Assert.IsNotNull(item);
        Assert.AreEqual(TestOrganizationItemId, item.Id.ToString());
        Assert.AreEqual("SharpWardenOrganizationEntry", item.Name.ClearString);
        Assert.AreEqual("OrganizationUsername", item.Login.Username.ClearString);
        Assert.AreEqual("OrganizationPassword", item.Login.Password.ClearString);
        Assert.AreEqual("OrganizationAuthenticatorKey", item.Login.Totp.ClearString);
    }

    [TestMethod]
    public async Task _0301_TestUserCreateLoginItemAsync()
    {
        // TODO: Test for item existence before creating to not leave dangling tests

        var cipherItem = new CipherItemModel(UserCryptoService)
        {
            Name = new EncryptedString(UserCryptoService)
            {
                ClearString = "TestCreateUserLoginItem"
            },
            Favorite = true,
            Edit = true,
            Reprompt = CipherRepromptType.Password,
            FolderId = Guid.Parse(TestUserFolderId),
            Notes = new EncryptedString(UserCryptoService)
            {
                ClearString = "TestCreateUserNotes"
            },
            ViewPassword = true,
        };

        var loginItem = cipherItem.CreateLogin();
        loginItem.Username = new EncryptedString(UserCryptoService)
        {
            ClearString = "TestCreateUserLoginItemUsername"
        };
        loginItem.Password = new EncryptedString(UserCryptoService)
        {
            ClearString = "TestCreateUserLoginItemPassword"
        };
        loginItem.Totp = new EncryptedString(UserCryptoService)
        {
            ClearString = "TestCreateUserLoginItemTOTP"
        };

        var cipherItemSaved = await VaultWebClient.CreateCipherItemAsync(cipherItem);
        Assert.IsNotNull(cipherItemSaved?.Id);
        Assert.AreEqual(cipherItemSaved.Name.ClearString, cipherItem.Name.ClearString);
        Assert.AreEqual(cipherItemSaved.Name.CipherString, cipherItem.Name.CipherString);

        Assert.AreEqual(cipherItemSaved.Notes.ClearString, cipherItem.Notes.ClearString);
        Assert.AreEqual(cipherItemSaved.Notes.CipherString, cipherItem.Notes.CipherString);

        Assert.AreEqual(cipherItemSaved.Favorite, cipherItem.Favorite);
        Assert.AreEqual(cipherItemSaved.Edit, cipherItem.Edit);
        Assert.AreEqual(cipherItemSaved.Reprompt, cipherItem.Reprompt);
        Assert.AreEqual(cipherItemSaved.FolderId, cipherItem.FolderId);
        Assert.AreEqual(cipherItemSaved.ViewPassword, cipherItem.ViewPassword);

        Assert.AreEqual(cipherItemSaved.Login.Username.ClearString, cipherItem.Login.Username.ClearString);
        Assert.AreEqual(cipherItemSaved.Login.Username.CipherString, cipherItem.Login.Username.CipherString);

        Assert.AreEqual(cipherItemSaved.Login.Password.ClearString, cipherItem.Login.Password.ClearString);
        Assert.AreEqual(cipherItemSaved.Login.Password.CipherString, cipherItem.Login.Password.CipherString);

        Assert.AreEqual(cipherItemSaved.Login.Totp.ClearString, cipherItem.Login.Totp.ClearString);
        Assert.AreEqual(cipherItemSaved.Login.Totp.CipherString, cipherItem.Login.Totp.CipherString);

        await VaultWebClient.DeleteCipherItemAsync(cipherItemSaved.Id.Value);
    }

    [TestMethod]
    public async Task _0302_TestUserCreateSecureNoteItemAsync()
    {
        // TODO: Test for item existence before creating to not leave dangling tests

        var cipherItem = new CipherItemModel(UserCryptoService)
        {
            Name = new EncryptedString(UserCryptoService)
            {
                ClearString = "TestCreateUserSecureNoteItem"
            },
            Favorite = true,
            Edit = true,
            Reprompt = CipherRepromptType.Password,
            FolderId = Guid.Parse(TestUserFolderId),
            Notes = new EncryptedString(UserCryptoService)
            {
                ClearString = "TestCreateUserNotes"
            },
            ViewPassword = true,
        };

        var secureNoteItem = cipherItem.CreateSecureNote();
        secureNoteItem.Type = SecureNoteType.Generic;

        var cipherItemSaved = await VaultWebClient.CreateCipherItemAsync(cipherItem);
        Assert.IsNotNull(cipherItemSaved?.Id);
        Assert.AreEqual(cipherItemSaved.Name.ClearString, cipherItem.Name.ClearString);
        Assert.AreEqual(cipherItemSaved.Name.CipherString, cipherItem.Name.CipherString);

        Assert.AreEqual(cipherItemSaved.Notes.ClearString, cipherItem.Notes.ClearString);
        Assert.AreEqual(cipherItemSaved.Notes.CipherString, cipherItem.Notes.CipherString);

        Assert.AreEqual(cipherItemSaved.Favorite, cipherItem.Favorite);
        Assert.AreEqual(cipherItemSaved.Edit, cipherItem.Edit);
        Assert.AreEqual(cipherItemSaved.Reprompt, cipherItem.Reprompt);
        Assert.AreEqual(cipherItemSaved.FolderId, cipherItem.FolderId);
        Assert.AreEqual(cipherItemSaved.ViewPassword, cipherItem.ViewPassword);

        Assert.AreEqual(cipherItemSaved.SecureNote.Type, cipherItem.SecureNote.Type);

        await VaultWebClient.DeleteCipherItemAsync(cipherItemSaved.Id.Value);
    }

    [TestMethod]
    public async Task _0303_TestUserCreateCardItemAsync()
    {
        // TODO: Test for item existence before creating to not leave dangling tests

        var cipherItem = new CipherItemModel(UserCryptoService)
        {
            Name = new EncryptedString(UserCryptoService)
            {
                ClearString = "TestCreateUserCardItem"
            },
            Favorite = true,
            Edit = true,
            Reprompt = CipherRepromptType.Password,
            FolderId = Guid.Parse(TestUserFolderId),
            Notes = new EncryptedString(UserCryptoService)
            {
                ClearString = "TestCreateUserNotes"
            },
            ViewPassword = true,
        };

        var cardItem = cipherItem.CreateCard();
        cardItem.Brand = new EncryptedString(UserCryptoService)
        {
            ClearString = "TestCreateUserCardItemBrand"
        };
        cardItem.ExpMonth = new EncryptedString(UserCryptoService)
        {
            ClearString = "TestCreateUserCardItemExpMonth"
        };
        cardItem.Number = new EncryptedString(UserCryptoService)
        {
            ClearString = "TestCreateUserCardItemNumber"
        };
        cardItem.CardholderName = new EncryptedString(UserCryptoService)
        {
            ClearString = "TestCreateUserCardItemCardholderName"
        };
        cardItem.Code = new EncryptedString(UserCryptoService)
        {
            ClearString = "TestCreateUserCardItemCode"
        };
        cardItem.ExpYear = new EncryptedString(UserCryptoService)
        {
            ClearString = "TestCreateUserCardItemExpYear"
        };

        var cipherItemSaved = await VaultWebClient.CreateCipherItemAsync(cipherItem);
        Assert.IsNotNull(cipherItemSaved?.Id);
        Assert.AreEqual(cipherItemSaved.Name.ClearString, cipherItem.Name.ClearString);
        Assert.AreEqual(cipherItemSaved.Name.CipherString, cipherItem.Name.CipherString);

        Assert.AreEqual(cipherItemSaved.Notes.ClearString, cipherItem.Notes.ClearString);
        Assert.AreEqual(cipherItemSaved.Notes.CipherString, cipherItem.Notes.CipherString);

        Assert.AreEqual(cipherItemSaved.Favorite, cipherItem.Favorite);
        Assert.AreEqual(cipherItemSaved.Edit, cipherItem.Edit);
        Assert.AreEqual(cipherItemSaved.Reprompt, cipherItem.Reprompt);
        Assert.AreEqual(cipherItemSaved.FolderId, cipherItem.FolderId);
        Assert.AreEqual(cipherItemSaved.ViewPassword, cipherItem.ViewPassword);

        Assert.AreEqual(cipherItemSaved.Card.Brand.ClearString         , cipherItem.Card.Brand.ClearString         );
        Assert.AreEqual(cipherItemSaved.Card.ExpMonth.ClearString      , cipherItem.Card.ExpMonth.ClearString      );
        Assert.AreEqual(cipherItemSaved.Card.Number.ClearString        , cipherItem.Card.Number.ClearString        );
        Assert.AreEqual(cipherItemSaved.Card.CardholderName.ClearString, cipherItem.Card.CardholderName.ClearString);
        Assert.AreEqual(cipherItemSaved.Card.Code.ClearString          , cipherItem.Card.Code.ClearString          );
        Assert.AreEqual(cipherItemSaved.Card.ExpYear.ClearString       , cipherItem.Card.ExpYear.ClearString       );

        Assert.AreEqual(cipherItemSaved.Card.Brand.CipherString         , cipherItem.Card.Brand.CipherString         );
        Assert.AreEqual(cipherItemSaved.Card.ExpMonth.CipherString      , cipherItem.Card.ExpMonth.CipherString      );
        Assert.AreEqual(cipherItemSaved.Card.Number.CipherString        , cipherItem.Card.Number.CipherString        );
        Assert.AreEqual(cipherItemSaved.Card.CardholderName.CipherString, cipherItem.Card.CardholderName.CipherString);
        Assert.AreEqual(cipherItemSaved.Card.Code.CipherString          , cipherItem.Card.Code.CipherString          );
        Assert.AreEqual(cipherItemSaved.Card.ExpYear.CipherString       , cipherItem.Card.ExpYear.CipherString       );

        await VaultWebClient.DeleteCipherItemAsync(cipherItemSaved.Id.Value);
    }

    [TestMethod]
    public async Task _0304_TestUserCreateIdentityItemAsync()
    {
        // TODO: Test for item existence before creating to not leave dangling tests

        var cipherItem = new CipherItemModel(UserCryptoService)
        {
            Name = new EncryptedString(UserCryptoService)
            {
                ClearString = "TestCreateUserLoginItem"
            },
            Favorite = true,
            Edit = true,
            Reprompt = CipherRepromptType.Password,
            FolderId = Guid.Parse(TestUserFolderId),
            Notes = new EncryptedString(UserCryptoService)
            {
                ClearString = "TestCreateUserNotes"
            },
            ViewPassword = true,
        };

        var identityItem = cipherItem.CreateIdentity();
        identityItem.Address1  = new EncryptedString(UserCryptoService)
        {
            ClearString = "TestCreateUserIdentityItemAddress1"
        };
        identityItem.Address2  = new EncryptedString(UserCryptoService)
        {
            ClearString = "TestCreateUserIdentityItemAddress2"
        };
        identityItem.Address3  = new EncryptedString(UserCryptoService)
        {
            ClearString = "TestCreateUserIdentityItemAddress3"
        };
        identityItem.City  = new EncryptedString(UserCryptoService)
        {
            ClearString = "TestCreateUserIdentityItemCity"
        };
        identityItem.Company  = new EncryptedString(UserCryptoService)
        {
            ClearString = "TestCreateUserIdentityItemCompany"
        };
        identityItem.Country  = new EncryptedString(UserCryptoService)
        {
            ClearString = "TestCreateUserIdentityItemCountry"
        };
        identityItem.Email  = new EncryptedString(UserCryptoService)
        {
            ClearString = "TestCreateUserIdentityItemEmail"
        };
        identityItem.FirstName  = new EncryptedString(UserCryptoService)
        {
            ClearString = "TestCreateUserIdentityItemFirstName"
        };
        identityItem.LastName  = new EncryptedString(UserCryptoService)
        {
            ClearString = "TestCreateUserIdentityItemLastName"
        };
        identityItem.LicenseNumber  = new EncryptedString(UserCryptoService)
        {
            ClearString = "TestCreateUserIdentityItemLicenseNumber"
        };
        identityItem.MiddleName  = new EncryptedString(UserCryptoService)
        {
            ClearString = "TestCreateUserIdentityItemMiddleName"
        };
        identityItem.PassportNumber  = new EncryptedString(UserCryptoService)
        {
            ClearString = "TestCreateUserIdentityItemPassportNumber"
        };
        identityItem.Phone  = new EncryptedString(UserCryptoService)
        {
            ClearString = "TestCreateUserIdentityItemPhone"
        };
        identityItem.PostalCode  = new EncryptedString(UserCryptoService)
        {
            ClearString = "TestCreateUserIdentityItemPostalCode"
        };
        identityItem.SecuritySocialNumber  = new EncryptedString(UserCryptoService)
        {
            ClearString = "TestCreateUserIdentityItemSSN"
        };
        identityItem.State  = new EncryptedString(UserCryptoService)
        {
            ClearString = "TestCreateUserIdentityItemState"
        };
        identityItem.Title  = new EncryptedString(UserCryptoService)
        {
            ClearString = "TestCreateUserIdentityItemTitle"
        };
        identityItem.Username = new EncryptedString(UserCryptoService)
        {
            ClearString = "TestCreateUserIdentityItemUsername"
        };

        var cipherItemSaved = await VaultWebClient.CreateCipherItemAsync(cipherItem);
        Assert.IsNotNull(cipherItemSaved?.Id);
        Assert.AreEqual(cipherItemSaved.Name.ClearString, cipherItem.Name.ClearString);
        Assert.AreEqual(cipherItemSaved.Name.CipherString, cipherItem.Name.CipherString);

        Assert.AreEqual(cipherItemSaved.Notes.ClearString, cipherItem.Notes.ClearString);
        Assert.AreEqual(cipherItemSaved.Notes.CipherString, cipherItem.Notes.CipherString);

        Assert.AreEqual(cipherItemSaved.Favorite, cipherItem.Favorite);
        Assert.AreEqual(cipherItemSaved.Edit, cipherItem.Edit);
        Assert.AreEqual(cipherItemSaved.Reprompt, cipherItem.Reprompt);
        Assert.AreEqual(cipherItemSaved.FolderId, cipherItem.FolderId);
        Assert.AreEqual(cipherItemSaved.ViewPassword, cipherItem.ViewPassword);

        Assert.AreEqual(cipherItemSaved.Identity.Address1.ClearString      , cipherItem.Identity.Address1.ClearString      );
        Assert.AreEqual(cipherItemSaved.Identity.Address2.ClearString      , cipherItem.Identity.Address2.ClearString      );
        Assert.AreEqual(cipherItemSaved.Identity.Address3.ClearString      , cipherItem.Identity.Address3.ClearString      );
        Assert.AreEqual(cipherItemSaved.Identity.City.ClearString          , cipherItem.Identity.City.ClearString          );
        Assert.AreEqual(cipherItemSaved.Identity.Company.ClearString       , cipherItem.Identity.Company.ClearString       );
        Assert.AreEqual(cipherItemSaved.Identity.Country.ClearString       , cipherItem.Identity.Country.ClearString       );
        Assert.AreEqual(cipherItemSaved.Identity.Email.ClearString         , cipherItem.Identity.Email.ClearString         );
        Assert.AreEqual(cipherItemSaved.Identity.FirstName.ClearString     , cipherItem.Identity.FirstName.ClearString     );
        Assert.AreEqual(cipherItemSaved.Identity.LastName.ClearString      , cipherItem.Identity.LastName.ClearString      );
        Assert.AreEqual(cipherItemSaved.Identity.LicenseNumber.ClearString , cipherItem.Identity.LicenseNumber.ClearString );
        Assert.AreEqual(cipherItemSaved.Identity.MiddleName.ClearString    , cipherItem.Identity.MiddleName.ClearString    );
        Assert.AreEqual(cipherItemSaved.Identity.PassportNumber.ClearString, cipherItem.Identity.PassportNumber.ClearString);
        Assert.AreEqual(cipherItemSaved.Identity.Phone.ClearString         , cipherItem.Identity.Phone.ClearString         );
        Assert.AreEqual(cipherItemSaved.Identity.PostalCode.ClearString    , cipherItem.Identity.PostalCode.ClearString    );
        Assert.AreEqual(cipherItemSaved.Identity.SecuritySocialNumber.ClearString           , cipherItem.Identity.SecuritySocialNumber.ClearString           );
        Assert.AreEqual(cipherItemSaved.Identity.State.ClearString         , cipherItem.Identity.State.ClearString         );
        Assert.AreEqual(cipherItemSaved.Identity.Title.ClearString         , cipherItem.Identity.Title.ClearString         );
        Assert.AreEqual(cipherItemSaved.Identity.Username.ClearString      , cipherItem.Identity.Username.ClearString      );

        Assert.AreEqual(cipherItemSaved.Identity.Address1.CipherString      , cipherItem.Identity.Address1.CipherString      );
        Assert.AreEqual(cipherItemSaved.Identity.Address2.CipherString      , cipherItem.Identity.Address2.CipherString      );
        Assert.AreEqual(cipherItemSaved.Identity.Address3.CipherString      , cipherItem.Identity.Address3.CipherString      );
        Assert.AreEqual(cipherItemSaved.Identity.City.CipherString          , cipherItem.Identity.City.CipherString          );
        Assert.AreEqual(cipherItemSaved.Identity.Company.CipherString       , cipherItem.Identity.Company.CipherString       );
        Assert.AreEqual(cipherItemSaved.Identity.Country.CipherString       , cipherItem.Identity.Country.CipherString       );
        Assert.AreEqual(cipherItemSaved.Identity.Email.CipherString         , cipherItem.Identity.Email.CipherString         );
        Assert.AreEqual(cipherItemSaved.Identity.FirstName.CipherString     , cipherItem.Identity.FirstName.CipherString     );
        Assert.AreEqual(cipherItemSaved.Identity.LastName.CipherString      , cipherItem.Identity.LastName.CipherString      );
        Assert.AreEqual(cipherItemSaved.Identity.LicenseNumber.CipherString , cipherItem.Identity.LicenseNumber.CipherString );
        Assert.AreEqual(cipherItemSaved.Identity.MiddleName.CipherString    , cipherItem.Identity.MiddleName.CipherString    );
        Assert.AreEqual(cipherItemSaved.Identity.PassportNumber.CipherString, cipherItem.Identity.PassportNumber.CipherString);
        Assert.AreEqual(cipherItemSaved.Identity.Phone.CipherString         , cipherItem.Identity.Phone.CipherString         );
        Assert.AreEqual(cipherItemSaved.Identity.PostalCode.CipherString    , cipherItem.Identity.PostalCode.CipherString    );
        Assert.AreEqual(cipherItemSaved.Identity.SecuritySocialNumber.CipherString           , cipherItem.Identity.SecuritySocialNumber.CipherString           );
        Assert.AreEqual(cipherItemSaved.Identity.State.CipherString         , cipherItem.Identity.State.CipherString         );
        Assert.AreEqual(cipherItemSaved.Identity.Title.CipherString         , cipherItem.Identity.Title.CipherString         );
        Assert.AreEqual(cipherItemSaved.Identity.Username.CipherString      , cipherItem.Identity.Username.CipherString      );

        await VaultWebClient.DeleteCipherItemAsync(cipherItemSaved.Id.Value);
    }

    [TestMethod]
    public async Task _0305_TestUserCreateSSHKeyItemAsync()
    {
        // TODO: Test for item existence before creating to not leave dangling tests

        var cipherItem = new CipherItemModel(UserCryptoService)
        {
            Name = new EncryptedString(UserCryptoService)
            {
                ClearString = "TestCreateUserRSAKeyItem"
            },
            Favorite = true,
            Edit = true,
            Reprompt = CipherRepromptType.Password,
            FolderId = Guid.Parse(TestUserFolderId),
            Notes = new EncryptedString(UserCryptoService)
            {
                ClearString = "TestCreateUserNotes"
            },
            ViewPassword = true,
        };

        var rsaKey = BitWardenHelper.GenerateRsaKey();

        var sshKeyItem = cipherItem.CreateSshKey();
        sshKeyItem.PublicKey = new EncryptedString(UserCryptoService)
        {
            ClearString = rsaKey.PublicKey,
        };
        sshKeyItem.PrivateKey = new EncryptedString(UserCryptoService)
        {
            ClearString = rsaKey.PrivateKey,
        };
        sshKeyItem.KeyFingerprint = new EncryptedString(UserCryptoService)
        {
            ClearString = rsaKey.Fingerprint,
        };

        var cipherItemSaved = await VaultWebClient.CreateCipherItemAsync(cipherItem);
        Assert.IsNotNull(cipherItemSaved?.Id);
        Assert.AreEqual(cipherItemSaved.Name.ClearString, cipherItem.Name.ClearString);
        Assert.AreEqual(cipherItemSaved.Name.CipherString, cipherItem.Name.CipherString);

        Assert.AreEqual(cipherItemSaved.Notes.ClearString, cipherItem.Notes.ClearString);
        Assert.AreEqual(cipherItemSaved.Notes.CipherString, cipherItem.Notes.CipherString);

        Assert.AreEqual(cipherItemSaved.SshKey.PublicKey.ClearString, cipherItem.SshKey.PublicKey.ClearString);
        Assert.AreEqual(cipherItemSaved.SshKey.PublicKey.CipherString, cipherItem.SshKey.PublicKey.CipherString);

        Assert.AreEqual(cipherItemSaved.SshKey.PrivateKey.ClearString, cipherItem.SshKey.PrivateKey.ClearString);
        Assert.AreEqual(cipherItemSaved.SshKey.PrivateKey.CipherString, cipherItem.SshKey.PrivateKey.CipherString);

        Assert.AreEqual(cipherItemSaved.SshKey.KeyFingerprint.ClearString, cipherItem.SshKey.KeyFingerprint.ClearString);
        Assert.AreEqual(cipherItemSaved.SshKey.KeyFingerprint.CipherString, cipherItem.SshKey.KeyFingerprint.CipherString);

        await VaultWebClient.DeleteCipherItemAsync(cipherItemSaved.Id.Value);
    }

    [TestMethod]
    public async Task _0306_TestUserCreateFolderItemAsync()
    {
        // TODO: Test for item existence before creating to not leave dangling tests

        var cryptString = new EncryptedString(UserCryptoService);
        cryptString.ClearString = "TestDirectory";

        var folder = await VaultWebClient.CreateFolderAsync(cryptString.CipherString);
        Assert.IsNotNull(folder?.Id);
        Assert.AreEqual(folder.Name.CipherString, cryptString.CipherString);
        Assert.AreEqual(folder.Name.ClearString, cryptString.ClearString);
        var folderId = folder.Id.Value;

        cryptString.ClearString = "Renamed TestDirectory";
        folder = await VaultWebClient.UpdateFolderAsync(folder.Id.Value, cryptString.CipherString);
        Assert.IsNotNull(folder?.Id);
        Assert.AreEqual(folder?.Id, folderId);
        Assert.AreEqual(folder.Name.CipherString, cryptString.CipherString);
        Assert.AreEqual(folder.Name.ClearString, cryptString.ClearString);

        var folders = await VaultWebClient.GetFoldersAsync();
        var foundFolder = folders.Find(e => e.Id == folderId);
        Assert.IsNotNull(foundFolder);
        Assert.AreEqual(foundFolder.Name.ClearString, cryptString.ClearString);

        folder = await VaultWebClient.GetFolderAsync(folder.Id.Value);
        Assert.IsNotNull(folder?.Id);
        Assert.AreEqual(folder?.Id, foundFolder.Id);

        await VaultWebClient.DeleteFolderAsync(folder.Id.Value);

        try
        {
            folder = await VaultWebClient.GetFolderAsync(folder.Id.Value);
            Assert.IsNull("GetFolderAsync with an inexistant ID should throw!");
        }
        catch (Exception)
        {
        }
    }

    [TestMethod]
    public async Task _0307_TestUserTrashCipherItemAsync()
    {
        var cipherItem = new CipherItemModel(UserCryptoService)
        {
            Name = new EncryptedString(UserCryptoService)
            {
                ClearString = "TestTrashItem"
            },
        };

        var secureNoteItem = cipherItem.CreateSecureNote();
        secureNoteItem.Type = SecureNoteType.Generic;

        var cipherItemSaved = await VaultWebClient.CreateCipherItemAsync(cipherItem);
        Assert.IsNotNull(cipherItemSaved?.Id);
        Assert.AreEqual(cipherItemSaved.Name.ClearString, cipherItem.Name.ClearString);
        Assert.AreEqual(cipherItemSaved.Name.CipherString, cipherItem.Name.CipherString);
        Assert.IsNull(cipherItemSaved.DeletedDate);

        await VaultWebClient.MoveToTrashCipherItemAsync(cipherItemSaved.Id.Value);
        cipherItemSaved = await VaultWebClient.GetCipherItemAsync(cipherItemSaved.Id.Value);
        Assert.IsNotNull(cipherItemSaved.DeletedDate);

        await VaultWebClient.RestoreCipherItemAsync(cipherItemSaved.Id.Value);
        cipherItemSaved = await VaultWebClient.GetCipherItemAsync(cipherItemSaved.Id.Value);
        Assert.IsNull(cipherItemSaved.DeletedDate);

        await VaultWebClient.DeleteCipherItemAsync(cipherItemSaved.Id.Value);
    }

    [TestMethod]
    public async Task _0401_TestOrganizationCreateLoginItemAsync()
    {
        // TODO: Test for item existence before creating to not leave dangling tests

        var organizationCryptoService = OrganizationCryptoFactoryService.GetOrganizationCryptoService(Guid.Parse(TestOrganizationId));

        var cipherItem = new CipherItemModel(organizationCryptoService)
        {
            Name = new EncryptedString(organizationCryptoService)
            {
                ClearString = "TestCreateOrganizationLoginItem"
            },
            Favorite = true,
            Edit = true,
            Reprompt = CipherRepromptType.Password,
            FolderId = Guid.Parse(TestUserFolderId),
            Notes = new EncryptedString(organizationCryptoService)
            {
                ClearString = "TestCreateOrganizationNotes"
            },
            ViewPassword = true,
            OrganizationId = Guid.Parse(TestOrganizationId),
            CollectionsIds = [Guid.Parse(TestOrganizationCollectionId)],
        };

        var loginItem = cipherItem.CreateLogin();
        loginItem.Username = new EncryptedString(organizationCryptoService)
        {
            ClearString = "TestOrganizationUsername",
        };
        loginItem.Password = new EncryptedString(organizationCryptoService)
        {
            ClearString = "TestOrganizationPassword",
        };

        var cipherItemSaved = await VaultWebClient.CreateCipherItemAsync(cipherItem);

        Assert.IsNotNull(cipherItemSaved?.Id);
        Assert.AreEqual(cipherItemSaved.Name.ClearString, cipherItem.Name.ClearString);
        Assert.AreEqual(cipherItemSaved.Name.CipherString, cipherItem.Name.CipherString);
        Assert.AreEqual(cipherItemSaved.OrganizationId, cipherItem.OrganizationId);

        Assert.AreEqual(cipherItemSaved.Login.Username.ClearString, cipherItem.Login.Username.ClearString);
        Assert.AreEqual(cipherItemSaved.Login.Username.CipherString, cipherItem.Login.Username.CipherString);

        Assert.AreEqual(cipherItemSaved.Login.Password.ClearString, cipherItem.Login.Password.ClearString);
        Assert.AreEqual(cipherItemSaved.Login.Password.CipherString, cipherItem.Login.Password.CipherString);

        // The collection ids are not returned on creation result,
        // but if someday they do, its handled.
        if (cipherItemSaved.CollectionsIds == null)
        {
            VaultService.ReloadBitWardenDatabase(await VaultWebClient.GetDatabaseAsync());
            cipherItemSaved = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == cipherItemSaved.Id.Value);
        }

        Assert.AreEqual(cipherItemSaved.CollectionsIds.Count, cipherItem.CollectionsIds.Count);
        Assert.AreEqual(cipherItemSaved.CollectionsIds[0], cipherItem.CollectionsIds[0]);

        await VaultWebClient.DeleteCipherItemAsync(cipherItemSaved.Id.Value);
    }

    [TestMethod]
    public async Task _0402_TestUserCreateFolderItemAsync()
    {
        // TODO: Test for item existence before creating to not leave dangling tests 

        var cryptString = new EncryptedString(UserCryptoService);
        cryptString.ClearString = "TestDirectory";

        var folder = await VaultWebClient.CreateFolderAsync(cryptString.CipherString);
        Assert.IsNotNull(folder?.Id);
        Assert.AreEqual(folder.Name.CipherString, cryptString.CipherString);
        Assert.AreEqual(folder.Name.ClearString, cryptString.ClearString);
        var folderId = folder.Id.Value;

        cryptString.ClearString = "Renamed TestDirectory";
        folder = await VaultWebClient.UpdateFolderAsync(folder.Id.Value, cryptString.CipherString);
        Assert.IsNotNull(folder?.Id);
        Assert.AreEqual(folder?.Id, folderId);
        Assert.AreEqual(folder.Name.CipherString, cryptString.CipherString);
        Assert.AreEqual(folder.Name.ClearString, cryptString.ClearString);

        var folders = await VaultWebClient.GetFoldersAsync();
        var foundFolder = folders.Find(e => e.Id == folderId);
        Assert.IsNotNull(foundFolder);
        Assert.AreEqual(foundFolder.Name.ClearString, cryptString.ClearString);

        folder = await VaultWebClient.GetFolderAsync(folder.Id.Value);
        Assert.IsNotNull(folder?.Id);
        Assert.AreEqual(folder?.Id, foundFolder.Id);

        await VaultWebClient.DeleteFolderAsync(folder.Id.Value);

        try
        {
            folder = await VaultWebClient.GetFolderAsync(folder.Id.Value);
            Assert.IsNull("GetFolderAsync with an inexistant ID should throw!");
        }
        catch (Exception)
        {
        }
    }

    [TestMethod]
    public async Task _0403_TestOrganizationCreateCollectionItemAsync()
    {
        // TODO: Test for item existence before creating to not leave dangling tests

        var organizationCryptoService = OrganizationCryptoFactoryService.GetOrganizationCryptoService(Guid.Parse(TestOrganizationId));

        var cryptString = new EncryptedString(organizationCryptoService);
        cryptString.ClearString = "TestCollection";

        var userProfile = VaultService.GetBitWardenDatabase().Profile;

        var collection = await VaultWebClient.CreateCollectionAsync(Guid.Parse(TestOrganizationId), cryptString.CipherString, [new UserCollectionPermissionsModel
        {
            Id = userProfile.Organizations.First(e => e.Id == Guid.Parse(TestOrganizationId)).OrganizationUserId,
            HidePasswords = false,
            Manage = true,
            ReadOnly = false,
        }], null);
        Assert.IsNotNull(collection);
        Assert.AreEqual(collection.OrganizationId.Value, Guid.Parse(TestOrganizationId));

        await VaultWebClient.DeleteCollectionAsync(Guid.Parse(TestOrganizationId), collection.Id.Value);
    }

    [TestMethod]
    public async Task _0404_TestOrganizationUpdateItemCollectionsAsync()
    {
        // TODO: Test for item existence before creating to not leave dangling tests

        var organizationCryptoService = OrganizationCryptoFactoryService.GetOrganizationCryptoService(Guid.Parse(TestOrganizationId));

        var cipherItem = new CipherItemModel(organizationCryptoService)
        {
            Name = new EncryptedString(organizationCryptoService)
            {
                ClearString = "TestMoveItemCollection"
            },
            Favorite = true,
            Edit = true,
            Reprompt = CipherRepromptType.Password,
            ViewPassword = true,
            OrganizationId = Guid.Parse(TestOrganizationId),
            CollectionsIds = [Guid.Parse(TestOrganizationCollectionId)],
        };

        var loginItem = cipherItem.CreateLogin();
        loginItem.Username = new EncryptedString(organizationCryptoService)
        {
            ClearString = "TestOrganizationUsername",
        };
        loginItem.Password = new EncryptedString(organizationCryptoService)
        {
            ClearString = "TestOrganizationPassword",
        };

        var cipherItemSaved = await VaultWebClient.CreateCipherItemAsync(cipherItem);

        Assert.IsNotNull(cipherItemSaved?.Id);

        // The collection ids are not returned on creation result,
        // but if someday they do, its handled.
        if (cipherItemSaved.CollectionsIds == null)
        {
            VaultService.ReloadBitWardenDatabase(await VaultWebClient.GetDatabaseAsync());
            cipherItemSaved = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == cipherItemSaved.Id.Value);
        }

        Assert.AreEqual(cipherItemSaved.CollectionsIds.Count, cipherItem.CollectionsIds.Count);
        Assert.AreEqual(cipherItemSaved.CollectionsIds[0], cipherItem.CollectionsIds[0]);

        var cryptString = new EncryptedString(organizationCryptoService);
        cryptString.ClearString = "TestCollection";

        var userProfile = VaultService.GetBitWardenDatabase().Profile;

        // Create collection here, Bitwarden free account is limited to 2 collections
        var collection = await VaultWebClient.CreateCollectionAsync(Guid.Parse(TestOrganizationId), cryptString.CipherString, [new UserCollectionPermissionsModel
        {
            Id = userProfile.Organizations.First(e => e.Id == Guid.Parse(TestOrganizationId)).OrganizationUserId,
            HidePasswords = false,
            Manage = true,
            ReadOnly = false,
        }], null);
        Assert.IsNotNull(collection);
        Assert.AreEqual(collection.OrganizationId.Value, Guid.Parse(TestOrganizationId));

        var movedItem = await VaultWebClient.UpdateCipherItemCollectionsAsync(cipherItemSaved.Id.Value, [collection.Id.Value]);

        Assert.AreEqual(movedItem.CollectionsIds[0], collection.Id.Value);

        await VaultWebClient.DeleteCipherItemAsync(cipherItemSaved.Id.Value);
        await VaultWebClient.DeleteCollectionAsync(Guid.Parse(TestOrganizationId), collection.Id.Value);
    }

    private static readonly TimeSpan NotificationTimeout = TimeSpan.FromMinutes(1);

    [TestMethod]
    public async Task _0901_TestNotificationAsync()
    {
        var notificationClient = DatabaseSessionScope.ServiceProvider.GetRequiredService<INotificationClientService>();
        var cts = new CancellationTokenSource();

        var encryptedString = new EncryptedString(UserCryptoService);

        var notificationMessage = default(PushNotificationBaseModel);
        var conditionVar = new object();

        // Need to wait, BitWarden's notification Hub is SLOW, so it can grab our previous tests notifications.
        await Task.Delay(20000);

        await notificationClient.StartAsync(cts.Token);
        notificationClient.OnPushNotificationAsyncReceived += message =>
        {
            lock (conditionVar)
            {
                notificationMessage = message;
                Monitor.Pulse(conditionVar);
                return Task.CompletedTask;
            }
        };

        var cipherItem = new CipherItemModel(UserCryptoService)
        {
            Name = new EncryptedString(UserCryptoService)
            {
                ClearString = "TestNotificationCipherItem"
            }
        };
        var cipherLoginItem = cipherItem.CreateLogin();

        lock (conditionVar)
        {
            cipherItem = VaultWebClient.CreateCipherItemAsync(cipherItem).GetAwaiter().GetResult();
            Assert.IsTrue(Monitor.Wait(conditionVar, NotificationTimeout), "No push notification received.");
        }
        Assert.AreEqual(PushNotificationType.SyncCipherCreate, notificationMessage.Type);
        Assert.AreEqual(typeof(SyncCipherPushNotificationModel), notificationMessage.Payload.GetType());
        Assert.AreEqual(cipherItem.Id, (notificationMessage.Payload as SyncCipherPushNotificationModel).Id);

        lock (conditionVar)
        {
            cipherItem.Login.Username = new EncryptedString(UserCryptoService)
            {
                ClearString = "UserNameUpdate"
            };
            cipherItem = VaultWebClient.UpdateCipherItemAsync(cipherItem.Id.Value, cipherItem).GetAwaiter().GetResult();
            Assert.IsTrue(Monitor.Wait(conditionVar, NotificationTimeout), "No push notification received.");
        }
        Assert.AreEqual(PushNotificationType.SyncCipherUpdate, notificationMessage.Type);
        Assert.AreEqual(typeof(SyncCipherPushNotificationModel), notificationMessage.Payload.GetType());
        Assert.AreEqual(cipherItem.Id, (notificationMessage.Payload as SyncCipherPushNotificationModel).Id);

        lock (conditionVar)
        {
            VaultWebClient.DeleteCipherItemAsync(cipherItem.Id.Value).GetAwaiter().GetResult();
            Assert.IsTrue(Monitor.Wait(conditionVar, NotificationTimeout), "No push notification received.");
        }
        Assert.AreEqual(PushNotificationType.SyncLoginDelete, notificationMessage.Type);
        Assert.AreEqual(typeof(SyncCipherPushNotificationModel), notificationMessage.Payload.GetType());
        Assert.AreEqual(cipherItem.Id, (notificationMessage.Payload as SyncCipherPushNotificationModel).Id);

        var folder = default(FolderItemModel);
        lock (conditionVar)
        {
            encryptedString.ClearString = "TestNotificationFolder";
            folder = VaultWebClient.CreateFolderAsync(encryptedString.CipherString).GetAwaiter().GetResult();
            Assert.IsTrue(Monitor.Wait(conditionVar, NotificationTimeout), "No push notification received.");
        }
        Assert.AreEqual(PushNotificationType.SyncFolderCreate, notificationMessage.Type);
        Assert.AreEqual(typeof(SyncFolderPushNotificationModel), notificationMessage.Payload.GetType());
        Assert.AreEqual(folder.Id, (notificationMessage.Payload as SyncFolderPushNotificationModel).Id);

        lock (conditionVar)
        {
            encryptedString.ClearString = "TestNotificationFolderNewName";
            folder = VaultWebClient.UpdateFolderAsync(folder.Id.Value, encryptedString.CipherString).GetAwaiter().GetResult();
            Assert.IsTrue(Monitor.Wait(conditionVar, NotificationTimeout), "No push notification received.");
        }
        Assert.AreEqual(PushNotificationType.SyncFolderUpdate, notificationMessage.Type);
        Assert.AreEqual(typeof(SyncFolderPushNotificationModel), notificationMessage.Payload.GetType());
        Assert.AreEqual(folder.Id, (notificationMessage.Payload as SyncFolderPushNotificationModel).Id);

        lock (conditionVar)
        {
            VaultWebClient.DeleteFolderAsync(folder.Id.Value).GetAwaiter().GetResult();
            Assert.IsTrue(Monitor.Wait(conditionVar, NotificationTimeout), "No push notification received.");
        }
        Assert.AreEqual(PushNotificationType.SyncFolderDelete, notificationMessage.Type);
        Assert.AreEqual(typeof(SyncFolderPushNotificationModel), notificationMessage.Payload.GetType());
        Assert.AreEqual(folder.Id, (notificationMessage.Payload as SyncFolderPushNotificationModel).Id);

        await notificationClient.StopAsync();
    }
}

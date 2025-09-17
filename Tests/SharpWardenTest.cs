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
    const string TestOrganizationCollectionId = "16a3dadf-cfa3-4bab-a097-b30201260764";
    const string TestOrganizationCollection2Id = "9be0a073-adf9-4e84-82fe-b35b00acdc2a";
    const string TestOrganizationItemId = "2c94bf3f-fcfe-48eb-b19c-b307006b69be";

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
        DatabaseSessionScope = SharpWarden.BitWardenHelper.CreateSessionScope(IWebClientService.BitWardenEUHostUrl, INotificationClientService.BitWardenEUHostUrl);
        VaultWebClient = DatabaseSessionScope.ServiceProvider.GetRequiredService<IWebClientService>();
        VaultService = DatabaseSessionScope.ServiceProvider.GetRequiredService<IVaultService>();
        UserCryptoService = DatabaseSessionScope.ServiceProvider.GetRequiredService<IUserCryptoService>();
        OrganizationCryptoFactoryService = DatabaseSessionScope.ServiceProvider.GetRequiredService<IOrganizationCryptoFactoryService>();
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
    public async Task _0103_TestUserCipherItemAsync()
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
    public async Task _0104_TestUserFolderItemAsync()
    {
        var folder = VaultService.GetBitWardenDatabase().Folders.Find(e => e.Id == Guid.Parse(TestUserFolderId));

        Assert.IsNotNull(folder);
        Assert.AreEqual(folder.Id.ToString(), TestUserFolderId);
        Assert.AreEqual(folder.Name.ClearString, "SharpWardenTestFolder");
    }

    [TestMethod]
    public async Task _0105_TestUserItemInFolderAsync()
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
    public async Task _0106_TestUserItemLoginAsync()
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
    public async Task _0107_TestUserItemCardAsync()
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
    public async Task _0108_TestUserItemIdentityAsync()
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
        Assert.AreEqual(item.Identity.SecuritySocialNumber.ClearString, "IdentitySocialSecurityNumber");
        Assert.AreEqual(item.Identity.PassportNumber.ClearString, "IdentityPassportNumber");
        Assert.AreEqual(item.Identity.LicenseNumber.ClearString, "IdentityLicenseNumber");
        Assert.AreEqual(item.Identity.Email.ClearString, "IdentityMail");
        Assert.AreEqual(item.Identity.Phone.ClearString, "IdentityPhone");
    }

    [TestMethod]
    public async Task _0109_TestUserItemSecureNoteAsync()
    {
        var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(TestUserItemSecureNoteId));

        Assert.IsNotNull(item);
        Assert.AreEqual(item.Id.ToString(), TestUserItemSecureNoteId);
        Assert.AreEqual(item.ItemType, CipherItemType.SecureNote);
        Assert.AreEqual(item.Notes.ClearString, "SecureNoteNotes");

        Assert.AreEqual(item.SecureNote.Type, SecureNoteType.Generic);
    }

    [TestMethod]
    public async Task _0110_TestUserItemSSHKeyAsync()
    {
        var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(TestUserItemSSHKeyId));

        Assert.IsNotNull(item);
        Assert.AreEqual(item.Id.ToString(), TestUserItemSSHKeyId);
        Assert.AreEqual(item.ItemType, CipherItemType.SshKey);
        Assert.AreEqual(item.Notes.ClearString, "SSHKeyNotes");

        Assert.AreEqual(item.SshKey.KeyFingerprint.ClearString, "SHA256:dbBC+ec5IlSfcufRZnalIk2q5ISTup3nff6mdjAccVI");
        Assert.AreEqual(item.SshKey.PublicKey.ClearString, "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIKoMZebfc4izVVC0pjt9KNAtpVQ15X1MgpdzjloRAP3N");
        Assert.AreEqual(item.SshKey.PrivateKey.ClearString, "-----BEGIN OPENSSH PRIVATE KEY-----\nb3BlbnNzaC1rZXktdjEAAAAABG5vbmUAAAAEbm9uZQAAAAAAAAABAAAAMwAAAAtzc2gtZW\nQyNTUxOQAAACCqDGXm33OIs1VQtKY7fSjQLaVUNeV9TIKXc45aEQD9zQAAAIhBcXK7QXFy\nuwAAAAtzc2gtZWQyNTUxOQAAACCqDGXm33OIs1VQtKY7fSjQLaVUNeV9TIKXc45aEQD9zQ\nAAAEAZBznOkm7Q61yxXzyWOmVw+OPn7eKOBpcf4cELcPnPnKoMZebfc4izVVC0pjt9KNAt\npVQ15X1MgpdzjloRAP3NAAAAAAECAwQF\n-----END OPENSSH PRIVATE KEY-----\n");
    }

    [TestMethod]
    public async Task _0111_TestUserItemCustomFieldsAsync()
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
    public async Task _0201_TestOrganizationCipherItemAsync()
    {
        var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(TestOrganizationItemId) && e.OrganizationId == Guid.Parse(TestOrganizationId));

        Assert.IsNotNull(item);
        Assert.AreEqual(item.Id.ToString(), TestOrganizationItemId);
        Assert.AreEqual(item.Name.ClearString, "SharpWardenOrganizationEntry");
        Assert.AreEqual(item.Login.Username.ClearString, "OrganizationUsername");
        Assert.AreEqual(item.Login.Password.ClearString, "OrganizationPassword");
        Assert.AreEqual(item.Login.Totp.ClearString, "OrganizationAuthenticatorKey");
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

        var movedItem = await VaultWebClient.UpdateCipherItemCollectionsAsync(cipherItemSaved.Id.Value, [Guid.Parse(TestOrganizationCollection2Id)]);

        Assert.AreEqual(movedItem.CollectionsIds[0], Guid.Parse(TestOrganizationCollection2Id));

        await VaultWebClient.DeleteCipherItemAsync(cipherItemSaved.Id.Value);
    }

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
            Monitor.Wait(conditionVar);
        }
        Assert.AreEqual(notificationMessage.Type, PushNotificationType.SyncCipherCreate);
        Assert.AreEqual(notificationMessage.Payload.GetType(), typeof(SyncCipherPushNotificationModel));
        Assert.AreEqual(cipherItem.Id, (notificationMessage.Payload as SyncCipherPushNotificationModel).Id);

        lock (conditionVar)
        {
            cipherItem.Login.Username = new EncryptedString(UserCryptoService)
            {
                ClearString = "UserNameUpdate"
            };
            cipherItem = VaultWebClient.UpdateCipherItemAsync(cipherItem.Id.Value, cipherItem).GetAwaiter().GetResult();
            Monitor.Wait(conditionVar);
        }
        Assert.AreEqual(notificationMessage.Type, PushNotificationType.SyncCipherUpdate);
        Assert.AreEqual(notificationMessage.Payload.GetType(), typeof(SyncCipherPushNotificationModel));
        Assert.AreEqual(cipherItem.Id, (notificationMessage.Payload as SyncCipherPushNotificationModel).Id);

        lock (conditionVar)
        {
            VaultWebClient.DeleteCipherItemAsync(cipherItem.Id.Value).GetAwaiter().GetResult();
            Monitor.Wait(conditionVar);
        }
        Assert.AreEqual(notificationMessage.Type, PushNotificationType.SyncLoginDelete);
        Assert.AreEqual(notificationMessage.Payload.GetType(), typeof(SyncCipherPushNotificationModel));
        Assert.AreEqual(cipherItem.Id, (notificationMessage.Payload as SyncCipherPushNotificationModel).Id);

        var folder = default(FolderItemModel);
        lock (conditionVar)
        {
            encryptedString.ClearString = "TestNotificationFolder";
            folder = VaultWebClient.CreateFolderAsync(encryptedString.CipherString).GetAwaiter().GetResult();
            Monitor.Wait(conditionVar);
        }
        Assert.AreEqual(notificationMessage.Type, PushNotificationType.SyncFolderCreate);
        Assert.AreEqual(notificationMessage.Payload.GetType(), typeof(SyncFolderPushNotificationModel));
        Assert.AreEqual(folder.Id, (notificationMessage.Payload as SyncFolderPushNotificationModel).Id);

        lock (conditionVar)
        {
            encryptedString.ClearString = "TestNotificationFolderNewName";
            folder = VaultWebClient.UpdateFolderAsync(folder.Id.Value, encryptedString.CipherString).GetAwaiter().GetResult();
            Monitor.Wait(conditionVar);
        }
        Assert.AreEqual(notificationMessage.Type, PushNotificationType.SyncFolderUpdate);
        Assert.AreEqual(notificationMessage.Payload.GetType(), typeof(SyncFolderPushNotificationModel));
        Assert.AreEqual(folder.Id, (notificationMessage.Payload as SyncFolderPushNotificationModel).Id);

        lock (conditionVar)
        {
            VaultWebClient.DeleteFolderAsync(folder.Id.Value).GetAwaiter().GetResult();
            Monitor.Wait(conditionVar);
        }
        Assert.AreEqual(notificationMessage.Type, PushNotificationType.SyncFolderDelete);
        Assert.AreEqual(notificationMessage.Payload.GetType(), typeof(SyncFolderPushNotificationModel));
        Assert.AreEqual(folder.Id, (notificationMessage.Payload as SyncFolderPushNotificationModel).Id);

        await notificationClient.StopAsync();
    }
}

using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
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
namespace SharpWardenExamples;

class Example
{
    static IServiceScope BitWardenDatabaseSessionScope;
    static IWebClientService VaultWebClient;
    static INotificationClientService NotificationClient;
    static IVaultService VaultService;
    static IUserCryptoService UserCryptoService;

    static void Main()
    {
        MainAsync().GetAwaiter().GetResult();
    }

    static void PrintCipherField(EncryptedString str, [CallerArgumentExpression("str")] string name = null)
    {
        name = name?.Split('.').Length > 0
            ? name.Split('.')[^1]
            : name;

        if (str?.CipherString != null)
        {
            if (str.HasSession())
            {
                Console.WriteLine($"  {name}: {str.ClearString}");
            }
            else
            { 
                Console.WriteLine($"  {name}: {str.CipherString}");
            }
        }
        else
        {
            Console.WriteLine($"  {name}: (null)");
        }
    }

    static void PrintField(string str, [CallerArgumentExpression("str")] string name = null)
    {
        name = name?.Split('.').Length > 0
            ? name.Split('.')[^1]
            : name;

        if (str != null)
            Console.WriteLine($"  {name}: {str}");
        else
            Console.WriteLine($"  {name}: (null)");
    }

    static void PrintLoginUri(UriModel uri)
    {
        Console.WriteLine("  Login Uri:");
        PrintCipherField(uri.Uri, "  Uri");
        PrintCipherField(uri.UriChecksum, "  UriChecksum");
        PrintField(uri.Match?.ToString() ?? string.Empty, "  Match");
    }

    static void PrintLoginField(LoginFieldModel model)
    {
        PrintCipherField(model.Username);
        PrintCipherField(model.Password);

        foreach (var uri in model.Uris)
            PrintLoginUri(uri);
    }

    static void PrintIdentityField(IdentityFieldModel model)
    {
        PrintCipherField(model.Address1);
        PrintCipherField(model.Address2);
        PrintCipherField(model.Address3);
        PrintCipherField(model.City);
        PrintCipherField(model.Company);
        PrintCipherField(model.Country);
        PrintCipherField(model.Email);
        PrintCipherField(model.FirstName);
        PrintCipherField(model.MiddleName);
        PrintCipherField(model.LastName);
        PrintCipherField(model.LicenseNumber);
        PrintCipherField(model.PassportNumber);
        PrintCipherField(model.Phone);
        PrintCipherField(model.PostalCode);
        PrintCipherField(model.SSN);
        PrintCipherField(model.State);
        PrintCipherField(model.Title);
        PrintCipherField(model.Username);
    }

    static void PrintSecureNoteField(SecureNoteFieldModel model)
    {
    }

    static void PrintCardField(CardFieldModel model)
    {
        PrintCipherField(model.Brand);
        PrintCipherField(model.CardholderName);
        PrintCipherField(model.Code);
        PrintCipherField(model.ExpMonth);
        PrintCipherField(model.ExpYear);
        PrintCipherField(model.Number);
    }

    static void PrintCustomField(CustomFieldModel field)
    {
        Console.WriteLine("  Custom field");
        PrintField(field.Type.ToString(), "  Type");
        PrintCipherField(field.Name, "  Name");
        PrintCipherField(field.Value, "  Value");
        PrintField(field.LinkedId == null ? null : field.LinkedId.ToString(), "  LinkedId");
    }

    static void PrintCustomFields(IEnumerable<CustomFieldModel> fields)
    {
        if (fields?.Any() != true)
            return;

        foreach (var field in fields)
        {
            PrintCustomField(field);
            Console.WriteLine();
        }
    }

    static void PrintAttachment(AttachmentModel attachement)
    {
        Console.WriteLine($"  Attachement id: {attachement.Id}");
        PrintCipherField(attachement.FileName, "  FileName");
        PrintField(Convert.ToHexString(attachement.Key.ClearBytes), "  Key");
        PrintField(attachement.Size.ToString(), "  Size");
        PrintField(attachement.Url, "  Url");
    }

    static void PrintAttachments(IEnumerable<AttachmentModel> attachments)
    {
        if (attachments?.Any() != true)
            return;

        foreach (var attachement in attachments)
        {
            PrintAttachment(attachement);
            Console.WriteLine();
        }
    }

    static void PrintCollection(CollectionItemModel collection)
    {
        Console.WriteLine($"  Collection id: {collection.Id}");
        PrintCipherField(collection.Name, "  Name");
        PrintField(collection.ExternalId?.ToString(), "  ExternalId");
        PrintField(collection.HidePasswords.ToString(), "  HidePasswords");
        PrintField(collection.Id?.ToString(), "  Id");
        PrintField(collection.Manage.ToString(), "  Manage");
        PrintField(collection.OrganizationId?.ToString(), "  OrganizationId");
        PrintField(collection.ReadOnly.ToString(), "  ReadOnly");
    }

    static void PrintCollectionsFromIds(IEnumerable<Guid> collectionsIds)
    {
        if (collectionsIds?.Any() != true)
            return;

        var collections = VaultService.GetBitWardenDatabase().Collections.Where(e => collectionsIds.Contains(e.Id.Value));
        
        foreach (var collection in collections)
        {
            PrintCollection(collection);
            Console.WriteLine();
        }
    }

    static void PrintCipherItem(CipherItemModel item)
    {
        var folder = VaultService.GetBitWardenDatabase().Folders.Find(e => e.Id == item.FolderId)?.Name.ClearString;
        if (folder != null)
            folder = "/" + folder;
        
        Console.WriteLine("================================");
        Console.WriteLine($"Item: {folder}/{item.Id}");

        PrintField(item.FolderId?.ToString(), "FolderId");
        PrintField(item.OrganizationId?.ToString(), "OrganizationId");
        PrintCipherField(item.Name);
        PrintCipherField(item.Notes);
        
        switch (item.ItemType)
        {
            case CipherItemType.Login: PrintLoginField(item.Login); break;
            case CipherItemType.Identity: PrintIdentityField(item.Identity); break;
            case CipherItemType.SecureNote: PrintSecureNoteField(item.SecureNote); break;
            case CipherItemType.Card: PrintCardField(item.Card); break;
        }
        
        PrintCollectionsFromIds(item.CollectionsIds);
        PrintCustomFields(item.Fields);
        PrintAttachments(item.Attachments);
        
        Console.WriteLine();
    }

    static void PrintCipherItems()
    {
        foreach (var item in VaultService.GetBitWardenDatabase().Items)
            PrintCipherItem(item);
    }

    static async Task TestFolderAsync()
    {
        var cryptString = new EncryptedString(UserCryptoService);
        
        cryptString.ClearString = "TestDirectory";
        
        var x = cryptString;
        
        var folder = await VaultWebClient.CreateFolderAsync(cryptString.CipherString);
        
        Console.WriteLine($"Folder created: {folder.Id}, {folder.Name.ClearString}");
        
        cryptString.ClearString = "Renamed TestDirectory";
        
        folder = await VaultWebClient.UpdateFolderAsync(folder.Id.Value, cryptString.CipherString);
        
        Console.WriteLine($"Folder updated: {folder.Id}, {folder.Name.ClearString}");
        
        var folders = await VaultWebClient.GetFoldersAsync();
        
        folder = await VaultWebClient.GetFolderAsync(folder.Id.Value);
        
        await VaultWebClient.DeleteFolderAsync(folder.Id.Value);
        
        try
        {
            folder = await VaultWebClient.GetFolderAsync(folder.Id.Value);
        }
        catch (Exception e)
        {
        
        }
    }

    static async Task TestCipherItemAsync()
    {
        var item = await VaultWebClient.GetCipherItemAsync(Guid.Parse("cb3bb34b-6491-4ded-b256-3cc9f76c975f"));

        var attachment = item.Attachments[0];
        var attachmentStream = await VaultWebClient.GetAttachmentAsync(attachment);
        
        var clearStream = new MemoryStream();

        await VaultService.DecryptAttachmentAsync(attachmentStream, attachment.Key.ClearBytes, clearStream);
        clearStream.Position = 0; 
        
        using var fileStream = new FileStream("new_file", FileMode.Open, FileAccess.Read);
        
        var attachmentKey = RandomNumberGenerator.GetBytes(64);
        
        var encryptedName = UserCryptoService.NewEncryptedString();
        encryptedName.ClearString = "test.txt";
        
        var encryptedKey = UserCryptoService.NewEncryptedString();
        encryptedKey.ClearBytes = attachmentKey;

        var cryptedAttachmentStream = new MemoryStream();

        await VaultService.EncryptAttachmentAsync(fileStream, encryptedKey.ClearBytes, cryptedAttachmentStream);
        cryptedAttachmentStream.Position = 0;

        await VaultWebClient.CreateCipherItemAttachmentAsync(item.Id.Value, encryptedName.CipherString, encryptedKey.CipherString, cryptedAttachmentStream);
    }

    static Task<string> WaitForUserOTPAsync()
    {
        Console.Write("Type the received otp code: ");
        return Task.FromResult(Console.ReadLine().Trim());
    }

    static async Task LoadOnlineDatabaseAsync(string masterPassword, string refreshToken)
    {
        // Authenticating with credentials triggers a mail notification and probably an OTP request.
        //await VaultWebClient.PreloginAsync(email);
        //await VaultWebClient.AuthenticateAsync(password, WaitForUserOTPAsync);

        //await VaultWebClient.AuthenticateWithRefreshTokenAsync(refreshToken);

        VaultService.LoadBitWardenDatabase(
            Encoding.UTF8.GetBytes(masterPassword),
            VaultWebClient.UserKdfIterations,
            await VaultWebClient.GetDatabaseAsync());
    }

    static async Task LoadLocalDatabaseAsync(string email)
    {
        using (var stream = new FileStream("example_bitwarden.db", FileMode.Open, FileAccess.Read))
        {
            VaultService.LoadBitWardenDatabase(
                Encoding.UTF8.GetBytes(email),
                600000,
                stream);
        }
    }

    static async Task OnPushNotificationAsyncReceived(PushNotificationBaseModel message)
    {
        
    }

    static async Task MainAsync()
    {
        using (BitWardenDatabaseSessionScope = BitWardenHelper.CreateSessionScope(IWebClientService.BitWardenEUHostUrl, INotificationClientService.BitWardenEUHostUrl))
        {
            var cts = new CancellationTokenSource();

            VaultWebClient = BitWardenDatabaseSessionScope.ServiceProvider.GetRequiredService<IWebClientService>();
            VaultService = BitWardenDatabaseSessionScope.ServiceProvider.GetRequiredService<IVaultService>();
            UserCryptoService = BitWardenDatabaseSessionScope.ServiceProvider.GetRequiredService<IUserCryptoService>();

            await VaultWebClient.AuthenticateWithApiKeyAsync(Environment.GetEnvironmentVariable("SharpWardenUser"), Environment.GetEnvironmentVariable("SharpWardenSecret"));
            // Build NotificationClient after login
            NotificationClient = BitWardenDatabaseSessionScope.ServiceProvider.GetRequiredService<INotificationClientService>();
            await NotificationClient.StartAsync(cts.Token);

            NotificationClient.OnPushNotificationAsyncReceived += OnPushNotificationAsyncReceived;

            await LoadOnlineDatabaseAsync(Environment.GetEnvironmentVariable("SharpWardenMasterPassword"), null);
            //await LoadLocalDatabaseAsync(credentials[1]);

            await TestFolderAsync();
            await TestCipherItemAsync();

            //PrintCipherItems();

            cts.Cancel();

            await Task.Delay(5000);
        }
    }
}
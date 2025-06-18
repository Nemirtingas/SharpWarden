using System.Runtime.CompilerServices;
using System.Text;
using SharpWarden;
using SharpWarden.BitWardenDatabaseSession;
using SharpWarden.BitWardenDatabaseSession.CipherItem.Models;
using SharpWarden.BitWardenDatabaseSession.CollectionItem.Models;
using SharpWarden.BitWardenDatabaseSession.FolderItem.Models;

namespace SharpWardenExamples;

class Example
{
    static DatabaseSession BitwardenDatabaseSession;

    static void Main()
    {
        MainAsync().GetAwaiter().GetResult();
    }

    static void PrintCipherField(EncryptedString str, [CallerArgumentExpression("str")] string name = null)
    {
        name = name?.Split('.').Length > 0
            ? name.Split('.')[^1]
            : name;

        if (str.CipherString != null)
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

    static void PrintLoginField(LoginFieldModel model)
    {
        PrintCipherField(model.Username);
        PrintCipherField(model.Password);
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

        var collections = BitwardenDatabaseSession.GetItemCollections(collectionsIds);

        foreach (var collection in collections)
        {
            PrintCollection(collection);
            Console.WriteLine();
        }
    }

    static void PrintCipherItem(CipherItemModel item)
    {
        string folder = BitwardenDatabaseSession.GetItemFolder(item.FolderId)?.Name.ClearString;
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

    static void PrintCipherItems(IEnumerable<CipherItemModel> items)
    {
        foreach (var item in items)
            PrintCipherItem(item);
    }

    static async Task TestFolderAsync(SharpWarden.WebClient.WebClient webClient)
    {
        webClient.LinkDatabaseSession(BitwardenDatabaseSession);
        var cryptString = new EncryptedString(BitwardenDatabaseSession);

        cryptString.ClearString = "TestDirectory";

        var x = cryptString;

        var folder = await webClient.CreateFolderAsync(cryptString.CipherString);

        Console.WriteLine($"Folder created: {folder.Id}, {folder.Name.ClearString}");

        cryptString.ClearString = "Renamed TestDirectory";

        folder = await webClient.UpdateFolderAsync(folder.Id.Value, cryptString.CipherString);

        Console.WriteLine($"Folder updated: {folder.Id}, {folder.Name.ClearString}");

        var folders = await webClient.GetFoldersAsync();

        folder = await webClient.GetFolderAsync(folder.Id.Value);

        await webClient.DeleteFolderAsync(folder.Id.Value);

        try
        {
            folder = await webClient.GetFolderAsync(folder.Id.Value);
        }
        catch (Exception e)
        {

        }
    }

    static async Task TestCipherItemAsync(SharpWarden.WebClient.WebClient webClient)
    {
        webClient.LinkDatabaseSession(BitwardenDatabaseSession);

        var item = await webClient.GetCipherItemAsync(Guid.Parse("c6a7e420-268b-465f-8f87-f8a16969da89"));
        item.SetDatabaseSession(BitwardenDatabaseSession, item.OrganizationId);

        var attachment = await webClient.GetAttachmentAsync(item.Attachments[4]);

        //var item = new CipherItemModel(BitwardenDatabaseSession);

        //var login = item.CreateLogin();
        //item.Name = new(BitwardenDatabaseSession);
        //item.Name.ClearString = "TestEntry";

        //login.Username = new(BitwardenDatabaseSession);
        //login.Username.ClearString = "TestUser";

        //await webClient.CreateCipherItemAsync(item);
    }

    static async Task MainAsync()
    {
        const string bitWardenHost = "https://bitwarden.com";

        var credentials = default(string[]);

        // Example file format: mail|password|refreshToken
        using (var fileStream = new FileStream("logins", FileMode.Open))
        {
            credentials = (await new StreamReader(fileStream).ReadToEndAsync()).Trim().Split("|", 3);
        }

        var vaultwardenWebClient = new SharpWarden.WebClient.WebClient(bitWardenHost);

        await vaultwardenWebClient.PreloginAsync(credentials[0]);

        // Authenticating with credentials triggers a mail notification.
        // TODO: API key authentication
        //await vaultwardenWebClient.AuthenticateAsync(credentials[1]);
        await vaultwardenWebClient.AuthenticateWithRefreshTokenAsync(credentials[2]);

        //Console.WriteLine($"Refresh token: {vaultwardenWebClient.GetWebSession().RefreshToken}");

        BitwardenDatabaseSession = new();
        //await BitwardenDatabaseSession.LoadBitWardenKeysAsync(
        //    Encoding.UTF8.GetBytes(credentials[0]),
        //    Encoding.UTF8.GetBytes(credentials[1]),
        //    vaultwardenWebClient.UserKdfIterations,
        //    vaultwardenWebClient.UserKey,
        //    vaultwardenWebClient.UserPrivateKey
        //);

        await BitwardenDatabaseSession.LoadBitWardenDatabaseAsync(
            Encoding.UTF8.GetBytes(credentials[1]),
            vaultwardenWebClient.UserKdfIterations,
            await vaultwardenWebClient.GetDatabaseAsync());

        //using (var stream = new FileStream("example_bitwarden.db", FileMode.Open, FileAccess.Read))
        //{
        //    await BitwardenDatabaseSession.LoadBitWardenDatabaseAsync(
        //        Encoding.UTF8.GetBytes(credentials[1]),
        //        600000,
        //        stream);
        //}

        await TestFolderAsync(vaultwardenWebClient);
        await TestCipherItemAsync(vaultwardenWebClient);

        //var cipherItems = BitwardenDatabaseSession.Database.Items;
        //var cipherItems = await vaultwardenWebClient.GetCipherItemsAsync();

        //var collections = BitwardenDatabaseSession.Database.Collections;

        //PrintCipherItems(cipherItems);
    }
}
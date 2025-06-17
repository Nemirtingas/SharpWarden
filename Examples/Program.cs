using System.Runtime.CompilerServices;
using System.Text;
using SharpWarden;
using SharpWarden.BitWardenDatabaseSession;
using SharpWarden.BitWardenDatabaseSession.FolderItem.Models;

namespace SharpWardenExamples;

class Example
{
    static DatabaseSession BitwardenDatabaseSession;

    static void Main()
    {
        MainAsync().GetAwaiter().GetResult();
    }

    static void PrintCipherField(string str, [CallerArgumentExpression("str")] string name = null)
    {
        name = name?.Split('.').Length > 0
            ? name.Split('.')[^1]
            : name;

        if (str != null)
            Console.WriteLine($"  {name}: {str}");
        else
            Console.WriteLine($"  {name}: (null)");
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

    static void PrintLoginField(SharpWarden.BitWardenDatabase.CipherItem.Models.LoginFieldModel model)
    {
        PrintCipherField(model.Username);
        PrintCipherField(model.Password);
    }

    static void PrintLoginField(SharpWarden.BitWardenDatabaseSession.CipherItem.Models.LoginFieldModel model)
    {
        PrintCipherField(model.Username);
        PrintCipherField(model.Password);
    }

    static void PrintIdentityField(SharpWarden.BitWardenDatabase.CipherItem.Models.IdentityFieldModel model)
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

    static void PrintIdentityField(SharpWarden.BitWardenDatabaseSession.CipherItem.Models.IdentityFieldModel model)
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

    static void PrintSecureNoteField(SharpWarden.BitWardenDatabase.CipherItem.Models.SecureNoteFieldModel model)
    {
    }

    static void PrintSecureNoteField(SharpWarden.BitWardenDatabaseSession.CipherItem.Models.SecureNoteFieldModel model)
    {
    }

    static void PrintCardField(SharpWarden.BitWardenDatabase.CipherItem.Models.CardFieldModel model)
    {
        PrintCipherField(model.Brand);
        PrintCipherField(model.CardholderName);
        PrintCipherField(model.Code);
        PrintCipherField(model.ExpMonth);
        PrintCipherField(model.ExpYear);
        PrintCipherField(model.Number);
    }

    static void PrintCardField(SharpWarden.BitWardenDatabaseSession.CipherItem.Models.CardFieldModel model)
    {
        PrintCipherField(model.Brand);
        PrintCipherField(model.CardholderName);
        PrintCipherField(model.Code);
        PrintCipherField(model.ExpMonth);
        PrintCipherField(model.ExpYear);
        PrintCipherField(model.Number);
    }

    static void PrintCustomField(SharpWarden.BitWardenDatabase.CipherItem.Models.CustomFieldModel field)
    {
        Console.WriteLine("  Custom field");
        PrintField(field.Type.ToString(), "  Type");
        PrintCipherField(field.Name, "  Name");
        PrintCipherField(field.Value, "  Value");
        PrintField(field.LinkedId == null ? null : field.LinkedId.ToString(), "  LinkedId");
    }

    static void PrintCustomField(SharpWarden.BitWardenDatabaseSession.CipherItem.Models.CustomFieldModel field)
    {
        Console.WriteLine("  Custom field");
        PrintField(field.Type.ToString(), "  Type");
        PrintCipherField(field.Name, "  Name");
        PrintCipherField(field.Value, "  Value");
        PrintField(field.LinkedId == null ? null : field.LinkedId.ToString(), "  LinkedId");
    }

    static void PrintCustomFields(IEnumerable<SharpWarden.BitWardenDatabase.CipherItem.Models.CustomFieldModel> fields)
    {
        if (fields?.Any() != true)
            return;

        foreach (var field in fields)
        {
            PrintCustomField( field);
            Console.WriteLine();
        }
    }

    static void PrintCustomFields(IEnumerable<SharpWarden.BitWardenDatabaseSession.CipherItem.Models.CustomFieldModel> fields)
    {
        if (fields?.Any() != true)
            return;

        foreach (var field in fields)
        {
            PrintCustomField(field);
            Console.WriteLine();
        }
    }

    static void PrintAttachment(SharpWarden.BitWardenDatabase.CipherItem.Models.AttachmentModel attachement)
    {
        Console.WriteLine($"  Attachement id: {attachement.Id}");
        PrintCipherField( attachement.FileName, "  FileName");
        PrintField(attachement.Key, "  Key");
        PrintField(attachement.Size.ToString(), "  Size");
        PrintField(attachement.Url, "  Url");
    }

    static void PrintAttachment(SharpWarden.BitWardenDatabaseSession.CipherItem.Models.AttachmentModel attachement)
    {
        Console.WriteLine($"  Attachement id: {attachement.Id}");
        PrintCipherField(attachement.FileName, "  FileName");
        PrintField(Convert.ToHexString(attachement.Key), "  Key");
        PrintField(attachement.Size.ToString(), "  Size");
        PrintField(attachement.Url, "  Url");
    }

    static void PrintAttachments(IEnumerable<SharpWarden.BitWardenDatabase.CipherItem.Models.AttachmentModel> attachments)
    {
        if (attachments?.Any() != true)
            return;

        foreach (var attachement in attachments)
        {
            PrintAttachment( attachement);
            Console.WriteLine();
        }
    }

    static void PrintAttachments(IEnumerable<SharpWarden.BitWardenDatabaseSession.CipherItem.Models.AttachmentModel> attachments)
    {
        if (attachments?.Any() != true)
            return;

        foreach (var attachement in attachments)
        {
            PrintAttachment(attachement);
            Console.WriteLine();
        }
    }

    static void PrintCollection(SharpWarden.BitWardenDatabaseSession.CollectionItem.Models.CollectionItemModel collection)
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

    static void PrintCipherItem(SharpWarden.BitWardenDatabase.CipherItem.Models.CipherItemModel item)
    {

        Console.WriteLine("================================");
        Console.WriteLine($"Item: {item.Id}, path: ");

        PrintField(item.FolderId?.ToString(), "FolderId");
        PrintField(item.OrganizationId?.ToString(), "OrganizationId");
        PrintCipherField(item.Name);
        PrintCipherField(item.Notes);
        

        switch (item.ItemType)
        {
            case SharpWarden.BitWardenDatabase.CipherItem.Models.CipherItemType.Login: PrintLoginField(item.Login); break;
            case SharpWarden.BitWardenDatabase.CipherItem.Models.CipherItemType.Identity: PrintIdentityField(item.Identity); break;
            case SharpWarden.BitWardenDatabase.CipherItem.Models.CipherItemType.SecureNote: PrintSecureNoteField(item.SecureNote); break;
            case SharpWarden.BitWardenDatabase.CipherItem.Models.CipherItemType.Card: PrintCardField(item.Card); break;
        }

        PrintCustomFields(item.Fields);
        PrintAttachments(item.Attachments);

        Console.WriteLine();
    }

    static void PrintCipherItem(SharpWarden.BitWardenDatabaseSession.CipherItem.Models.CipherItemModel item)
    {
        string folder = BitwardenDatabaseSession.GetItemFolder(item.FolderId)?.Name;
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
            case SharpWarden.BitWardenDatabase.CipherItem.Models.CipherItemType.Login: PrintLoginField(item.Login); break;
            case SharpWarden.BitWardenDatabase.CipherItem.Models.CipherItemType.Identity: PrintIdentityField(item.Identity); break;
            case SharpWarden.BitWardenDatabase.CipherItem.Models.CipherItemType.SecureNote: PrintSecureNoteField(item.SecureNote); break;
            case SharpWarden.BitWardenDatabase.CipherItem.Models.CipherItemType.Card: PrintCardField(item.Card); break;
        }

        PrintCollectionsFromIds(item.CollectionsIds);
        PrintCustomFields(item.Fields);
        PrintAttachments(item.Attachments);

        Console.WriteLine();
    }

    static void PrintCipherItems(IEnumerable<SharpWarden.BitWardenDatabase.CipherItem.Models.CipherItemModel> items)
    {
        foreach (var item in items)
            PrintCipherItem(item);
    }

    static void PrintCipherItems(IEnumerable<SharpWarden.BitWardenDatabaseSession.CipherItem.Models.CipherItemModel> items)
    {
        foreach (var item in items)
            PrintCipherItem(item);
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
        await BitwardenDatabaseSession.LoadBitWardenKeysAsync(
            Encoding.UTF8.GetBytes(credentials[0]),
            Encoding.UTF8.GetBytes(credentials[1]),
            vaultwardenWebClient.UserKdfIterations,
            vaultwardenWebClient.UserKey,
            vaultwardenWebClient.UserPrivateKey
        );

        //await BitwardenDatabaseSession.LoadBitWardenDatabaseAsync(
        //    Encoding.UTF8.GetBytes(credentials[1]),
        //    vaultwardenWebClient.UserKdfIterations,
        //    await vaultwardenWebClient.GetDatabaseAsync());

        //var cryptString = BitwardenDatabaseSession.CryptClearStringWithMasterKey(null, "Hello!");
        //var clearString = BitwardenDatabaseSession.GetClearStringWithMasterKey(null, cryptString);

        //cryptString = BitwardenDatabaseSession.CryptClearStringWithMasterKey(null, "TestDirectory");
        //clearString = BitwardenDatabaseSession.GetClearStringWithMasterKey(null, cryptString);

        var cryptString = new EncryptedString(BitwardenDatabaseSession);

        cryptString.ClearString = "TestDirectory";

        var folder = await vaultwardenWebClient.CreateFolderAsync(cryptString.CipherString);

        await vaultwardenWebClient.UpdateFolderAsync(folder.Id.Value, cryptString.CipherString);

        var f = new FolderItemModel(BitwardenDatabaseSession, folder);

        await vaultwardenWebClient.DeleteFolderAsync(folder.Id.Value);

        //var cipherItems = BitwardenDatabaseSession.Database.Items;
        //var cipherItems = await vaultwardenWebClient.GetCipherItemsAsync();

        //var collections = BitwardenDatabaseSession.Database.Collections;

        //PrintCipherItems(cipherItems);
    }
}
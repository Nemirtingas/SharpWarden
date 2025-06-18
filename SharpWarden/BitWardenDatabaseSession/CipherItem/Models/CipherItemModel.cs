using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Models;

namespace SharpWarden.BitWardenDatabaseSession.CipherItem.Models;

public class CipherItemModel : IDatabaseSessionModel
{
    private DatabaseSession _DatabaseSession;

    public CipherItemModel(DatabaseSession databaseSession)
    {
        SetDatabaseSession(databaseSession);
    }

    public bool HasSession() => _DatabaseSession != null;

    public void SetDatabaseSession(DatabaseSession databaseSession)
    {
        _DatabaseSession = databaseSession;

        Card?.SetDatabaseSession(_DatabaseSession);
        Identity?.SetDatabaseSession(_DatabaseSession);
        Login?.SetDatabaseSession(_DatabaseSession);
        Name?.SetDatabaseSession(_DatabaseSession);
        Notes?.SetDatabaseSession(_DatabaseSession);
        SSHKey?.SetDatabaseSession(_DatabaseSession);

        if (Attachments != null)
            foreach (var v in Attachments)
                v.SetDatabaseSession(databaseSession);

        if (Fields != null)
            foreach (var v in Fields)
                v.SetDatabaseSession(databaseSession);

        if (PasswordHistory != null)
            foreach (var v in PasswordHistory)
                v.SetDatabaseSession(_DatabaseSession);
    }

    public void SetDatabaseSession(DatabaseSession databaseSession, Guid? organizationId)
    {
        _DatabaseSession = databaseSession;
        OrganizationId = organizationId;

        Card?.SetDatabaseSession(_DatabaseSession, OrganizationId);
        Identity?.SetDatabaseSession(_DatabaseSession, OrganizationId);
        Login?.SetDatabaseSession(_DatabaseSession, OrganizationId);
        Name?.SetDatabaseSession(_DatabaseSession, OrganizationId);
        Notes?.SetDatabaseSession(_DatabaseSession, OrganizationId);
        SSHKey?.SetDatabaseSession(_DatabaseSession, OrganizationId);

        if (Attachments != null)
            foreach (var v in Attachments)
                v.SetDatabaseSession(_DatabaseSession, OrganizationId);

        if (Fields != null)
            foreach (var v in Fields)
                v.SetDatabaseSession(_DatabaseSession, OrganizationId);

        if (PasswordHistory != null)
            foreach (var v in PasswordHistory)
                v.SetDatabaseSession(_DatabaseSession, OrganizationId);

    }

    public LoginFieldModel CreateLogin()
    {
        ItemType = CipherItemType.Login;
        Login = new LoginFieldModel(_DatabaseSession);
        return Login;
    }

    public SecureNoteFieldModel CreateSecureNote()
    {
        ItemType = CipherItemType.SecureNote;
        SecureNote = new SecureNoteFieldModel();
        return SecureNote;
    }

    public IdentityFieldModel CreateIdentity()
    {
        ItemType = CipherItemType.Identity;
        Identity = new IdentityFieldModel(_DatabaseSession);
        return Identity;
    }

    public CardFieldModel CreateCard()
    {
        ItemType = CipherItemType.Card;
        Card = new CardFieldModel(_DatabaseSession);
        return Card;
    }

    [JsonProperty("attachments")]
    public List<AttachmentModel> Attachments { get; set; }

    [JsonProperty("card")]
    public CardFieldModel Card { get; set; }

    [JsonProperty("collectionIds")]
    public List<Guid> CollectionsIds { get; set; }

    [JsonProperty("creationDate")]
    public DateTime? CreationDate { get; set; }

    // Ignore data item, its dynamic and contains one of the Card/Identity/Login/SecureNote object + some other fields like Fields.
    //[JsonProperty("data")]
    //public object Data { get; set; }

    [JsonProperty("deletedDate")]
    public DateTime? DeletedDate { get; set; }

    [JsonProperty("edit")]
    public bool Edit { get; set; }

    [JsonProperty("favorite")]
    public bool Favorite { get; set; }

    [JsonProperty("fields")]
    public List<CustomFieldModel> Fields { get; set; }

    [JsonProperty("folderId")]
    public Guid? FolderId { get; set; }

    [JsonProperty("id")]
    public Guid? Id { get; set; }

    [JsonProperty("identity")]
    public IdentityFieldModel Identity { get; set; }

    [JsonProperty("key")]
    public string Key { get; set; }

    [JsonProperty("login")]
    public LoginFieldModel Login { get; set; }

    [JsonProperty("name")]
    public EncryptedString Name { get; set; }

    [JsonProperty("notes")]
    public EncryptedString Notes { get; set; }

    [JsonProperty("object")]
    public ObjectType ObjectType { get; set; } = ObjectType.CipherDetails;

    [JsonProperty("organizationId")]
    public Guid? OrganizationId { get; set; }

    [JsonProperty("organizationUseTotp")]
    public bool OrganizationUseTOTP { get; set; }

    [JsonProperty("passwordHistory")]
    public List<PasswordHistoryModel> PasswordHistory { get; set; }

    [JsonProperty("reprompt")]
    public CipherRepromptType Reprompt { get; set; }

    [JsonProperty("revisionDate")]
    public DateTime? RevisionDate { get; set; }

    [JsonProperty("secureNote")]
    public SecureNoteFieldModel SecureNote { get; set; }

    [JsonProperty("sshKey")]
    public EncryptedString SSHKey { get; set; }

    [JsonProperty("type")]
    public CipherItemType ItemType { get; set; }

    [JsonProperty("viewPassword")]
    public bool ViewPassword { get; set; }
}

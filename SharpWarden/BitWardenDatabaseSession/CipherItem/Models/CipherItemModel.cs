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

        if (Attachments != null)
            foreach (var v in Attachments)
                v.SetDatabaseSession(databaseSession);

        Card?.SetDatabaseSession(_DatabaseSession);

        if (Fields != null)
            foreach (var v in Fields)
                v.SetDatabaseSession(databaseSession);

        Identity?.SetDatabaseSession(_DatabaseSession);
        Login?.SetDatabaseSession(_DatabaseSession);

        Name = new EncryptedString(Name.CipherString, _DatabaseSession);
        Notes = new EncryptedString(Notes.CipherString, _DatabaseSession);
        SSHKey = new EncryptedString(SSHKey.CipherString, _DatabaseSession);
    }

    public void SetDatabaseSession(DatabaseSession databaseSession, Guid? organizationId)
    {
        _DatabaseSession = databaseSession;
        OrganizationId = organizationId;

        if (Attachments != null)
            foreach (var v in Attachments)
                v.SetDatabaseSession(_DatabaseSession, OrganizationId);

        Card?.SetDatabaseSession(_DatabaseSession, OrganizationId);

        if (Fields != null)
            foreach (var v in Fields)
                v.SetDatabaseSession(_DatabaseSession, OrganizationId);

        Identity?.SetDatabaseSession(_DatabaseSession, OrganizationId);
        Login?.SetDatabaseSession(_DatabaseSession, OrganizationId);
        Name = new EncryptedString(Name.CipherString, _DatabaseSession, OrganizationId);
        Notes = new EncryptedString(Notes.CipherString, _DatabaseSession, OrganizationId);
        SSHKey = new EncryptedString(SSHKey.CipherString, _DatabaseSession, OrganizationId);
    }

    [JsonProperty("attachments")]
    public List<AttachmentModel> Attachments { get; set; }

    [JsonProperty("card")]
    public CardFieldModel Card { get; set; }

    [JsonProperty("collectionIds")]
    public List<Guid> CollectionsIds { get; set; }

    [JsonProperty("creationDate")]
    public DateTimeOffset? CreationDate { get; set; }

    // Ignore data item, its dynamic and contains one of the Card/Identity/Login/SecureNote object + some other fields like Fields.
    //[JsonProperty("data")]
    //public object Data { get; set; }

    [JsonProperty("deletedDate")]
    public DateTimeOffset? DeletedDate { get; set; }

    [JsonProperty("edit")]
    public bool Edit { get; set; }

    [JsonProperty("favorite")]
    public bool Favorite { get; set; }

    [JsonProperty("fields")]
    public List<CustomFieldModel> Fields { get; set; }

    [JsonProperty("folderId")]
    public Guid? FolderId { get; set; }

    [JsonProperty("id")]
    public Guid Id { get; set; }

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
    public int Reprompt { get; set; }

    [JsonProperty("revisionDate")]
    public DateTimeOffset? RevisionDate { get; set; }

    [JsonProperty("secureNote")]
    public SecureNoteFieldModel SecureNote { get; set; }

    [JsonProperty("sshKey")]
    public EncryptedString SSHKey { get; set; }

    [JsonProperty("type")]
    public CipherItemType ItemType { get; set; }

    [JsonProperty("viewPassword")]
    public bool ViewPassword { get; set; }
}

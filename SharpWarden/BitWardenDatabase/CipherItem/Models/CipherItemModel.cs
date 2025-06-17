using Newtonsoft.Json;

namespace SharpWarden.BitWardenDatabase.CipherItem.Models;

public class CipherItemModel
{
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
    public string Name { get; set; }

    [JsonProperty("notes")]
    public string Notes { get; set; }

    [JsonProperty("object")]
    public ObjectType ObjectType { get; set; } = ObjectType.CipherDetails;

    [JsonProperty("organizationId")]
    public Guid? OrganizationId { get; set; }

    [JsonProperty("organizationUseTotp")]
    public bool OrganizationUseTopt { get; set; }

    [JsonProperty("passwordHistory")]
    public List<PasswordHistoryModel> PasswordHistory { get; set; }

    [JsonProperty("reprompt")]
    public int Reprompt { get; set; }

    [JsonProperty("revisionDate")]
    public DateTimeOffset? RevisionDate { get; set; }

    [JsonProperty("secureNote")]
    public SecureNoteFieldModel SecureNote { get; set; }

    [JsonProperty("sshKey")]
    public string SshKey { get; set; }

    [JsonProperty("type")]
    public CipherItemType ItemType { get; set; }

    [JsonProperty("viewPassword")]
    public bool ViewPassword { get; set; }
}
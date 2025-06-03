using Newtonsoft.Json;

namespace BitwardenSharp.Models;

public class BitWardenItemModel
{
    [JsonProperty("attachments")]
    public List<BitWardenItemAttachementModel> Attachments { get; set; }

    //[JsonProperty("card")]
    //public object Card { get; set; }

    [JsonProperty("collectionIds")]
    public List<Guid> CollectionsIds { get; set; }

    [JsonProperty("creationDate")]
    public DateTimeOffset? CreationDate { get; set; }

    //[JsonProperty("data")]
    //public object Data { get; set; }

    [JsonProperty("deletedDate")]
    public DateTimeOffset? DeletedDate { get; set; }

    [JsonProperty("edit")]
    public bool Edit { get; set; }

    [JsonProperty("favorite")]
    public bool Favorite { get; set; }

    [JsonProperty("fields")]
    public List<BitWardenItemCustomFieldModel> Fields { get; set; }

    [JsonProperty("folderId")]
    public Guid? FolderId { get; set; }

    [JsonProperty("id")]
    public Guid? Id { get; set; }

    //[JsonProperty("identity")]
    //public object Identity { get; set; }

    //[JsonProperty("key")]
    //public object Key { get; set; }

    [JsonProperty("login")]
    public BitWardenItemLoginFieldModel Login { get; set; }

    [JsonProperty("name")]
    public BitWardenEncryptedString Name { get; set; }

    //[JsonProperty("notes")]
    //public object Notes { get; set; }

    [JsonProperty("object")]
    public string Object { get; set; }

    [JsonProperty("organizationId")]
    public Guid? OrganizationId { get; set; }

    [JsonProperty("organizationUseTotp")]
    public bool OrganizationUseTopt { get; set; }

    [JsonProperty("passwordHistory")]
    public List<BitWardenItemPasswordHistoryModel> PasswordHistory { get; set; }

    [JsonProperty("reprompt")]
    public int Reprompt { get; set; }

    [JsonProperty("revisionDate")]
    public DateTimeOffset? RevisionDate { get; set; }

    [JsonProperty("secureNote")]
    public object SecureNote { get; set; }

    [JsonProperty("sshKey")]
    public BitWardenEncryptedString SshKey { get; set; }

    [JsonProperty("type")]
    public int Type { get; set; }

    [JsonProperty("viewPassword")]
    public bool ViewPassword { get; set; }
}
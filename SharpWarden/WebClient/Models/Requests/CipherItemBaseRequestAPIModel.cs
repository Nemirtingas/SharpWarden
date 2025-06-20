using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

namespace SharpWarden.WebClient.Models;

public abstract class CipherItemBaseRequestAPIModel
{
    [JsonProperty("type")]
    public abstract CipherItemType ItemType { get; }

    [JsonProperty("folderId")]
    public Guid? FolderId { get; set; }

    [JsonProperty("organizationId")]
    public Guid? OrganizationId { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("notes")]
    public string Notes { get; set; }

    [JsonProperty("fields")]
    public List<CustomFieldAPIModel> Fields { get; set; }

    [JsonProperty("favorite")]
    public bool Favorite { get; set; }

    [JsonProperty("lastKnownRevisionDate")]
    public DateTime? LastKnownRevisionDate { get; set; }

    [JsonProperty("reprompt")]
    public CipherRepromptType Reprompt { get; set; }
}
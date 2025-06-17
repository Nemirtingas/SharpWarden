using Newtonsoft.Json;

namespace SharpWarden.BitWardenDatabase.CollectionItem.Models;

public class CollectionItemModel
{
    [JsonProperty("externalId")]
    public Guid? ExternalId { get; set; }

    [JsonProperty("hidePasswords")]
    public bool HidePasswords { get; set; }

    [JsonProperty("id")]
    public Guid? Id { get; set; }

    [JsonProperty("manage")]
    public bool Manage { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("object")]
    public ObjectType ObjectType { get; set; } = ObjectType.CollectionDetails;

    [JsonProperty("organizationId")]
    public Guid? OrganizationId { get; set; }

    [JsonProperty("readOnly")]
    public bool ReadOnly { get; set; }
}
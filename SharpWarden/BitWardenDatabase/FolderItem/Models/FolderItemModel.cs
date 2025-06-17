using Newtonsoft.Json;

namespace SharpWarden.BitWardenDatabase.FolderItem.Models;

public class FolderItemModel
{
    [JsonProperty("id")]
    public Guid? Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("object")]
    public ObjectType ObjectType { get; set; } = ObjectType.Folder;

    [JsonProperty("revisionDate")]
    public DateTimeOffset? RevisionDate { get; set; }
}
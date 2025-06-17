using Newtonsoft.Json;

namespace SharpWarden.BitWardenDatabase.CipherItem.Models;

public class AttachmentModel
{
    [JsonProperty("fileName")]
    public string FileName { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("key")]
    public string Key { get; set; }

    [JsonProperty("object")]
    public ObjectType ObjectType { get; set; } = ObjectType.Attachment;

    [JsonProperty("size")]
    public int Size { get; set; }

    [JsonProperty("sizeName")]
    public string SizeName { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }
}
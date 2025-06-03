using Newtonsoft.Json;

namespace BitwardenSharp.Models;

public class BitWardenItemAttachementModel
{
    [JsonProperty("fileName")]
    public BitWardenEncryptedString FileName { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("key")]
    public BitWardenEncryptedString Key { get; set; }

    [JsonProperty("object")]
    public BitWardenObjectType ObjectType { get; set; }

    [JsonProperty("size")]
    public int Size { get; set; }

    [JsonProperty("sizeName")]
    public string SizeName { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }
}
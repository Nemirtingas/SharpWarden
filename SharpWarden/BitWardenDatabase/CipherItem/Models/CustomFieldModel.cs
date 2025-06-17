using Newtonsoft.Json;

namespace SharpWarden.BitWardenDatabase.CipherItem.Models;

public class CustomFieldModel
{
    [JsonProperty("linkedId")]
    public CustomFieldLinkType? LinkedId { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("type")]
    public CustomFieldType Type { get; set; }

    [JsonProperty("value")]
    public string Value { get; set; }
}
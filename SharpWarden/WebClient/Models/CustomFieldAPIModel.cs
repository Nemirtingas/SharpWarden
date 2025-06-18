using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.CipherItem.Models;

namespace SharpWarden.WebClient.Models;

public class CustomFieldAPIModel
{
    [JsonProperty("type")]
    public CustomFieldType Type { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("value")]
    public string Value { get; set; }

    [JsonProperty("linkedId")]
    public CustomFieldLinkType? LinkedId { get; set; }
}
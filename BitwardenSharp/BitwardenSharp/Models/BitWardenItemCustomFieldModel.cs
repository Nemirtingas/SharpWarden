using Newtonsoft.Json;

namespace BitwardenSharp.Models;

public enum BitWardenItemCustomFieldType
{
    Text = 0,
    Hidden = 1,
    Boolean = 2,
    Linked = 3,
}

public class BitWardenItemCustomFieldModel
{
    [JsonProperty("linkedId")]
    public Guid? linkedId { get; set; }

    [JsonProperty("name")]
    public BitWardenEncryptedString Name { get; set; }

    [JsonProperty("type")]
    public BitWardenItemCustomFieldType Type { get; set; }

    [JsonProperty("value")]
    public BitWardenEncryptedString Value { get; set; }
}
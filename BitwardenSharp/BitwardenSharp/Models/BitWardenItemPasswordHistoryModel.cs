using Newtonsoft.Json;

namespace BitwardenSharp.Models;

public class BitWardenItemPasswordHistoryModel
{
    [JsonProperty("lastUsedDate")]
    public DateTimeOffset? LastUsedDate { get; set; }

    [JsonProperty("password")]
    public string Password { get; set; }
}
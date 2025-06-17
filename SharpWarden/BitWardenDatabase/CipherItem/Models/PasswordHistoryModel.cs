using Newtonsoft.Json;

namespace SharpWarden.BitWardenDatabase.CipherItem.Models;

public class PasswordHistoryModel
{
    [JsonProperty("lastUsedDate")]
    public DateTimeOffset? LastUsedDate { get; set; }

    [JsonProperty("password")]
    public string Password { get; set; }
}
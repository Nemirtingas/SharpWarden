using Newtonsoft.Json;

namespace BitwardenSharp.Models;

public class BitWardenDatabaseModel
{
    [JsonProperty("ciphers")]
    public List<BitWardenItemModel> Items { get; set; }

    [JsonProperty("profile")]
    public BitWardenProfileModel Profile { get; set; }
}
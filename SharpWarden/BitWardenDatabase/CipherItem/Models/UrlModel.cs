using Newtonsoft.Json;

namespace SharpWarden.BitWardenDatabase.CipherItem.Models;

public class UrlModel
{
    [JsonProperty("match")]
    public UrlMatchType? Match { get; set; }

    [JsonProperty("uri")]
    public string Uri { get; set; }

    [JsonProperty("uriChecksum")]
    public string UriChecksum { get; set; }
}
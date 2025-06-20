using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

namespace SharpWarden.WebClient.Models;

public class CipherLoginUriAPIModel
{
    [JsonProperty("uri")]
    public string Uri { get; set; }
    
    [JsonProperty("uriChecksum")]
    public string UriChecksum { get; set; }

    [JsonProperty("match")]
    public UriMatchType? Match { get; set; }
}
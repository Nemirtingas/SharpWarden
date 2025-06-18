using Newtonsoft.Json;

namespace SharpWarden.WebClient.Models;

public class MultiDeleteRequestAPIModel
{
    [JsonProperty("ids")]
    public List<Guid> Ids { get; set; }
}
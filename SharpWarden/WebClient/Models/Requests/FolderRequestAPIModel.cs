using Newtonsoft.Json;

namespace SharpWarden.WebClient.Models;

public class FolderRequestAPIModel
{
    [JsonProperty("name")]
    public string Name { get; set; }
}
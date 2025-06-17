using Newtonsoft.Json;

namespace SharpWarden.WebClient.Models;

public class UpdateFolderAPIModel
{
    [JsonProperty("name")]
    public string Name { get; set; }
}
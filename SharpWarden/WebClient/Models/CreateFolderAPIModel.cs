using Newtonsoft.Json;

namespace SharpWarden.WebClient.Models;

public class CreateFolderAPIModel
{
    [JsonProperty("name")]
    public string Name { get; set; }
}
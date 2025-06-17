using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpWarden.BitWardenDatabaseSession;

namespace SharpWarden.WebClient.Models;

public class ApiResultModel
{
    [JsonProperty("continuationToken")]
    public string ContinuationToken { get; set; }

    [JsonProperty("object")]
    public ObjectType ObjectType { get; set; }

    [JsonProperty("data")]
    public JObject Data { get; set; }
}

public class ApiResultModel<T>
{
    [JsonProperty("continuationToken")]
    public string ContinuationToken { get; set; }

    [JsonProperty("object")]
    public ObjectType ObjectType { get; set; }

    [JsonProperty("data")]
    public T Data { get; set; }
}
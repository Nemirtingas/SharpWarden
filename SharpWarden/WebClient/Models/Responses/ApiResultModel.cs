using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession;

namespace SharpWarden.WebClient.Models;

public class ApiResultModel
{
    [JsonProperty("object")]
    public ObjectType ObjectType { get; set; }
}

public class ApiResultModel<T> : ApiResultModel
{
    [JsonProperty("continuationToken")]
    public string ContinuationToken { get; set; }

    [JsonProperty("data")]
    public T Data { get; set; }
}
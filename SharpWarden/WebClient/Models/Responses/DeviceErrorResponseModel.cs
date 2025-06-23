using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession;

namespace SharpWarden.WebClient.Models;

public class DeviceErrorResponseModel
{
    [JsonProperty("Message")]
    public string Message { get; set; }

    [JsonProperty("Object")]
    public ObjectType ObjectType { get; set; }
}
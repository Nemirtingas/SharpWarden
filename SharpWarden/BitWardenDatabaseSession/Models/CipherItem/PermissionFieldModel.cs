using Newtonsoft.Json;

namespace SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

public class PermissionFieldModel
{
    public PermissionFieldModel()
    {
    }

    [JsonProperty("delete")]
    public bool Delete { get; set; }

    [JsonProperty("restore")]
    public bool Restore { get; set; }
}
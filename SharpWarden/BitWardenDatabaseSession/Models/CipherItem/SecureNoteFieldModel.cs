using Newtonsoft.Json;

namespace SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

public class SecureNoteFieldModel
{
    // Always 0? The not is stored in the 'notes' field in the item.
    [JsonProperty("type")]
    public SecureNoteType Type { get; set; }
}
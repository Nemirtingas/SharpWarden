using Newtonsoft.Json;

namespace SharpWarden.BitWardenDatabase.CipherItem.Models;

public class SecureNoteFieldModel
{
    // Always 0? The not is stored in the 'notes' field in the item.
    [JsonProperty("type")]
    public int Type { get; set; }
}
using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.CipherItem.Models;

namespace SharpWarden.WebClient.Models;

public class CipherSecureNoteAPIModel
{
    [JsonProperty("type")]
    public SecureNoteType SecureNoteType { get; set; }
}
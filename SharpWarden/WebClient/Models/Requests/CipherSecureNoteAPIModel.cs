using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

namespace SharpWarden.WebClient.Models;

public class CipherSecureNoteAPIModel
{
    [JsonProperty("type")]
    public SecureNoteType SecureNoteType { get; set; }
}
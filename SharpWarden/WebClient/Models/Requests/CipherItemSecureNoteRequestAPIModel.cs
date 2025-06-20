using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

namespace SharpWarden.WebClient.Models;

public class CipherItemSecureNoteRequestAPIModel : CipherItemBaseRequestAPIModel
{
    [JsonProperty("type")]
    public override CipherItemType ItemType => CipherItemType.SecureNote;

    [JsonProperty("secureNote")]
    public CipherSecureNoteAPIModel SecureNote { get; set; }
}
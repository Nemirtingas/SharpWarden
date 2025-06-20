using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

namespace SharpWarden.WebClient.Models;

public class CipherItemIdentityRequestAPIModel : CipherItemBaseRequestAPIModel
{
    [JsonProperty("type")]
    public override CipherItemType ItemType => CipherItemType.Identity;

    [JsonProperty("identity")]
    public CipherIdentityAPIModel Identity { get; set; }
}
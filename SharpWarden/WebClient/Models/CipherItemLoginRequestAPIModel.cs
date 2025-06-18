using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.CipherItem.Models;

namespace SharpWarden.WebClient.Models;

public class CipherItemLoginRequestAPIModel : CipherItemBaseRequestAPIModel
{
    [JsonProperty("type")]
    public override CipherItemType ItemType => CipherItemType.Login;

    [JsonProperty("login")]
    public CipherLoginAPIModel Login { get; set; }
}
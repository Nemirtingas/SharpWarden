using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.CipherItem.Models;

namespace SharpWarden.WebClient.Models;

public class CipherItemCardRequestAPIModel : CipherItemBaseRequestAPIModel
{
    [JsonProperty("type")]
    public override CipherItemType ItemType => CipherItemType.Card;

    [JsonProperty("card")]
    public CipherCardAPIModel Card { get; set; }
}
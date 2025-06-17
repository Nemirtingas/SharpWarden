using Newtonsoft.Json;

using SharpWarden.BitWardenDatabase.CipherItem.Models;
using SharpWarden.BitWardenDatabase.FolderItem.Models;
using SharpWarden.BitWardenDatabase.ProfileItem.Models;
using SharpWarden.BitWardenDatabase.CollectionItem.Models;

namespace SharpWarden.BitWardenDatabase.Models;

public class DatabaseModel
{
    [JsonProperty("ciphers")]
    public List<CipherItemModel> Items { get; set; }

    [JsonProperty("folders")]
    public List<FolderItemModel> Folders { get; set; }

    [JsonProperty("collections")]
    public List<CollectionItemModel> Collections { get; set; }

    [JsonProperty("profile")]
    public ProfileItemModel Profile { get; set; }
}
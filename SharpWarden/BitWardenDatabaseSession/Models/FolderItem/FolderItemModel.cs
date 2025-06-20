using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Services;

namespace SharpWarden.BitWardenDatabaseSession.Models.FolderItem;

public class FolderItemModel : ISessionAware
{
    private IUserCryptoService _CryptoService;

    public FolderItemModel()
    {
    }

    public FolderItemModel(IUserCryptoService cryptoService)
    {
        SetCryptoService(cryptoService);
    }

    public bool HasSession() => _CryptoService != null;

    public void SetCryptoService(IUserCryptoService cryptoService)
    {
        _CryptoService = cryptoService;

        Name?.SetCryptoService(_CryptoService);
    }

    [JsonProperty("id")]
    public Guid? Id { get; set; }

    [JsonProperty("name")]
    public EncryptedString Name { get; set; }

    [JsonProperty("object")]
    public ObjectType ObjectType { get; set; } = ObjectType.Folder;

    [JsonProperty("revisionDate")]
    public DateTime? RevisionDate { get; set; }
}
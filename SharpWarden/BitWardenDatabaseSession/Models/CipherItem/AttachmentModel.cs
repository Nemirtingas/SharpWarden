using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Services;

namespace SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

public class AttachmentModel : ISessionAware
{
    private IUserCryptoService _CryptoService;

    public AttachmentModel()
    {
    }

    public AttachmentModel(IUserCryptoService cryptoService)
    {
        SetCryptoService(cryptoService);
    }

    public bool HasSession() => _CryptoService != null;

    public void SetCryptoService(IUserCryptoService cryptoService)
    {
        _CryptoService = cryptoService;

        FileName?.SetCryptoService(_CryptoService);
        Key?.SetCryptoService(_CryptoService);
    }

    [JsonProperty("fileName")]
    public EncryptedString FileName { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("key")]
    public EncryptedString Key { get; set; }

    [JsonProperty("object")]
    public ObjectType ObjectType { get; set; } = ObjectType.Attachment;

    [JsonProperty("size")]
    public int Size { get; set; }

    [JsonProperty("sizeName")]
    public string SizeName { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }
}
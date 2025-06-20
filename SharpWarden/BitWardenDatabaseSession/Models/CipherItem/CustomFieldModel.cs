using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Services;

namespace SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

public class CustomFieldModel
{
    private IUserCryptoService _CryptoService;

    public CustomFieldModel()
    {
    }

    public CustomFieldModel(IUserCryptoService cryptoService)
    {
        SetCryptoService(cryptoService);
    }

    public bool HasSession() => _CryptoService != null;

    public void SetCryptoService(IUserCryptoService cryptoService)
    {
        _CryptoService = cryptoService;

        Name?.SetCryptoService(_CryptoService);
        Value?.SetCryptoService(_CryptoService);
    }

    [JsonProperty("linkedId")]
    public CustomFieldLinkType? LinkedId { get; set; }

    [JsonProperty("name")]
    public EncryptedString Name { get; set; }

    [JsonProperty("type")]
    public CustomFieldType Type { get; set; }

    [JsonProperty("value")]
    public EncryptedString Value { get; set; }
}
using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Services;

namespace SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

public class UriModel
{
    private IUserCryptoService _CryptoService;

    public UriModel()
    {
    }

    public UriModel(IUserCryptoService cryptoService)
    {
        SetCryptoService(cryptoService);
    }

    public bool HasSession() => _CryptoService != null;

    public void SetCryptoService(IUserCryptoService cryptoService)
    {
        _CryptoService = cryptoService;

        Uri?.SetCryptoService(_CryptoService);
        UriChecksum?.SetCryptoService(_CryptoService);
    }

    [JsonProperty("match")]
    public UriMatchType? Match { get; set; }

    [JsonProperty("uri")]
    public EncryptedString Uri { get; set; }

    [JsonProperty("uriChecksum")]
    public EncryptedString UriChecksum { get; set; }
}
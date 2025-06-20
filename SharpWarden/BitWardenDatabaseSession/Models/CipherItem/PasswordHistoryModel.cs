using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Services;

namespace SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

public class PasswordHistoryModel : ISessionAware
{
    private IUserCryptoService _CryptoService;

    public PasswordHistoryModel()
    {
    }

    public PasswordHistoryModel(IUserCryptoService cryptoService)
    {
        SetCryptoService(cryptoService);
    }

    public bool HasSession() => _CryptoService != null;

    public void SetCryptoService(IUserCryptoService cryptoService)
    {
        _CryptoService = cryptoService;

        Password?.SetCryptoService(_CryptoService);
    }

    [JsonProperty("lastUsedDate")]
    public DateTime? LastUsedDate { get; set; }

    [JsonProperty("password")]
    public EncryptedString Password { get; set; }
}
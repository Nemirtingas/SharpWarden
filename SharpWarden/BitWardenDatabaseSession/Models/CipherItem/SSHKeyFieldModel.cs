using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Services;

namespace SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

public class SSHKeyFieldModel : ISessionAware
{
    private IUserCryptoService _CryptoService;

    public SSHKeyFieldModel()
    {
    }

    public SSHKeyFieldModel(IUserCryptoService cryptoService)
    {
        SetCryptoService(cryptoService);
    }

    public bool HasSession() => _CryptoService != null;

    public void SetCryptoService(IUserCryptoService cryptoService)
    {
        _CryptoService = cryptoService;

        KeyFingerprint?.SetCryptoService(_CryptoService);
        PrivateKey?.SetCryptoService(_CryptoService);
        PublicKey?.SetCryptoService(_CryptoService);
    }

    [JsonProperty("keyFingerprint")]
    public EncryptedString KeyFingerprint { get; set; }

    [JsonProperty("privateKey")]
    public EncryptedString PrivateKey { get; set; }

    [JsonProperty("publicKey")]
    public EncryptedString PublicKey { get; set; }
}
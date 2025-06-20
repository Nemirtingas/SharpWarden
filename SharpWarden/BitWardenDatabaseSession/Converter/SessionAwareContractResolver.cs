using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SharpWarden.BitWardenDatabaseSession.Services;

namespace SharpWarden.BitWardenDatabaseSession.Converter;

public class SessionAwareContractResolver : DefaultContractResolver
{
    private readonly IUserCryptoService _CryptoService;

    public SessionAwareContractResolver(IUserCryptoService cryptoService)
    {
        _CryptoService = cryptoService;
    }

    protected override JsonContract CreateContract(Type objectType)
    {
        var contract = base.CreateContract(objectType);

        if (typeof(ISessionAware).IsAssignableFrom(objectType))
        {
            var converterType = typeof(SessionAwareConverter<>).MakeGenericType(objectType);
            contract.Converter = (JsonConverter)Activator.CreateInstance(converterType, _CryptoService)!;
        }

        return contract;
    }
}
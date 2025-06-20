using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpWarden.BitWardenDatabaseSession.Services;

namespace SharpWarden.BitWardenDatabaseSession.Converter;

public class SessionAwareConverter<T> : JsonConverter where T : ISessionAware
{
    private readonly IUserCryptoService _CryptoService;

    public SessionAwareConverter(IUserCryptoService cryptoService)
    {
        _CryptoService = cryptoService;
    }

    public override bool CanConvert(Type objectType) => typeof(T).IsAssignableFrom(objectType);

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        // 
        var newSerializer = new JsonSerializer
        {
            Formatting = serializer.Formatting,
            NullValueHandling = serializer.NullValueHandling,
        };
        foreach (var converter in DefaultSessionJsonConverterService.GetAllRequiredConverters())
            newSerializer.Converters.Add(converter);

        var obj = newSerializer.Deserialize<T>(reader);
        obj?.SetCryptoService(_CryptoService);
        return obj!;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}

using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Converter;

namespace SharpWarden.BitWardenDatabaseSession.Services;

public class DefaultSessionJsonConverterService : ISessionJsonConverterService
{
    private readonly IUserCryptoService _CryptoService;
    private readonly JsonSerializer _serializer;

    public DefaultSessionJsonConverterService(
        IUserCryptoService cryptoService)
    {
        _CryptoService = cryptoService;

        var settings = new JsonSerializerSettings
        {
            ContractResolver = new SessionAwareContractResolver(_CryptoService),
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };
        foreach (var converter in GetAllRequiredConverters())
            settings.Converters.Add(converter);

        _serializer = JsonSerializer.Create(settings);
    }

    public static List<JsonConverter> GetAllRequiredConverters()
    {
        return [new EncryptedStringConverter()];
    }

    private T _Deserialize<T>(TextReader reader)
    {
        using (var jsonTextReader = new JsonTextReader(reader))
            return _serializer.Deserialize<T>(jsonTextReader);
    }

    private void _Serialize(object obj, TextWriter writer)
    {
        using (var jsonTextWriter = new JsonTextWriter(writer))
            _serializer.Serialize(jsonTextWriter, obj);
    }

    public T Deserialize<T>(string json)
        => _Deserialize<T>(new StringReader(json));

    public T Deserialize<T>(Stream json)
        => _Deserialize<T>(new StreamReader(json));

    public string Serialize(object obj)
    {
        using (var sw = new StringWriter())
        {
            _Serialize(obj, sw);
            return sw.ToString();
        }
    }
}
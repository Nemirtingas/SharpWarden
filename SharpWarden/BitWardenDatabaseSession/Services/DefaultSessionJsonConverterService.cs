// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Converter;

namespace SharpWarden.BitWardenDatabaseSession.Services;

public class DefaultSessionJsonConverterService : ISessionJsonConverterService
{
    private readonly JsonSerializer _serializer;

    public DefaultSessionJsonConverterService(
        IUserCryptoService cryptoService)
    {
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new SessionAwareContractResolver(cryptoService),
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
        using var jsonTextReader = new JsonTextReader(reader);
        return _serializer.Deserialize<T>(jsonTextReader);
    }

    private void _Serialize(object obj, TextWriter writer)
    {
        using var jsonTextWriter = new JsonTextWriter(writer);
        _serializer.Serialize(jsonTextWriter, obj);
    }

    public T Deserialize<T>(string json)
        => _Deserialize<T>(new StringReader(json));

    public T Deserialize<T>(Stream json)
        => _Deserialize<T>(new StreamReader(json));

    public string Serialize(object obj)
    {
        using var sw = new StringWriter();
        _Serialize(obj, sw);
        return sw.ToString();
    }
}
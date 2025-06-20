using Newtonsoft.Json;

namespace SharpWarden.BitWardenDatabaseSession.Converter;

public class EncryptedStringConverter : JsonConverter<EncryptedString>
{
    public override EncryptedString ReadJson(JsonReader reader, Type objectType, EncryptedString existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
        {
            return new EncryptedString
            {
                CipherString = (string)reader.Value
            };
        }

        if (reader.TokenType == JsonToken.None || reader.TokenType == JsonToken.Null || reader.TokenType == JsonToken.Undefined)
            return null;

        throw new JsonSerializationException($"Unexpected token type {reader.TokenType} when parsing EncryptedString.");
    }

    public override void WriteJson(JsonWriter writer, EncryptedString value, JsonSerializer serializer)
    {
        writer.WriteValue(value?.CipherString);
    }
}
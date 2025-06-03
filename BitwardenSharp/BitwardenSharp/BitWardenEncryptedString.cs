using Newtonsoft.Json;

namespace BitwardenSharp;

public enum BitWardenEncryptedStringType
{
    Unknown = -1,
    MasterKey = 2,
    RSACrypt = 4,
}

public class BitWardenEncryptedStringConverter : JsonConverter<BitWardenEncryptedString>
{
    public override BitWardenEncryptedString ReadJson(JsonReader reader, Type objectType, BitWardenEncryptedString existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
        {
            return new BitWardenEncryptedString
            {
                CipherString = (string)reader.Value
            };
        }

        if (reader.TokenType == JsonToken.None || reader.TokenType == JsonToken.Null || reader.TokenType == JsonToken.Undefined)
            return null;

        throw new JsonSerializationException($"Unexpected token type {reader.TokenType} when parsing BitWardenEncryptedString.");
    }

    public override void WriteJson(JsonWriter writer, BitWardenEncryptedString value, JsonSerializer serializer)
    {
        writer.WriteValue(value?.CipherString);
    }
}

public class BitWardenEncryptedString
{
    public string CipherString { get; set; }
}
// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

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
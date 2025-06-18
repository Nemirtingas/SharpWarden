using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession;
using SharpWarden.BitWardenDatabaseSession.Models;
using static SharpWarden.BitWardenCipherService;

namespace SharpWarden;

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

public class EncryptedString : IDatabaseSessionModel
{
    [JsonIgnore]
    private DatabaseSession _DatabaseSession { get; set; }

    [JsonIgnore]
    private Guid? _OrganizationId { get; set; }

    public string CipherString { get; set; }

    public EncryptedString()
    {
    }

    public EncryptedString(DatabaseSession databaseSession)
    {
        _DatabaseSession = databaseSession;
    }

    public bool HasSession() => _DatabaseSession != null;

    public void SetDatabaseSession(DatabaseSession databaseSession)
    {
        _DatabaseSession = databaseSession;
    }

    public void SetDatabaseSession(DatabaseSession databaseSession, Guid? organizationId)
    {
        _DatabaseSession = databaseSession;
        _OrganizationId = organizationId;
    }

    public EncryptedString Clone()
    {
        return new EncryptedString
        {
            _DatabaseSession = _DatabaseSession,
            _OrganizationId = _OrganizationId,
            CipherString = CipherString,
        };
    }

    [JsonIgnore]
    public string ClearString
    {
        get
        {
            if (CipherString == null)
                return null;

            if (_DatabaseSession == null)
                    throw new InvalidOperationException("The database session is not loaded.");

            if (CipherType == BitWardenCipherType.AesCbc256_HmacSha256_B64)
                return _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, CipherString);

            if (CipherType == BitWardenCipherType.Rsa2048_OaepSha1_B64)
                return _DatabaseSession.GetClearStringWithMasterKeyWithRSAKey(_OrganizationId, CipherString);

            throw new NotImplementedException();
        }

        set
        {
            if (_DatabaseSession == null)
                throw new InvalidOperationException("The database session is not loaded.");

            switch (CipherType)
            {
                case BitWardenCipherType.Unknown: // Common case of an unknown type would be to crypt with the master key.
                case BitWardenCipherType.AesCbc256_HmacSha256_B64: CipherString = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value); return;
                case BitWardenCipherType.Rsa2048_OaepSha1_B64: CipherString = _DatabaseSession.CryptClearStringWithRSAKey(_OrganizationId, value); return;
            }

            throw new NotImplementedException();
        }
    }

    [JsonIgnore]
    public byte[] ClearBytes
    {
        get
        {
            if (CipherString == null)
                return null;

            if (_DatabaseSession == null)
                throw new InvalidOperationException("The database session is not loaded.");

            if (CipherType == BitWardenCipherType.AesCbc256_HmacSha256_B64)
                return _DatabaseSession.GetClearBytesWithMasterKey(_OrganizationId, CipherString);

            if (CipherType == BitWardenCipherType.Rsa2048_OaepSha1_B64)
                return _DatabaseSession.GetClearBytesWithRSAKey(_OrganizationId, CipherString);

            throw new NotImplementedException();
        }

        set
        {
            if (_DatabaseSession == null)
                throw new InvalidOperationException("The database session is not loaded.");

            switch (CipherType)
            {
                case BitWardenCipherType.Unknown: // Common case of an unknown type would be to crypt with the master key.
                case BitWardenCipherType.AesCbc256_HmacSha256_B64: CipherString = _DatabaseSession.CryptClearBytesWithMasterKey(_OrganizationId, value); return;
                case BitWardenCipherType.Rsa2048_OaepSha1_B64: CipherString = _DatabaseSession.CryptClearBytesWithRSAKey(_OrganizationId, value); return;
            }

            throw new NotImplementedException();
        }
    }

    [JsonIgnore]
    public BitWardenCipherType CipherType
    {
        get
        {
            if (CipherString == null)
                return BitWardenCipherType.Unknown;

            var parts = CipherString.Split(".", 2);
            if (parts.Length <= 0 || !Enum.TryParse<BitWardenCipherType>(parts[0], out var type))
                return BitWardenCipherType.Unknown;

            return type;
        }

        set
        {
            if (CipherType == value)
                return;

            if (CipherType == BitWardenCipherType.Unknown)
            {
                switch (value)
                {
                    case BitWardenCipherType.AesCbc256_HmacSha256_B64: CipherString = _DatabaseSession.CryptClearStringWithMasterKey(null, ""); break;
                    case BitWardenCipherType.Rsa2048_OaepSha1_B64: CipherString = _DatabaseSession.CryptClearStringWithRSAKey(null, ""); break;
                }
                return;
            }
        }
    }

    [JsonIgnore]
    public byte[] IV
    {
        get
        {
            if (CipherType != BitWardenCipherType.AesCbc256_HmacSha256_B64)
                return null;

            var parts = CipherString.Split('.', 2);
            var innerParts = parts[1].Split('|');

            return Convert.FromBase64String(innerParts[0]);
        }
    }

    [JsonIgnore]
    public byte[] Mac
    {
        get
        {
            if (CipherType != BitWardenCipherType.AesCbc256_HmacSha256_B64)
                return null;

            var parts = CipherString.Split('.', 2);
            var innerParts = parts[1].Split('|');

            var mac = default(byte[]);
            try
            {
                mac = Convert.FromBase64String(innerParts[2]);
            }
            catch
            {
                var pp = innerParts[2].Split('/');
                pp[0] = pp[0] + "/";
                mac = Convert.FromBase64String(string.Join('/', pp));
            }

            return mac;
        }
    }

    [JsonIgnore]
    public byte[] Data
    {
        get
        {
            if (CipherType == BitWardenCipherType.AesCbc256_HmacSha256_B64)
            {
                var parts = CipherString.Split('.', 2);
                var innerParts = parts[1].Split('|');
                return Convert.FromBase64String(innerParts[1]);
            }

            if (CipherType == BitWardenCipherType.Rsa2048_OaepSha1_B64)
            {
                var parts = CipherString.Split('.', 2);
                string[] innerParts = parts[1].Split('|');
                return Convert.FromBase64String(innerParts[0]);
            }

            return null;
        }
    }
}
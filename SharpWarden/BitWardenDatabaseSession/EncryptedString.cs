using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession;
using SharpWarden.BitWardenDatabaseSession.Models;

namespace SharpWarden;

public enum EncryptedStringType
{
    Unknown = -1,
    MasterKey = 2,
    RSACrypt = 4,
}

public class EncryptedStringConverter : JsonConverter<EncryptedString?>
{
    public override EncryptedString? ReadJson(JsonReader reader, Type objectType, EncryptedString? existingValue, bool hasExistingValue, JsonSerializer serializer)
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

    public override void WriteJson(JsonWriter writer, EncryptedString? value, JsonSerializer serializer)
    {
        writer.WriteValue(value?.CipherString);
    }
}

public struct EncryptedString
{
    [JsonIgnore]
    public DatabaseSession DatabaseSession { get; set; }

    [JsonIgnore]
    public Guid? OrganizationId { get; set; }

    public string CipherString { get; set; }

    public EncryptedString()
    {
    }

    public EncryptedString(string cipherString)
    {
        CipherString = cipherString;
    }

    public EncryptedString(DatabaseSession databaseSession)
    {
        DatabaseSession = databaseSession;
    }

    public EncryptedString(string cipherString, DatabaseSession databaseSession)
    {
        DatabaseSession = databaseSession;
        CipherString = cipherString;
    }

    public EncryptedString(DatabaseSession databaseSession, Guid? organizationId)
    {
        DatabaseSession = databaseSession;
        OrganizationId = organizationId;
    }

    public EncryptedString(string cipherString, DatabaseSession databaseSession, Guid? organizationId)
    {
        DatabaseSession = databaseSession;
        OrganizationId = organizationId;
        CipherString = cipherString;
    }

    [JsonIgnore]
    public bool HasSession => DatabaseSession != null;

    [JsonIgnore]
    public string ClearString
    {
        get
        {
            if (CipherString == null)
                return null;

            if (DatabaseSession == null)
                    throw new InvalidOperationException("The database session is not loaded.");

            if (CipherType == EncryptedStringType.MasterKey)
                return DatabaseSession.GetClearStringWithMasterKey(OrganizationId, CipherString);

            if (CipherType == EncryptedStringType.RSACrypt)
                return DatabaseSession.GetClearStringWithMasterKeyWithRSAKey(OrganizationId, CipherString);

            throw new NotImplementedException();
        }

        set
        {
            if (DatabaseSession == null)
                throw new InvalidOperationException("The database session is not loaded.");

            switch (CipherType)
            {
                case EncryptedStringType.Unknown: // Common case of an unknown type would be to crypt with the master key.
                case EncryptedStringType.MasterKey: CipherString = DatabaseSession.CryptClearStringWithMasterKey(OrganizationId, value); return;
                case EncryptedStringType.RSACrypt: CipherString = DatabaseSession.CryptClearStringWithRSAKey(OrganizationId, value); return;
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

            if (DatabaseSession == null)
                throw new InvalidOperationException("The database session is not loaded.");

            if (CipherType == EncryptedStringType.MasterKey)
                return DatabaseSession.GetClearBytesWithMasterKey(OrganizationId, CipherString);

            if (CipherType == EncryptedStringType.RSACrypt)
                return DatabaseSession.GetClearBytesWithRSAKey(OrganizationId, CipherString);

            throw new NotImplementedException();
        }

        set
        {
            if (DatabaseSession == null)
                throw new InvalidOperationException("The database session is not loaded.");

            switch (CipherType)
            {
                case EncryptedStringType.Unknown: // Common case of an unknown type would be to crypt with the master key.
                case EncryptedStringType.MasterKey: CipherString = DatabaseSession.CryptClearBytesWithMasterKey(OrganizationId, value); return;
                case EncryptedStringType.RSACrypt: CipherString = DatabaseSession.CryptClearBytesWithRSAKey(OrganizationId, value); return;
            }

            throw new NotImplementedException();
        }
    }

    [JsonIgnore]
    public EncryptedStringType CipherType
    {
        get
        {
            if (CipherString == null)
                return EncryptedStringType.Unknown;

            var parts = CipherString.Split(".", 2);
            if (parts.Length <= 0 || !Enum.TryParse<EncryptedStringType>(parts[0], out var type))
                return EncryptedStringType.Unknown;

            return type;
        }

        set
        {
            if (CipherType == value)
                return;

            if (CipherType == EncryptedStringType.Unknown)
            {
                switch (value)
                {
                    case EncryptedStringType.MasterKey: CipherString = DatabaseSession.CryptClearStringWithMasterKey(null, ""); break;
                    case EncryptedStringType.RSACrypt: CipherString = DatabaseSession.CryptClearStringWithRSAKey(null, ""); break;
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
            if (CipherType != EncryptedStringType.MasterKey)
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
            if (CipherType != EncryptedStringType.MasterKey)
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
            if (CipherType == EncryptedStringType.MasterKey)
            {
                var parts = CipherString.Split('.', 2);
                var innerParts = parts[1].Split('|');
                return Convert.FromBase64String(innerParts[1]);
            }

            if (CipherType == EncryptedStringType.RSACrypt)
            {
                var parts = CipherString.Split('.', 2);
                string[] innerParts = parts[1].Split('|');
                return Convert.FromBase64String(innerParts[0]);
            }

            return null;
        }
    }
}
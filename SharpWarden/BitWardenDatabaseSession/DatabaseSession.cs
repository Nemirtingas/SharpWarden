using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.CollectionItem.Models;
using SharpWarden.BitWardenDatabaseSession.FolderItem.Models;
using SharpWarden.BitWardenDatabaseSession.Models;

namespace SharpWarden.BitWardenDatabaseSession;

public class DatabaseSession
{
    private class UserKeys
    {
        internal byte[] SymmetricKey = new byte[32];
        internal byte[] SymmetricMac = new byte[32];
        internal byte[] RSAKey;
    }

    private UserKeys _UserKeys;

    private Dictionary<Guid, UserKeys> _Keys = new();

    public DatabaseModel Database { get; private set; }

    private UserKeys _GetUserKeys(Guid? ownerId)
    {
        if (ownerId == null)
        {
            if (_UserKeys == null)
                throw new Exception($"Database not unlocked, please call {nameof(LoadBitWardenDatabaseAsync)} or {nameof(LoadBitWardenKeysAsync)} before trying to decrypt strings.");

            return _UserKeys;
        }

        if (!_Keys.TryGetValue(ownerId.Value, out var keys) && Database?.Profile?.Organizations != null)
        {
            foreach (var organization in Database.Profile.Organizations)
            {
                if (organization.Id == ownerId.Value)
                {
                    var organizationKey = BitWardenCipherService.DecryptWithRsa2048OaepSha1Base64(organization.Key, _UserKeys.RSAKey);
                    keys = new UserKeys
                    {
                        SymmetricKey = organizationKey.Take(32).ToArray(),
                        SymmetricMac = organizationKey.Skip(32).Take(32).ToArray(),
                    };
                    _Keys[ownerId.Value] = keys;
                    break;
                }
            }
        }

        if (keys == null)
            throw new Exception($"Key not found for organization {ownerId}.");

        return keys;
    }

    public Task<bool> LoadBitWardenKeysAsync(byte[] userEmail, byte[] userPassword, int kdfIterations, string key, string privateKey)
    {
        _Keys = new();

        var userMasterKey = BitWardenCipherService.ComputeMasterKey(userEmail, userPassword, kdfIterations);
        var userEncryptionKey = BitWardenCipherService.HKDF_SHA256(userMasterKey, Encoding.UTF8.GetBytes("enc"), 32);
        var userMacKey = BitWardenCipherService.HKDF_SHA256(userMasterKey, Encoding.UTF8.GetBytes("mac"), 32);

        for (int i = 0; i < userMasterKey.Length; ++i)
            userMasterKey[i] = 0;

        // Decrypt the protected symmetric key
        var symKey = BitWardenCipherService.DecryptWithAesCbc256HmacSha256Base64(key, userEncryptionKey, userMacKey);
        if (symKey.Length != 64)
            throw new CryptographicException("Invalid decrypted symmetric key length.");

        _UserKeys = new UserKeys();

        Array.Copy(symKey, 0, _UserKeys.SymmetricKey, 0, 32);
        Array.Copy(symKey, 32, _UserKeys.SymmetricMac, 0, 32);

        // Decrypt RSA private key
        _UserKeys.RSAKey = GetClearBytesWithMasterKey(null, privateKey);

        return Task.FromResult(true);
    }

    public async Task<bool> LoadBitWardenDatabaseAsync(byte[] userPassword, int kdfIterations, Stream stream)
    {
        using (var sr = new StreamReader(stream))
        using (var reader = new JsonTextReader(sr))
        {
            var serializer = JsonSerializer.Create();
            serializer.Converters.Add(new EncryptedStringConverter());
            return await LoadBitWardenDatabaseAsync(userPassword, kdfIterations, serializer.Deserialize<DatabaseModel>(reader));
        }
    }

    public async Task<bool> LoadBitWardenDatabaseAsync(byte[] userPassword, int kdfIterations, DatabaseModel database)
    {
        _Keys = new();

        Database = database;
        Database.SetDatabaseSession(this);

        return await LoadBitWardenKeysAsync(Encoding.UTF8.GetBytes(Database.Profile.Email), userPassword, kdfIterations, Database.Profile.Key.CipherString, Database.Profile.PrivateKey.CipherString);
    }

    public string GetClearStringWithMasterKeyWithRSAKey(Guid? ownerId, string cipherString)
    {
        if (cipherString == null)
            return null;

        var keys = _GetUserKeys(ownerId);
        return BitWardenCipherService.DecryptStringWithRsa2048OaepSha1Base64(cipherString, keys.RSAKey);
    }

    public string GetClearStringWithMasterKey(Guid? ownerId, string cipherString)
    {
        if (cipherString == null)
            return null;

        var keys = _GetUserKeys(ownerId);
        return BitWardenCipherService.DecryptStringWithAesCbc256HmacSha256Base64(cipherString, keys.SymmetricKey, keys.SymmetricMac);
    }

    public byte[] GetClearBytesWithRSAKey(Guid? ownerId, string cipherString)
    {
        if (cipherString == null)
            return null;

        var keys = _GetUserKeys(ownerId);
        return BitWardenCipherService.DecryptWithRsa2048OaepSha1Base64(cipherString, keys.RSAKey);
    }

    public byte[] GetClearBytesWithMasterKey(Guid? ownerId, string cipherString)
    {
        if (cipherString == null)
            return null;

        var keys = _GetUserKeys(ownerId);
        return BitWardenCipherService.DecryptWithAesCbc256HmacSha256Base64(cipherString, keys.SymmetricKey, keys.SymmetricMac);
    }

    public string CryptClearStringWithRSAKey(Guid? ownerId, string clearString)
    {
        if (clearString == null)
            return null;

        var keys = _GetUserKeys(ownerId);
        return BitWardenCipherService.EncryptWithRsa2048OaepSha1Base64(Encoding.UTF8.GetBytes(clearString), keys.RSAKey);
    }

    public string CryptClearStringWithMasterKey(Guid? ownerId, string clearString)
    {
        if (clearString == null)
            return null;

        var keys = _GetUserKeys(ownerId);
        return BitWardenCipherService.EncryptWithAesCbc256HmacSha256Base64(Encoding.UTF8.GetBytes(clearString), keys.SymmetricKey, keys.SymmetricMac);
    }

    public string CryptClearBytesWithRSAKey(Guid? ownerId, byte[] bytes)
    {
        if (bytes == null)
            return null;

        var keys = _GetUserKeys(ownerId);
        return BitWardenCipherService.EncryptWithRsa2048OaepSha1Base64(bytes, keys.RSAKey);
    }

    public string CryptClearBytesWithMasterKey(Guid? ownerId, byte[] bytes)
    {
        if (bytes == null)
            return null;

        var keys = _GetUserKeys(ownerId);
        return BitWardenCipherService.EncryptWithAesCbc256HmacSha256Base64(bytes, keys.SymmetricKey, keys.SymmetricMac);
    }

    public FolderItemModel GetItemFolder(Guid? id)
    {
        if (id == null)
            return null;

        return Database.Folders.Find(e => e.Id == id);
    }

    public List<CollectionItemModel> GetItemCollections(IEnumerable<Guid> ids)
    {
        return ids == null
            ? new List<CollectionItemModel>()
            : ids.Select(id => Database.Collections.Find(e => e.Id == id))
                .Where(c => c != null)
                .ToList();
    }

    public void CreateNewDatabase()
    {
        Database = new DatabaseModel(this);
    }
}
using System.Collections;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using BitwardenSharp.Models;
using Newtonsoft.Json;

namespace BitwardenSharp;

public class BitWardenDatabaseSession
{
    private class UserKeys
    {
        internal byte[] SymmetricKey = new byte[32];
        internal byte[] SymmetricMac = new byte[32];
        internal byte[] RSAKey;
    }

    private BitWardenDatabaseModel _Database;

    private UserKeys _UserKeys;

    private Dictionary<Guid, UserKeys> _Keys;

    private UserKeys _GetUserKeys(Guid? ownerId)
    {
        if (ownerId == null)
            return _UserKeys;

        if (!_Keys.TryGetValue(ownerId.Value, out var keys))
        {
            foreach (var organization in _Database.Profile.Organizations)
            {
                if (organization.Id == ownerId.Value)
                {
                    var organizationKey = BitwardenCipherService.DecryptWithRSA(organization.Key.CipherString, _UserKeys.RSAKey);
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

    public async Task<bool> LoadBitwardenDatabaseAsync(byte[] userEmail, byte[] userPassword, int kdfIterations, Stream databaseStream)
    {
        _Keys = new();
        var serializer = new JsonSerializer();
        serializer.Converters.Add(new BitWardenEncryptedStringConverter());

        using (var sr = new StreamReader(databaseStream))
        using (var jsonTextReader = new JsonTextReader(sr))
        {
            _Database = serializer.Deserialize<BitWardenDatabaseModel>(jsonTextReader);
        }

        if (userEmail == null)
            userEmail = Encoding.UTF8.GetBytes(_Database.Profile.Email);

        var userMasterKey = BitwardenCipherService.ComputeMasterKey(userEmail, userPassword, kdfIterations);
        var userEncryptionKey = BitwardenCipherService.HKDF_SHA256(userMasterKey, Encoding.UTF8.GetBytes("enc"), 32);
        var userMacKey = BitwardenCipherService.HKDF_SHA256(userMasterKey, Encoding.UTF8.GetBytes("mac"), 32);

        for (int i = 0; i < userMasterKey.Length; ++i)
            userMasterKey[i] = 0;

        // Decrypt the protected symmetric key
        var symKey = BitwardenCipherService.DecryptWithMasterKey(_Database.Profile.Key, userEncryptionKey, userMacKey);
        if (symKey.Length != 64)
            throw new CryptographicException("Invalid decrypted symmetric key length.");

        _UserKeys = new UserKeys();

        Array.Copy(symKey, 0, _UserKeys.SymmetricKey, 0, 32);
        Array.Copy(symKey, 32, _UserKeys.SymmetricMac, 0, 32);

        // Decrypt RSA private key
        _UserKeys.RSAKey = BitwardenCipherService.DecryptWithMasterKey(_Database.Profile.PrivateKey, _UserKeys.SymmetricKey, _UserKeys.SymmetricMac);

        return true;
    }

    public IEnumerable<BitWardenItemModel> Items => _Database.Items;

    public string GetClearString(Guid? ownerId, string cipherString)
    {
        var keys = _GetUserKeys(ownerId);

        return BitwardenCipherService.DecryptStringWithMasterKey(cipherString, keys.SymmetricKey, keys.SymmetricMac);
    }

    public string GetClearString(Guid? ownerId, BitWardenEncryptedString cipherString) => GetClearString(ownerId, cipherString.CipherString);
}
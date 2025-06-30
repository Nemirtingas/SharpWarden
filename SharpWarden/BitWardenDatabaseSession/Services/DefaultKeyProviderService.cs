// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using System.Security.Cryptography;
using System.Text;

namespace SharpWarden.BitWardenDatabaseSession.Services;

public class UserKeys
{
    public byte[] SymmetricKey = new byte[32];
    public byte[] SymmetricMac = new byte[32];
    public byte[] RSAKey;
}

public class DefaultKeyProviderService : IKeyProviderService
{
    private UserKeys _UserKeys;

    private Dictionary<Guid, UserKeys> _Keys = new();

    public UserKeys GetUserKeys(Guid? ownerId)
    {
        if (ownerId == null)
        {
            if (_UserKeys == null)
                throw new Exception("Key not found for user.");

            return _UserKeys;
        }

        if (!_Keys.TryGetValue(ownerId.Value, out var keys) || keys == null)
            throw new Exception($"Key not found for organization {ownerId}.");

        return keys;
    }

    public void LoadBitWardenUserKey(byte[] userEmail, byte[] userPassword, int kdfIterations, string key, string privateKey)
    {
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
        _UserKeys.RSAKey = BitWardenCipherService.DecryptWithAesCbc256HmacSha256Base64(privateKey, _UserKeys.SymmetricKey, _UserKeys.SymmetricMac);
    }

    public void LoadBitWardenOrganizationKey(Guid organizationId, string organizationCipheredKey)
    {
        if (_Keys.ContainsKey(organizationId))
            return;

        var organizationKey = BitWardenCipherService.DecryptWithRsa2048OaepSha1Base64(organizationCipheredKey, _UserKeys.RSAKey);
        _Keys[organizationId] = new UserKeys
        {
            SymmetricKey = organizationKey.Take(32).ToArray(),
            SymmetricMac = organizationKey.Skip(32).Take(32).ToArray(),
        };
    }
}
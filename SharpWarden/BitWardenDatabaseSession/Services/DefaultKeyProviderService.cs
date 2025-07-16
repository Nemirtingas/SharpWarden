// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using System.Security.Cryptography;

namespace SharpWarden.BitWardenDatabaseSession.Services;

public class UserKeys
{
    public byte[] SymmetricKey = new byte[32];
    public byte[] SymmetricMac = new byte[32];
    public byte[] RsaKey;
}

public class DefaultKeyProviderService : IKeyProviderService
{
    private UserKeys _userKeys;

    private readonly Dictionary<Guid, UserKeys> _keys = new();

    public UserKeys GetUserKeys(Guid? ownerId)
    {
        if (ownerId == null)
        {
            if (_userKeys == null)
                throw new Exception("Key not found for user.");

            return _userKeys;
        }

        if (!_keys.TryGetValue(ownerId.Value, out var keys) || keys == null)
            throw new Exception($"Key not found for organization {ownerId}.");

        return keys;
    }

    public void LoadBitWardenUserKey(byte[] userEmail, byte[] userPassword, int kdfIterations, string key, string privateKey)
    {
        var userMasterKey = BitWardenCipherService.ComputeMasterKey(userEmail, userPassword, kdfIterations);
        var userEncryptionKey = BitWardenCipherService.HKDFSHA256(userMasterKey, "enc"u8.ToArray(), 32);
        var userMacKey = BitWardenCipherService.HKDFSHA256(userMasterKey, "mac"u8.ToArray(), 32);

        for (int i = 0; i < userMasterKey.Length; ++i)
            userMasterKey[i] = 0;

        // Decrypt the protected symmetric key
        var symKey = BitWardenCipherService.DecryptWithAesCbc256HmacSha256Base64(key, userEncryptionKey, userMacKey);
        if (symKey.Length != 64)
            throw new CryptographicException("Invalid decrypted symmetric key length.");

        _userKeys = new UserKeys();

        Array.Copy(symKey, 0, _userKeys.SymmetricKey, 0, 32);
        Array.Copy(symKey, 32, _userKeys.SymmetricMac, 0, 32);

        // Decrypt RSA private key
        _userKeys.RsaKey = BitWardenCipherService.DecryptWithAesCbc256HmacSha256Base64(privateKey, _userKeys.SymmetricKey, _userKeys.SymmetricMac);
    }

    public void LoadBitWardenOrganizationKey(Guid organizationId, string organizationCipheredKey)
    {
        if (_keys.ContainsKey(organizationId))
            return;

        var organizationKey = BitWardenCipherService.DecryptWithRsa2048OaepSha1Base64(organizationCipheredKey, _userKeys.RsaKey);
        _keys[organizationId] = new UserKeys
        {
            SymmetricKey = organizationKey.Take(32).ToArray(),
            SymmetricMac = organizationKey.Skip(32).Take(32).ToArray(),
        };
    }
}
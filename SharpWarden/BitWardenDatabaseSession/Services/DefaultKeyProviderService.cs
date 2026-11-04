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
                throw new KeyNotFoundException("The user key is not loaded.");

            return _userKeys;
        }

        if (!_keys.TryGetValue(ownerId.Value, out var keys) || keys == null)
            throw new KeyNotFoundException($"Key not found for organization {ownerId}.");

        return keys;
    }

    public void LoadBitWardenUserKey(byte[] userEmail, byte[] userPassword, int kdfIterations, string key, string privateKey)
    {
        var userMasterKey = BitWardenCipherService.ComputeMasterKey(userEmail, userPassword, kdfIterations);
        byte[] userEncryptionKey = null;
        byte[] userMacKey = null;
        byte[] symKey = null;

        try
        {
            userEncryptionKey = BitWardenCipherService.HKDFSHA256(userMasterKey, "enc"u8.ToArray(), 32);
            userMacKey = BitWardenCipherService.HKDFSHA256(userMasterKey, "mac"u8.ToArray(), 32);

            // Decrypt the protected symmetric key
            symKey = BitWardenCipherService.DecryptWithAesCbc256HmacSha256Base64(key, userEncryptionKey, userMacKey);
            if (symKey.Length != 64)
                throw new CryptographicException("Invalid decrypted symmetric key length.");

            var userKeys = new UserKeys();

            Array.Copy(symKey, 0, userKeys.SymmetricKey, 0, 32);
            Array.Copy(symKey, 32, userKeys.SymmetricMac, 0, 32);

            // Decrypt RSA private key
            userKeys.RsaKey = BitWardenCipherService.DecryptWithAesCbc256HmacSha256Base64(privateKey, userKeys.SymmetricKey, userKeys.SymmetricMac);

            _userKeys = userKeys;
        }
        finally
        {
            // Do not leave intermediate key material lying around in memory.
            CryptographicOperations.ZeroMemory(userMasterKey);
            if (userEncryptionKey != null) CryptographicOperations.ZeroMemory(userEncryptionKey);
            if (userMacKey != null) CryptographicOperations.ZeroMemory(userMacKey);
            if (symKey != null) CryptographicOperations.ZeroMemory(symKey);
        }
    }

    public void LoadBitWardenOrganizationKey(Guid organizationId, string organizationCipheredKey)
    {
        if (_keys.ContainsKey(organizationId))
            return;

        if (_userKeys?.RsaKey == null)
            throw new InvalidOperationException("The user key must be loaded before organization keys.");

        var organizationKey = BitWardenCipherService.DecryptWithRsa2048OaepSha1Base64(organizationCipheredKey, _userKeys.RsaKey);
        try
        {
            if (organizationKey.Length != 64)
                throw new CryptographicException($"Invalid decrypted organization key length for organization {organizationId}.");

            _keys[organizationId] = new UserKeys
            {
                SymmetricKey = organizationKey[..32],
                SymmetricMac = organizationKey[32..],
            };
        }
        finally
        {
            CryptographicOperations.ZeroMemory(organizationKey);
        }
    }
}
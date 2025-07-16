// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using System.Text;
using static SharpWarden.BitWardenCipherService;

namespace SharpWarden.BitWardenDatabaseSession.Services;

public class DefaultCryptoService : ICryptoService
{
    private readonly IKeyProviderService _keyProviderService;

    public DefaultCryptoService(IKeyProviderService keyProviderService)
    {
        _keyProviderService = keyProviderService;
    }

    public IKeyProviderService KeyProviderService => _keyProviderService;

    public string GetClearStringWithRsaKey(Guid? organizationId, string cipherString)
    {
        if (cipherString == null)
            return null;

        var keys = _keyProviderService.GetUserKeys(organizationId);
        return BitWardenCipherService.DecryptStringWithRsa2048OaepSha1Base64(cipherString, keys.RsaKey);
    }

    public string GetClearStringWithMasterKey(Guid? organizationId, string cipherString)
    {
        if (cipherString == null)
            return null;

        var keys = _keyProviderService.GetUserKeys(organizationId);
        return BitWardenCipherService.DecryptStringWithAesCbc256HmacSha256Base64(cipherString, keys.SymmetricKey, keys.SymmetricMac);
    }

    public string GetClearStringAuto(Guid? organizationId, string cipherString)
    {
        if (cipherString == null)
            return null;

        var parts = cipherString.Split('.', 2);

        if (parts.Length != 2 || !Enum.TryParse<BitWardenCipherType>(parts[0], out var cipherType))
            throw new InvalidDataException("Unexpected cipher type.");

        switch (cipherType)
        {
            case BitWardenCipherType.Rsa2048OaepSha1B64: return GetClearStringWithRsaKey(organizationId, cipherString);
            case BitWardenCipherType.AesCbc256HmacSha256B64: return GetClearStringWithMasterKey(organizationId, cipherString);
        }

        throw new NotImplementedException($"Unhandled cipher type {cipherType}");
    }

    public byte[] GetClearBytesWithRsaKey(Guid? organizationId, string cipherString)
    {
        if (cipherString == null)
            return null;

        var keys = _keyProviderService.GetUserKeys(organizationId);
        return BitWardenCipherService.DecryptWithRsa2048OaepSha1Base64(cipherString, keys.RsaKey);
    }

    public byte[] GetClearBytesWithMasterKey(Guid? organizationId, string cipherString)
    {
        if (cipherString == null)
            return null;

        var keys = _keyProviderService.GetUserKeys(organizationId);
        return BitWardenCipherService.DecryptWithAesCbc256HmacSha256Base64(cipherString, keys.SymmetricKey, keys.SymmetricMac);
    }

    public byte[] GetClearBytesAuto(Guid? organizationId, string cipherString)
    {
        if (cipherString == null)
            return null;

        var parts = cipherString.Split('.', 2);

        if (parts.Length != 2 || !Enum.TryParse<BitWardenCipherType>(parts[0], out var cipherType))
            throw new InvalidDataException("Unexpected cipher type.");

        switch (cipherType)
        {
            case BitWardenCipherType.Rsa2048OaepSha1B64: return GetClearBytesWithRsaKey(organizationId, cipherString);
            case BitWardenCipherType.AesCbc256HmacSha256B64: return GetClearBytesWithMasterKey(organizationId, cipherString);
        }

        throw new NotImplementedException($"Unhandled cipher type {cipherType}");
    }

    public string CryptClearStringWithRsaKey(Guid? organizationId, string clearString)
    {
        if (clearString == null)
            return null;

        var keys = _keyProviderService.GetUserKeys(organizationId);
        return BitWardenCipherService.EncryptWithRsa2048OaepSha1Base64(Encoding.UTF8.GetBytes(clearString), keys.RsaKey);
    }

    public string CryptClearStringWithMasterKey(Guid? organizationId, string clearString)
    {
        if (clearString == null)
            return null;

        var keys = _keyProviderService.GetUserKeys(organizationId);
        return BitWardenCipherService.EncryptWithAesCbc256HmacSha256Base64(Encoding.UTF8.GetBytes(clearString), keys.SymmetricKey, keys.SymmetricMac);
    }

    public string CryptClearString(Guid? organizationId, string clearString, BitWardenCipherType cipherType)
    {
        switch (cipherType)
        {
            case BitWardenCipherType.Rsa2048OaepSha1B64: return CryptClearStringWithRsaKey(organizationId, clearString);
            case BitWardenCipherType.AesCbc256HmacSha256B64: return CryptClearStringWithMasterKey(organizationId, clearString);
        }

        throw new NotImplementedException($"Unhandled cipher type {cipherType}");
    }

    public string CryptClearBytesWithRsaKey(Guid? organizationId, byte[] bytes)
    {
        if (bytes == null)
            return null;

        var keys = _keyProviderService.GetUserKeys(organizationId);
        return BitWardenCipherService.EncryptWithRsa2048OaepSha1Base64(bytes, keys.RsaKey);
    }

    public string CryptClearBytesWithMasterKey(Guid? organizationId, byte[] bytes)
    {
        if (bytes == null)
            return null;

        var keys = _keyProviderService.GetUserKeys(organizationId);
        return BitWardenCipherService.EncryptWithAesCbc256HmacSha256Base64(bytes, keys.SymmetricKey, keys.SymmetricMac);
    }

    public string CryptClearBytes(Guid? organizationId, byte[] bytes, BitWardenCipherType cipherType)
    {
        switch (cipherType)
        {
            case BitWardenCipherType.Rsa2048OaepSha1B64: return CryptClearBytesWithRsaKey(organizationId, bytes);
            case BitWardenCipherType.AesCbc256HmacSha256B64: return CryptClearBytesWithMasterKey(organizationId, bytes);
        }

        throw new NotImplementedException($"Unhandled cipher type {cipherType}");
    }
}
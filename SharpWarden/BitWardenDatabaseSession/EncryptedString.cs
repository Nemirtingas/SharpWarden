// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession;
using SharpWarden.BitWardenDatabaseSession.Services;
using static SharpWarden.BitWardenCipherService;

namespace SharpWarden;

public class EncryptedString : ISessionAware
{
    [JsonIgnore]
    private IUserCryptoService _CryptoService { get; set; }

    public string CipherString { get; set; }

    public EncryptedString()
    {
    }

    public EncryptedString(IUserCryptoService cryptoService)
    {
        _CryptoService = cryptoService;
    }

    public bool HasSession() => _CryptoService != null;

    public void SetCryptoService(IUserCryptoService cryptoService)
    {
        _CryptoService = cryptoService;
    }

    public EncryptedString Clone()
    {
        return new EncryptedString
        {
            _CryptoService = _CryptoService,
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

            if (_CryptoService == null)
                    throw new InvalidOperationException("The database session is not loaded.");

            return _CryptoService.GetClearStringAuto(CipherString);
        }

        set
        {
            if (_CryptoService == null)
                throw new InvalidOperationException("The database session is not loaded.");

            switch (CipherType)
            {
                case BitWardenCipherType.Unknown: // Common case of an unknown type would be to crypt with the master key.
                case BitWardenCipherType.AesCbc256_HmacSha256_B64: CipherString = _CryptoService.CryptClearStringWithMasterKey(value); return;
                case BitWardenCipherType.Rsa2048_OaepSha1_B64: CipherString = _CryptoService.CryptClearStringWithRSAKey(value); return;
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

            if (_CryptoService == null)
                throw new InvalidOperationException("The database session is not loaded.");

            switch(CipherType)
            {
                case BitWardenCipherType.AesCbc256_HmacSha256_B64: return _CryptoService.GetClearBytesWithMasterKey(CipherString);
                case BitWardenCipherType.Rsa2048_OaepSha1_B64: return _CryptoService.GetClearBytesWithRSAKey(CipherString);
            }

            throw new NotImplementedException();
        }

        set
        {
            if (_CryptoService == null)
                throw new InvalidOperationException("The database session is not loaded.");

            switch (CipherType)
            {
                case BitWardenCipherType.Unknown: // Common case of an unknown type would be to crypt with the master key.
                case BitWardenCipherType.AesCbc256_HmacSha256_B64: CipherString = _CryptoService.CryptClearBytesWithMasterKey(value); return;
                case BitWardenCipherType.Rsa2048_OaepSha1_B64: CipherString = _CryptoService.CryptClearBytesWithRSAKey(value); return;
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
            var cipherType = CipherType;

            if (cipherType == value)
                return;

            if (cipherType == BitWardenCipherType.Unknown)
            {
                switch (value)
                {
                    case BitWardenCipherType.AesCbc256_HmacSha256_B64: CipherString = _CryptoService.CryptClearStringWithMasterKey(""); break;
                    case BitWardenCipherType.Rsa2048_OaepSha1_B64: CipherString = _CryptoService.CryptClearStringWithRSAKey(""); break;
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
            switch (CipherType)
            {
                case BitWardenCipherType.AesCbc256_HmacSha256_B64:
                {
                    var parts = CipherString.Split('.', 2);
                    var innerParts = parts[1].Split('|');
                    return Convert.FromBase64String(innerParts[1]);
                }

                case BitWardenCipherType.Rsa2048_OaepSha1_B64:
                {
                    var parts = CipherString.Split('.', 2);
                    var innerParts = parts[1].Split('|');
                    return Convert.FromBase64String(innerParts[0]);
                }
        }

            return null;
        }
    }
}
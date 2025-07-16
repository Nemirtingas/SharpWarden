// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession;
using SharpWarden.BitWardenDatabaseSession.Services;

namespace SharpWarden;

public class EncryptedString : ISessionAware
{
    [JsonIgnore] private IUserCryptoService _cryptoService;

    public string CipherString { get; set; }

    public EncryptedString()
    {
    }

    public EncryptedString(IUserCryptoService cryptoService)
    {
        _cryptoService = cryptoService;
    }

    public bool HasSession() => _cryptoService != null;

    public void SetCryptoService(IUserCryptoService cryptoService)
    {
        _cryptoService = cryptoService;
    }

    public EncryptedString Clone()
    {
        return new EncryptedString
        {
            _cryptoService = _cryptoService,
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

            if (_cryptoService == null)
                    throw new InvalidOperationException("The database session is not loaded.");

            return _cryptoService.GetClearStringAuto(CipherString);
        }

        set
        {
            if (_cryptoService == null)
                throw new InvalidOperationException("The database session is not loaded.");

            switch (CipherType)
            {
                case BitWardenCipherService.BitWardenCipherType.Unknown: // Common case of an unknown type would be to crypt with the master key.
                case BitWardenCipherService.BitWardenCipherType.AesCbc256HmacSha256B64: CipherString = _cryptoService.CryptClearStringWithMasterKey(value); return;
                case BitWardenCipherService.BitWardenCipherType.Rsa2048OaepSha1B64: CipherString = _cryptoService.CryptClearStringWithRsaKey(value); return;
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

            if (_cryptoService == null)
                throw new InvalidOperationException("The database session is not loaded.");

            switch(CipherType)
            {
                case BitWardenCipherService.BitWardenCipherType.AesCbc256HmacSha256B64: return _cryptoService.GetClearBytesWithMasterKey(CipherString);
                case BitWardenCipherService.BitWardenCipherType.Rsa2048OaepSha1B64: return _cryptoService.GetClearBytesWithRsaKey(CipherString);
            }

            throw new NotImplementedException();
        }

        set
        {
            if (_cryptoService == null)
                throw new InvalidOperationException("The database session is not loaded.");

            switch (CipherType)
            {
                case BitWardenCipherService.BitWardenCipherType.Unknown: // Common case of an unknown type would be to crypt with the master key.
                case BitWardenCipherService.BitWardenCipherType.AesCbc256HmacSha256B64: CipherString = _cryptoService.CryptClearBytesWithMasterKey(value); return;
                case BitWardenCipherService.BitWardenCipherType.Rsa2048OaepSha1B64: CipherString = _cryptoService.CryptClearBytesWithRsaKey(value); return;
            }

            throw new NotImplementedException();
        }
    }

    [JsonIgnore]
    public BitWardenCipherService.BitWardenCipherType CipherType
    {
        get
        {
            if (CipherString == null)
                return BitWardenCipherService.BitWardenCipherType.Unknown;

            var parts = CipherString.Split(".", 2);
            if (parts.Length <= 0 || !Enum.TryParse<BitWardenCipherService.BitWardenCipherType>(parts[0], out var type))
                return BitWardenCipherService.BitWardenCipherType.Unknown;

            return type;
        }

        set
        {
            var cipherType = CipherType;

            if (cipherType == value)
                return;

            if (cipherType == BitWardenCipherService.BitWardenCipherType.Unknown)
            {
                switch (value)
                {
                    case BitWardenCipherService.BitWardenCipherType.AesCbc256HmacSha256B64: CipherString = _cryptoService.CryptClearStringWithMasterKey(""); break;
                    case BitWardenCipherService.BitWardenCipherType.Rsa2048OaepSha1B64: CipherString = _cryptoService.CryptClearStringWithRsaKey(""); break;
                }
            }
        }
    }

    [JsonIgnore]
    public byte[] InitializationVector
    {
        get
        {
            if (CipherType != BitWardenCipherService.BitWardenCipherType.AesCbc256HmacSha256B64)
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
            if (CipherType != BitWardenCipherService.BitWardenCipherType.AesCbc256HmacSha256B64)
                return null;

            var parts = CipherString.Split('.', 2);
            var innerParts = parts[1].Split('|');

            byte[] mac;
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
                case BitWardenCipherService.BitWardenCipherType.AesCbc256HmacSha256B64:
                {
                    var parts = CipherString.Split('.', 2);
                    var innerParts = parts[1].Split('|');
                    return Convert.FromBase64String(innerParts[1]);
                }

                case BitWardenCipherService.BitWardenCipherType.Rsa2048OaepSha1B64:
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
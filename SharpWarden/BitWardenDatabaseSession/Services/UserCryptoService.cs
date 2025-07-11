// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

namespace SharpWarden.BitWardenDatabaseSession.Services;

public class UserCryptoService : DefaultCryptoService, IUserCryptoService, ICryptoService
{
    public UserCryptoService(IKeyProviderService keyProviderService) :
        base(keyProviderService)
    {
    }

    public string GetClearStringWithRSAKey(string cipherString)
        => GetClearStringWithRSAKey(null, cipherString);

    public string GetClearStringWithMasterKey(string cipherString)
        => GetClearStringWithMasterKey(null, cipherString);

    public string GetClearStringAuto(string cipherString)
        => GetClearStringAuto(null, cipherString);

    public byte[] GetClearBytesWithRSAKey(string cipherString)
        => GetClearBytesWithRSAKey(null, cipherString);

    public byte[] GetClearBytesWithMasterKey(string cipherString)
        => GetClearBytesWithMasterKey(null, cipherString);

    public byte[] GetClearBytesAuto(string cipherString)
        => GetClearBytesAuto(null, cipherString);

    public string CryptClearStringWithRSAKey(string clearString)
        => CryptClearStringWithRSAKey(null, clearString);

    public string CryptClearStringWithMasterKey(string clearString)
        => CryptClearStringWithMasterKey(null, clearString);

    public string CryptClearBytesWithRSAKey(byte[] bytes)
        => CryptClearBytesWithRSAKey(null, bytes);

    public string CryptClearBytesWithMasterKey(byte[] bytes)
        => CryptClearBytesWithMasterKey(null, bytes);

    public EncryptedString NewEncryptedString()
    {
        return new EncryptedString(this);
    }
}
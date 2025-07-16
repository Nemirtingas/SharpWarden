// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

namespace SharpWarden.BitWardenDatabaseSession.Services;

public interface IUserCryptoService : ICryptoService
{
    string GetClearStringWithRsaKey(string cipherString);
    string GetClearStringWithMasterKey(string cipherString);
    string GetClearStringAuto(string cipherString);
    byte[] GetClearBytesWithRsaKey(string cipherString);
    byte[] GetClearBytesWithMasterKey(string cipherString);
    byte[] GetClearBytesAuto(string cipherString);
    string CryptClearStringWithRsaKey(string clearString);
    string CryptClearStringWithMasterKey(string clearString);
    string CryptClearBytesWithRsaKey(byte[] bytes);
    string CryptClearBytesWithMasterKey(byte[] bytes);

    EncryptedString NewEncryptedString();
}
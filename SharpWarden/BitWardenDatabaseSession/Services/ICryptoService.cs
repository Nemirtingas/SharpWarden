// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using static SharpWarden.BitWardenCipherService;

namespace SharpWarden.BitWardenDatabaseSession.Services;

public interface ICryptoService
{
    IKeyProviderService KeyProviderService { get; }
    string GetClearStringWithRSAKey(Guid? organizationId, string cipherString);
    string GetClearStringWithMasterKey(Guid? organizationId, string cipherString);
    string GetClearStringAuto(Guid? organizationId, string cipherString);
    byte[] GetClearBytesWithRSAKey(Guid? organizationId, string cipherString);
    byte[] GetClearBytesWithMasterKey(Guid? organizationId, string cipherString);
    byte[] GetClearBytesAuto(Guid? organizationId, string cipherString);
    string CryptClearStringWithRSAKey(Guid? organizationId, string clearString);
    string CryptClearStringWithMasterKey(Guid? organizationId, string clearString);
    string CryptClearString(Guid? organizationId, string clearString, BitWardenCipherType cipherType);
    string CryptClearBytesWithRSAKey(Guid? organizationId, byte[] bytes);
    string CryptClearBytesWithMasterKey(Guid? organizationId, byte[] bytes);
    string CryptClearBytes(Guid? organizationId, byte[] bytes, BitWardenCipherType cipherType);
}
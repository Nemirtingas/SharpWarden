// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

namespace SharpWarden.BitWardenDatabaseSession.Services;

public class OrganizationCryptoService : DefaultCryptoService, IUserCryptoService
{
    private readonly Guid? _organizationId;

    public OrganizationCryptoService(
        IKeyProviderService keyProviderService,
        Guid? organizationId) :
        base(keyProviderService)
    {
        _organizationId = organizationId;
    }

    public string GetClearStringWithRsaKey(string cipherString)
        => GetClearStringWithRsaKey(_organizationId, cipherString);

    public string GetClearStringWithMasterKey(string cipherString)
        => GetClearStringWithMasterKey(_organizationId, cipherString);

    public string GetClearStringAuto(string cipherString)
        => GetClearStringAuto(_organizationId, cipherString);

    public byte[] GetClearBytesWithRsaKey(string cipherString)
        => GetClearBytesWithRsaKey(_organizationId, cipherString);

    public byte[] GetClearBytesWithMasterKey(string cipherString)
        => GetClearBytesWithMasterKey(_organizationId, cipherString);

    public byte[] GetClearBytesAuto(string cipherString)
        => GetClearBytesAuto(_organizationId, cipherString);

    public string CryptClearStringWithRsaKey(string clearString)
        => CryptClearStringWithRsaKey(_organizationId, clearString);

    public string CryptClearStringWithMasterKey(string clearString)
        => CryptClearStringWithMasterKey(_organizationId, clearString);

    public string CryptClearBytesWithRsaKey(byte[] bytes)
        => CryptClearBytesWithRsaKey(_organizationId, bytes);

    public string CryptClearBytesWithMasterKey(byte[] bytes)
        => CryptClearBytesWithMasterKey(_organizationId, bytes);

    public EncryptedString NewEncryptedString()
    {
        return new EncryptedString(this);
    }
}
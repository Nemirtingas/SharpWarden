// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

namespace SharpWarden.BitWardenDatabaseSession.Services;

public class OrganizationCryptoService : DefaultCryptoService, IUserCryptoService
{
    private readonly Guid? _OrganizationId;

    public OrganizationCryptoService(
        IKeyProviderService keyProviderService,
        Guid? organizationId) :
        base(keyProviderService)
    {
        _OrganizationId = organizationId;
    }

    public string GetClearStringWithRSAKey(string cipherString)
        => GetClearStringWithRSAKey(_OrganizationId, cipherString);

    public string GetClearStringWithMasterKey(string cipherString)
        => GetClearStringWithMasterKey(_OrganizationId, cipherString);

    public string GetClearStringAuto(string cipherString)
        => GetClearStringAuto(_OrganizationId, cipherString);

    public byte[] GetClearBytesWithRSAKey(string cipherString)
        => GetClearBytesWithRSAKey(_OrganizationId, cipherString);

    public byte[] GetClearBytesWithMasterKey(string cipherString)
        => GetClearBytesWithMasterKey(_OrganizationId, cipherString);

    public byte[] GetClearBytesAuto(string cipherString)
        => GetClearBytesAuto(_OrganizationId, cipherString);

    public string CryptClearStringWithRSAKey(string clearString)
        => CryptClearStringWithRSAKey(_OrganizationId, clearString);

    public string CryptClearStringWithMasterKey(string clearString)
        => CryptClearStringWithMasterKey(_OrganizationId, clearString);

    public string CryptClearBytesWithRSAKey(byte[] bytes)
        => CryptClearBytesWithRSAKey(_OrganizationId, bytes);

    public string CryptClearBytesWithMasterKey(byte[] bytes)
        => CryptClearBytesWithMasterKey(_OrganizationId, bytes);

    public EncryptedString NewEncryptedString()
    {
        return new EncryptedString(this);
    }
}
// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

namespace SharpWarden.BitWardenDatabaseSession.Services;

public class ConditionalCryptoService : DefaultCryptoService, IUserCryptoService
{
    private Func<Guid?> _OrganizationIdSelector;
    public ICryptoService CryptoService { get; set; }

    public ConditionalCryptoService(
        IKeyProviderService keyProviderService,
        Func<Guid?> organizationIdSelector):
        base(keyProviderService)
    {
        _OrganizationIdSelector = organizationIdSelector;
    }

    public string GetClearStringWithRSAKey(string cipherString)
        => GetClearStringWithRSAKey(_OrganizationIdSelector(), cipherString);

    public string GetClearStringWithMasterKey(string cipherString)
        => GetClearStringWithMasterKey(_OrganizationIdSelector(), cipherString);

    public string GetClearStringAuto(string cipherString)
        => GetClearStringAuto(_OrganizationIdSelector(), cipherString);

    public byte[] GetClearBytesWithRSAKey(string cipherString)
        => GetClearBytesWithRSAKey(_OrganizationIdSelector(), cipherString);

    public byte[] GetClearBytesWithMasterKey(string cipherString)
        => GetClearBytesWithMasterKey(_OrganizationIdSelector(), cipherString);

    public byte[] GetClearBytesAuto(string cipherString)
        => GetClearBytesAuto(_OrganizationIdSelector(), cipherString);

    public string CryptClearStringWithRSAKey(string clearString)
        => CryptClearStringWithRSAKey(_OrganizationIdSelector(), clearString);

    public string CryptClearStringWithMasterKey(string clearString)
        => CryptClearStringWithMasterKey(_OrganizationIdSelector(), clearString);

    public string CryptClearBytesWithRSAKey(byte[] bytes)
        => CryptClearBytesWithRSAKey(_OrganizationIdSelector(), bytes);

    public string CryptClearBytesWithMasterKey(byte[] bytes)
        => CryptClearBytesWithMasterKey(_OrganizationIdSelector(), bytes);

    public EncryptedString NewEncryptedString()
    {
        return new EncryptedString(this);
    }
}
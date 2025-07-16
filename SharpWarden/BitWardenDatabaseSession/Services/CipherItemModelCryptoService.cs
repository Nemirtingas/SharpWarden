// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

namespace SharpWarden.BitWardenDatabaseSession.Services;

public class ConditionalCryptoService : DefaultCryptoService, IUserCryptoService
{
    private readonly Func<Guid?> _organizationIdSelector;
    public ICryptoService CryptoService { get; set; }

    public ConditionalCryptoService(
        IKeyProviderService keyProviderService,
        Func<Guid?> organizationIdSelector):
        base(keyProviderService)
    {
        _organizationIdSelector = organizationIdSelector;
    }

    public string GetClearStringWithRsaKey(string cipherString)
        => GetClearStringWithRsaKey(_organizationIdSelector(), cipherString);

    public string GetClearStringWithMasterKey(string cipherString)
        => GetClearStringWithMasterKey(_organizationIdSelector(), cipherString);

    public string GetClearStringAuto(string cipherString)
        => GetClearStringAuto(_organizationIdSelector(), cipherString);

    public byte[] GetClearBytesWithRsaKey(string cipherString)
        => GetClearBytesWithRsaKey(_organizationIdSelector(), cipherString);

    public byte[] GetClearBytesWithMasterKey(string cipherString)
        => GetClearBytesWithMasterKey(_organizationIdSelector(), cipherString);

    public byte[] GetClearBytesAuto(string cipherString)
        => GetClearBytesAuto(_organizationIdSelector(), cipherString);

    public string CryptClearStringWithRsaKey(string clearString)
        => CryptClearStringWithRsaKey(_organizationIdSelector(), clearString);

    public string CryptClearStringWithMasterKey(string clearString)
        => CryptClearStringWithMasterKey(_organizationIdSelector(), clearString);

    public string CryptClearBytesWithRsaKey(byte[] bytes)
        => CryptClearBytesWithRsaKey(_organizationIdSelector(), bytes);

    public string CryptClearBytesWithMasterKey(byte[] bytes)
        => CryptClearBytesWithMasterKey(_organizationIdSelector(), bytes);

    public EncryptedString NewEncryptedString()
    {
        return new EncryptedString(this);
    }
}
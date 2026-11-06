// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using System.Security.Cryptography;

namespace SharpWarden.BitWardenDatabaseSession.Services;

public class ConditionalCryptoService : DefaultCryptoService, IUserCryptoService
{
    private readonly Func<Guid?> _organizationIdSelector;
    private readonly Func<string> _itemKeySelector;

    // Last decrypted item key, swapped atomically: (owner, encrypted item key, decrypted keys).
    private Tuple<Guid?, string, UserKeys> _itemKeyCache;

    public ICryptoService CryptoService { get; set; }

    public ConditionalCryptoService(
        IKeyProviderService keyProviderService,
        Func<Guid?> organizationIdSelector):
        this(keyProviderService, organizationIdSelector, null)
    {
    }

    /// <param name="keyProviderService">Provider of the user and organization keys.</param>
    /// <param name="organizationIdSelector">Owner of the item (null for the user).</param>
    /// <param name="itemKeySelector">
    /// Optional per-item key (e.g. a cipher "key"), encrypted with the owner key. When set, the item data
    /// is encrypted with this key instead of the owner key.
    /// </param>
    public ConditionalCryptoService(
        IKeyProviderService keyProviderService,
        Func<Guid?> organizationIdSelector,
        Func<string> itemKeySelector):
        base(keyProviderService)
    {
        _organizationIdSelector = organizationIdSelector;
        _itemKeySelector = itemKeySelector;
    }

    protected override UserKeys GetKeys(Guid? organizationId)
    {
        var ownerKeys = base.GetKeys(organizationId);

        var itemKey = _itemKeySelector?.Invoke();
        if (string.IsNullOrEmpty(itemKey))
            return ownerKeys;

        var cache = _itemKeyCache;
        if (cache != null && cache.Item1 == organizationId && cache.Item2 == itemKey)
            return cache.Item3;

        var rawKey = BitWardenCipherService.DecryptWithAesCbc256HmacSha256Base64(itemKey, ownerKeys.SymmetricKey, ownerKeys.SymmetricMac);
        try
        {
            if (rawKey.Length != 64)
                throw new CryptographicException("Invalid decrypted item key length.");

            var itemKeys = new UserKeys
            {
                SymmetricKey = rawKey[..32],
                SymmetricMac = rawKey[32..],
                RsaKey = ownerKeys.RsaKey,
            };

            _itemKeyCache = Tuple.Create(organizationId, itemKey, itemKeys);
            return itemKeys;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(rawKey);
        }
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
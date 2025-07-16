// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using System.Text;
using SharpWarden.BitWardenDatabaseSession.Models;

namespace SharpWarden.BitWardenDatabaseSession.Services;

public class DefaultVaultService : IVaultService
{
    private readonly ISessionJsonConverterService _jsonService;
    private readonly IKeyProviderService _keyProviderService;
    private DatabaseModel _database;

    public DefaultVaultService(
        ISessionJsonConverterService jsonService,
        IKeyProviderService keyProviderService)
    {
        _jsonService = jsonService;
        _keyProviderService = keyProviderService;
    }

    public void LoadBitWardenDatabase(byte[] userPassword, int kdfIterations, Stream stream)
    {
        LoadBitWardenDatabase(userPassword, kdfIterations, _jsonService.Deserialize<DatabaseModel>(stream));
    }

    public void LoadBitWardenDatabase(byte[] userPassword, int kdfIterations, DatabaseModel database)
    {
        _database = database;
        _keyProviderService.LoadBitWardenUserKey(
            Encoding.UTF8.GetBytes(database.Profile.Email),
            userPassword,
            kdfIterations,
            database.Profile.Key.CipherString,
            database.Profile.PrivateKey.CipherString);

        foreach (var organization in _database.Profile.Organizations)
            _keyProviderService.LoadBitWardenOrganizationKey(organization.Id, organization.Key);
    }

    public void ReloadBitWardenDatabase(DatabaseModel database)
    {
        _database = database;
    }

    public DatabaseModel GetBitWardenDatabase()
    {
        return _database;
    }

    public async Task DecryptAttachmentAsync(Stream cryptedAttachment, byte[] key, Stream output)
    {
        await BitWardenCipherService.DecryptWithAesCbc256HmacSha256Base64ByteStreamAsync(cryptedAttachment, output, key).ConfigureAwait(false);
    }

    public async Task EncryptAttachmentAsync(Stream clearAttachment, byte[] key, Stream output)
    {
        await BitWardenCipherService.EncryptWithAesCbc256HmacSha256Base64ByteStreamAsync(clearAttachment, output, key).ConfigureAwait(false);
    }
}
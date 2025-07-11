// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using System.Text;
using SharpWarden.BitWardenDatabaseSession.Models;

namespace SharpWarden.BitWardenDatabaseSession.Services;

public class DefaultVaultService : IVaultService
{
    private readonly ISessionJsonConverterService _JsonService;
    private readonly IKeyProviderService _KeyProviderService;
    private DatabaseModel _Database;

    public DefaultVaultService(
        ISessionJsonConverterService jsonService,
        IKeyProviderService keyProviderService
    )
    {
        _JsonService = jsonService;
        _KeyProviderService = keyProviderService;
    }

    public void LoadBitWardenDatabase(byte[] userPassword, int kdfIterations, Stream stream)
    {
        LoadBitWardenDatabase(userPassword, kdfIterations, _JsonService.Deserialize<DatabaseModel>(stream));
    }

    public void LoadBitWardenDatabase(byte[] userPassword, int kdfIterations, DatabaseModel database)
    {
        _Database = database;
        _KeyProviderService.LoadBitWardenUserKey(
            Encoding.UTF8.GetBytes(database.Profile.Email),
            userPassword,
            kdfIterations,
            database.Profile.Key.CipherString,
            database.Profile.PrivateKey.CipherString);

        foreach (var organization in _Database.Profile.Organizations)
            _KeyProviderService.LoadBitWardenOrganizationKey(organization.Id, organization.Key);
    }

    public void ReloadBitWardenDatabase(DatabaseModel database)
    {
        _Database = database;
    }

    public DatabaseModel GetBitWardenDatabase()
    {
        return _Database;
    }

    public async Task DecryptAttachmentAsync(Stream cryptedAttachment, byte[] key, Stream output)
    {
        await BitWardenCipherService.DecryptWithAesCbc256HmacSha256Base64ByteStreamAsync(cryptedAttachment, output, key);
    }

    public async Task EncryptAttachmentAsync(Stream clearAttachment, byte[] key, Stream output)
    {
        await BitWardenCipherService.EncryptWithAesCbc256HmacSha256Base64ByteStreamAsync(clearAttachment, output, key);
    }
}
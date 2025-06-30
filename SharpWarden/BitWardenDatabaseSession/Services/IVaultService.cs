// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using SharpWarden.BitWardenDatabaseSession.Models;

namespace SharpWarden.BitWardenDatabaseSession.Services;

public interface IVaultService
{
    /// <summary>
    /// Sets the internal database reference to the one passed in parameter.
    /// </summary>
    /// <param name="database"></param>
    void ReloadBitWardenDatabase(DatabaseModel database);
    /// <summary>
    /// This will load a database, presumably downloaded via the <see cref="WebClient.WebClient.GetDatabaseAsync"/>.
    /// </summary>
    /// <param name="userPassword"></param>
    /// <param name="kdfIterations"></param>
    /// <param name="stream">The stream must be Json convertible to <see cref="DatabaseModel"/></param>
    void LoadBitWardenDatabase(byte[] userPassword, int kdfIterations, Stream stream);
    /// <summary>
    /// This will load a database, presumably downloaded via the <see cref="WebClient.WebClient.GetDatabaseAsync"/>.
    /// </summary>
    /// <param name="userPassword"></param>
    /// <param name="kdfIterations"></param>
    /// <param name="database"></param>
    void LoadBitWardenDatabase(byte[] userPassword, int kdfIterations, DatabaseModel database);
    /// <summary>
    /// Call this after loading the database: <see cref="LoadBitWardenDatabase"/> .
    /// </summary>
    /// <returns></returns>
    DatabaseModel GetBitWardenDatabase();
    /// <summary>
    /// Decrypt an already downloaded attachment. <see cref="WebClient.WebClient.GetAttachmentAsync"/>
    /// </summary>
    /// <param name="cryptedAttachment"></param>
    /// <param name="key"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    Task DecryptAttachmentAsync(Stream cryptedAttachment, byte[] key, Stream output);
    Task EncryptAttachmentAsync(Stream clearAttachment, byte[] key, Stream output);
}
// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using SharpWarden.BitWardenWebSession.Models;
using SharpWarden.BitWardenDatabaseSession.Models;
using SharpWarden.BitWardenDatabaseSession.Models.FolderItem;
using SharpWarden.BitWardenDatabaseSession.Models.CipherItem;
using SharpWarden.BitWardenDatabaseSession.Models.ProfileItem;
using SharpWarden.BitWardenDatabaseSession.Models.CollectionItem;

namespace SharpWarden.WebClient.Services;

public interface IWebClientService
{
    public const string Fido2KeyCipherMinimumVersion = "2023.10.0";
    public const string SSHKeyCipherMinimumVersion = "2024.12.0";
    public const string DenyLegacyUserMinimumVersion = "2025.6.0";

    public const string BitWardenComHostUrl = "https://vault.bitwarden.com";
    public const string BitWardenEUHostUrl = "https://vault.bitwarden.eu";

    public LoginModel GetWebSession();

    public int UserKdfIterations { get; }
    public string UserKey { get; }
    public string UserPrivateKey { get; }

    public DateTime ExpiresAt { get; }

    /// <summary>
    /// Device identifier sent to the server on login. Persist it and pass it back to
    /// <see cref="BitWardenHelper.CreateSessionScope(string, string, Guid)"/> so the server recognizes
    /// the device: otherwise each session is a new device (new device notification mail / OTP).
    /// </summary>
    public Guid DeviceId { get; }

    /// <summary>
    /// Id of the authenticated user (read from the access token).
    /// </summary>
    public Guid UserId { get; }

    public Task PreloginAsync(string username, CancellationToken cancellationToken = default);

    public Task AuthenticateAsync(string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// </summary>
    /// <param name="password"></param>
    /// <param name="newDeviceOtpAsyncCallback">A callback that will be run if a device OTP is needed to login.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="BitWardenHttpRequestException"></exception>
    public Task AuthenticateAsync(string password, Func<Task<string>> newDeviceOtpAsyncCallback, CancellationToken cancellationToken = default);

    public Task AuthenticateWithRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    public Task AuthenticateWithApiKeyAsync(string clientId, string clientSecret, CancellationToken cancellationToken = default);

    public Task<CipherItemModel> GetCipherItemAsync(Guid id, CancellationToken cancellationToken = default);

    public Task<List<CipherItemModel>> GetCipherItemsAsync(CancellationToken cancellationToken = default);

    public Task<AttachmentModel> GetCipherItemAttachmentAsync(Guid id, string attachmentId, CancellationToken cancellationToken = default);

    public Task<AttachmentModel> CreateCipherItemAttachmentAsync(Guid id, string encryptedFileName, string encryptedKey, Stream attachmentStream, CancellationToken cancellationToken = default);

    public Task DeleteCipherItemAttachmentAsync(Guid id, string attachmentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an user item. Can also be used to create an organization item but you will need to provide the OrganizationId AND at least one CollectionId. 
    /// </summary>
    /// <param name="cipherItem"></param>
    /// <returns></returns>
    /// <exception cref="InvalidDataException"></exception>
    public Task<CipherItemModel> CreateCipherItemAsync(CipherItemModel cipherItem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create an organization item.
    /// </summary>
    /// <param name="cipherItem"></param>
    /// <param name="collectionIds"></param>
    /// <returns></returns>
    /// <exception cref="InvalidDataException"></exception>
    public Task<CipherItemModel> CreateOrganizationCipherItemAsync(CipherItemModel cipherItem, IEnumerable<Guid> collectionIds, CancellationToken cancellationToken = default);

    public Task<CipherItemModel> UpdateCipherItemAsync(Guid id, CipherItemModel cipherItem, CancellationToken cancellationToken = default);

    public Task<CipherItemModel> UpdateCipherItemCollectionsAsync(Guid id, List<Guid> newCollections, CancellationToken cancellationToken = default);

    public Task MoveToTrashCipherItemAsync(Guid id, CancellationToken cancellationToken = default);

    public Task RestoreCipherItemAsync(Guid id, CancellationToken cancellationToken = default);

    public Task DeleteCipherItemAsync(Guid id, CancellationToken cancellationToken = default);

    public Task DeleteCipherItemsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    public Task<FolderItemModel> GetFolderAsync(Guid id, CancellationToken cancellationToken = default);

    public Task<List<FolderItemModel>> GetFoldersAsync(CancellationToken cancellationToken = default);

    public Task<FolderItemModel> CreateFolderAsync(string encryptedName, CancellationToken cancellationToken = default);

    public Task<FolderItemModel> UpdateFolderAsync(Guid id, string encryptedName, CancellationToken cancellationToken = default);

    public Task DeleteFolderAsync(Guid id, CancellationToken cancellationToken = default);

    public Task DeleteFoldersAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    public Task DeleteAllFolderAsync(CancellationToken cancellationToken = default);

    public Task<CollectionItemModel> CreateCollectionAsync(Guid organizationId, string encryptedName, List<UserCollectionPermissionsModel> users, List<UserCollectionPermissionsModel> groups, CancellationToken cancellationToken = default);

    public Task<List<CollectionItemModel>> GetCollectionsAsync(Guid organizationId, CancellationToken cancellationToken = default);

    public Task DeleteCollectionAsync(Guid organizationId, Guid collectionId, CancellationToken cancellationToken = default);

    public Task<Stream> GetAttachmentAsync(AttachmentModel attachment, CancellationToken cancellationToken = default);

    public Task<ProfileItemModel> GetAccountProfileAsync(CancellationToken cancellationToken = default);

    public Task<DatabaseModel> GetDatabaseAsync(bool excludeDomains = true, CancellationToken cancellationToken = default);
}
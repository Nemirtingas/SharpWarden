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

    public Task PreloginAsync(string username);

    public Task AuthenticateAsync(string password);

    public Task AuthenticateAsync(string password, Func<Task<string>> newDeviceOtpAsyncCallback);

    public Task AuthenticateWithRefreshTokenAsync(string refreshToken);

    public Task AuthenticateWithApiKeyAsync(string clientId, string clientSecret);

    public Task<CipherItemModel> GetCipherItemAsync(Guid id);

    public Task<List<CipherItemModel>> GetCipherItemsAsync();

    public Task<AttachmentModel> GetCipherItemAttachmentAsync(Guid id, string attachmentId);

    public Task<AttachmentModel> CreateCipherItemAttachmentAsync(Guid id, string encryptedFileName, string encryptedKey, Stream attachmentStream);

    public Task DeleteCipherItemAttachmentAsync(Guid id, string attachmentId);

    /// <summary>
    /// Creates an user item. Can also be used to create an organization item but you will need to provide the OrganizationId AND at least one CollectionId. 
    /// </summary>
    /// <param name="cipherItem"></param>
    /// <returns></returns>
    /// <exception cref="InvalidDataException"></exception>
    public Task<CipherItemModel> CreateCipherItemAsync(CipherItemModel cipherItem);

    /// <summary>
    /// Create an organization item.
    /// </summary>
    /// <param name="cipherItem"></param>
    /// <param name="collectionIds"></param>
    /// <returns></returns>
    /// <exception cref="InvalidDataException"></exception>
    public Task<CipherItemModel> CreateOrganizationCipherItemAsync(CipherItemModel cipherItem, IEnumerable<Guid> collectionIds);

    public Task<CipherItemModel> UpdateCipherItemAsync(Guid id, CipherItemModel cipherItem);

    public Task MoveToTrashCipherItemAsync(Guid id);

    public Task RestoreCipherItemAsync(Guid id);

    public Task DeleteCipherItemAsync(Guid id);

    public Task DeleteCipherItemsAsync(IEnumerable<Guid> ids);

    public Task<FolderItemModel> GetFolderAsync(Guid id);

    public Task<List<FolderItemModel>> GetFoldersAsync();

    public Task<FolderItemModel> CreateFolderAsync(string encryptedName);

    public Task<FolderItemModel> UpdateFolderAsync(Guid id, string encryptedName);

    public Task DeleteFolderAsync(Guid id);

    public Task DeleteFoldersAsync(IEnumerable<Guid> ids);

    public Task DeleteAllFolderAsync();

    public Task<CollectionItemModel> CreateCollectionAsync(Guid organizationId, string encryptedName, List<UserCollectionPermissionsModel> users, List<UserCollectionPermissionsModel> groups);

    public Task<List<CollectionItemModel>> GetCollectionsAsync(Guid organizationId);

    public Task DeleteCollectionAsync(Guid organizationId, Guid collectionId);

    public Task<Stream> GetAttachmentAsync(AttachmentModel attachment);

    public Task<ProfileItemModel> GetAccountProfileAsync();

    public Task<DatabaseModel> GetDatabaseAsync(bool excludeDomains = true);
}
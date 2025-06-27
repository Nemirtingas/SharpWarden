using SharpWarden.BitWardenWebSession.Models;
using SharpWarden.BitWardenDatabaseSession.Models;
using SharpWarden.BitWardenDatabaseSession.Models.FolderItem;
using SharpWarden.BitWardenDatabaseSession.Models.CipherItem;
using SharpWarden.BitWardenDatabaseSession.Models.ProfileItem;

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

    public Task<CipherItemModel> CreateCipherItemAsync(CipherItemModel cipherItem);

    public Task<CipherItemModel> UpdateCipherItemAsync(Guid id, CipherItemModel cipherItem);

    public Task DeleteCipherItemAsync(Guid id);

    public Task DeleteCipherItemsAsync(IEnumerable<Guid> ids);

    public Task<FolderItemModel> GetFolderAsync(Guid id);

    public Task<List<FolderItemModel>> GetFoldersAsync();

    public Task<FolderItemModel> CreateFolderAsync(string encryptedName);

    public Task<FolderItemModel> UpdateFolderAsync(Guid id, string encryptedName);

    public Task DeleteFolderAsync(Guid id);

    public Task DeleteFoldersAsync(IEnumerable<Guid> ids);

    public Task DeleteAllFolderAsync();

    public Task<Stream> GetAttachmentAsync(AttachmentModel attachment);

    public Task<ProfileItemModel> GetAccountProfileAsync();

    public Task<DatabaseModel> GetDatabaseAsync(bool excludeDomains = true);
}
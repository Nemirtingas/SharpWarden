using System.Net.Http.Headers;
using System.Text;
using SharpWarden.BitWardenWebSession.Models;
using SharpWarden.WebClient.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpWarden.BitWardenDatabaseSession.Models;
using SharpWarden.BitWardenDatabaseSession.FolderItem.Models;
using SharpWarden.BitWardenDatabaseSession.CipherItem.Models;
using SharpWarden.BitWardenDatabaseSession.ProfileItem.Models;

namespace SharpWarden.WebClient;

public class WebClient
{
    private Guid _Guid;
    private readonly HttpClient _HttpClient;
    private LoginModel _WebSession;
    private string _Username;
    private JsonSerializer _JsonSerializer;

    public WebClient(string baseUrl)
    {
        _Guid = Guid.NewGuid();
        _WebSession = new LoginModel();
        _HttpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _JsonSerializer = new JsonSerializer();
        _JsonSerializer.Converters.Add(new EncryptedStringConverter());
    }

    private T _Deserialize<T>(Stream stream)
    {
        using (var sr = new StreamReader(stream))
        using (var jsonTextReader = new JsonTextReader(sr))
        {
            return _JsonSerializer.Deserialize<T>(jsonTextReader);
        }
    }

    private string _Serialize<T>(T value)
        => JsonConvert.SerializeObject(value);

    private StringContent _APIModelToContent<T>(T apiModel)
    {
        return new StringContent(_Serialize(apiModel), Encoding.UTF8, new MediaTypeHeaderValue("application/json", "charset=utf-8"));
    }

    public LoginModel GetWebSession() => _WebSession.Clone();

    public int UserKdfIterations => _WebSession.KdfIterations;
    public string UserKey => _WebSession.Key;
    public string UserPrivateKey => _WebSession.PrivateKey;

    public async Task<bool> PreloginAsync(string username)
    {
        var content = new StringContent(new JObject { { "email", username } }.ToString(), new UTF8Encoding(false), "application/json");
        var response = await _HttpClient.PostAsync("/identity/accounts/prelogin", content);
        response.EnsureSuccessStatusCode();

        _WebSession.KdfIterations = _Deserialize<PreLoginModel>(await response.Content.ReadAsStreamAsync()).KdfIterations;
        _Username = username;

        return true;
    }

    public async Task<bool> AuthenticateAsync(string password)
    {
        if (string.IsNullOrWhiteSpace(_Username) || !(_WebSession?.KdfIterations > 0))
            throw new InvalidOperationException("Prelogin must be called prior to " + nameof(AuthenticateAsync) + " .");

        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var masterKey = BitWardenCipherService.ComputeMasterKey(Encoding.UTF8.GetBytes(_Username), passwordBytes, _WebSession.KdfIterations);
        var userPasswordHash = BitWardenCipherService.ComputeUserPasswordHash(masterKey, passwordBytes);

        var parameters = new Dictionary<string, string>{
            { "scope"           , "api offline_access" },
            { "client_id"       , "web" },
            { "deviceType"      , $"{(int)DeviceType.Sdk}" },
            { "deviceIdentifier", _Guid.ToString() },
            { "deviceName"      , "SharpWarden" },
            { "grant_type"      , "password" },
            { "username"        , _Username },
            { "password"        , Convert.ToBase64String(userPasswordHash) },
        };

        var content = new FormUrlEncodedContent(parameters);

        var response = await _HttpClient.PostAsync("/identity/connect/token", content);
        response.EnsureSuccessStatusCode();

        _WebSession = _Deserialize<LoginModel>(await response.Content.ReadAsStreamAsync());

        _HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _WebSession.AccessToken);

        return true;
    }

    public async Task<bool> AuthenticateWithRefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(_Username) || !(_WebSession?.KdfIterations > 0))
            throw new InvalidOperationException("Prelogin must be called prior to " + nameof(AuthenticateWithRefreshTokenAsync) + " .");

        var parameters = new Dictionary<string, string>{
            { "client_id"       , "web"           },
            { "grant_type"      , "refresh_token" },
            { "refresh_token"   , refreshToken    },
        };

        var content = new FormUrlEncodedContent(parameters);

        var response = await _HttpClient.PostAsync("/identity/connect/token", content);
        response.EnsureSuccessStatusCode();

        _WebSession.UpdateSession(_Deserialize<RefreshModel>(await response.Content.ReadAsStreamAsync()));

        _HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _WebSession.AccessToken);

        var profile = await GetProfileAsync();

        _WebSession.Key = profile.Key.CipherString;
        _WebSession.PrivateKey = profile.PrivateKey.CipherString;

        return true;
    }

    public async Task<CipherItemModel> GetCipherItemAsync(Guid id)
    {
        var response = await _HttpClient.GetAsync($"/api/ciphers/{id}");
        response.EnsureSuccessStatusCode();
        return _Deserialize<CipherItemModel>(await response.Content.ReadAsStreamAsync());
    }

    public async Task<List<CipherItemModel>> GetCipherItemsAsync()
    {
        var response = await _HttpClient.GetAsync("/api/ciphers");
        response.EnsureSuccessStatusCode();
        return _Deserialize<ApiResultModel<List<CipherItemModel>>>(await response.Content.ReadAsStreamAsync()).Data;
    }

    public async Task<FolderItemModel> CreateFolderAsync(string encryptedName)
    {
        var content = _APIModelToContent(new CreateFolderAPIModel
        {
            Name = encryptedName
        });

        var response = await _HttpClient.PostAsync("/api/folders", content);
        response.EnsureSuccessStatusCode();
        return _Deserialize<FolderItemModel>(await response.Content.ReadAsStreamAsync());
    }

    public async Task<FolderItemModel> UpdateFolderAsync(Guid folderId, string encryptedName)
    {
        var content = _APIModelToContent(new UpdateFolderAPIModel
        {
            Name = encryptedName
        });

        var response = await _HttpClient.PutAsync($"/api/folders/{folderId}", content);
        response.EnsureSuccessStatusCode();
        return _Deserialize<FolderItemModel>(await response.Content.ReadAsStreamAsync());
    }

    public async Task<FolderItemModel> DeleteFolderAsync(Guid folderId)
    {
        var response = await _HttpClient.DeleteAsync($"/api/folders/{folderId}");
        response.EnsureSuccessStatusCode();
        return _Deserialize<FolderItemModel>(await response.Content.ReadAsStreamAsync());
    }

    public async Task<ProfileItemModel> GetProfileAsync()
    {
        var response = await _HttpClient.GetAsync("/api/accounts/profile");
        response.EnsureSuccessStatusCode();
        return _Deserialize<ProfileItemModel>(await response.Content.ReadAsStreamAsync());
    }

    public async Task<DatabaseModel> GetDatabaseAsync()
    {
        var response = await _HttpClient.GetAsync("/api/sync?excludeDomains=true");
        response.EnsureSuccessStatusCode();
        return _Deserialize<DatabaseModel>(await response.Content.ReadAsStreamAsync());
    }
}
using System.Net.Http.Headers;
using System.Text;
using BitwardenSharp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BitwardenSharp;

public class VaultwardenWebClient
{
    private Guid _Guid;
    private readonly HttpClient _HttpClient;
    private BitWardenWebSessionLoginModel _WebSession;
    private string _Username;

    public VaultwardenWebClient(string baseUrl)
    {
        _Guid = Guid.NewGuid();
        _HttpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public async Task<bool> PreloginAsync(string username)
    {
        var content = new StringContent(new JObject { { "email", username } }.ToString(), new UTF8Encoding(false), "application/json");
        var response = await _HttpClient.PostAsync("/identity/accounts/prelogin", content);
        response.EnsureSuccessStatusCode();

        var serializer = new JsonSerializer();
        using (var sr = new StreamReader(await response.Content.ReadAsStreamAsync()))
        using (var jsonTextReader = new JsonTextReader(sr))
        {
            if (_WebSession == null)
                _WebSession = new();

            _WebSession.KdfIterations = serializer.Deserialize<BitWardenWebSessionPreLoginModel>(jsonTextReader).KdfIterations;
            _Username = username;
        }

        return true;
    }

    public async Task<bool> AuthenticateAsync(string password)
    {
        if (string.IsNullOrWhiteSpace(_Username) || !(_WebSession?.KdfIterations > 0))
            throw new InvalidOperationException("Prelogin must be called prior to Authenticate.");

        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var masterKey = BitwardenCipherService.ComputeMasterKey(Encoding.UTF8.GetBytes(_Username), passwordBytes, _WebSession.KdfIterations);
        var userPasswordHash = BitwardenCipherService.ComputeUserPasswordHash(masterKey, passwordBytes);

        var parameters = new Dictionary<string, string>{
            { "scope"           , "api offline_access" },
            { "client_id"       , "web" },
            { "deviceType"      , "21" }, // SDK client
            { "deviceIdentifier", _Guid.ToString() },
            { "deviceName"      , "SharpWarden" },
            { "grant_type"      , "password" },
            { "username"        , _Username },
            { "password"        , Convert.ToBase64String(userPasswordHash) },
        };

        var content = new FormUrlEncodedContent(parameters);

        var response = await _HttpClient.PostAsync("/identity/connect/token", content);
        response.EnsureSuccessStatusCode();

        var serializer = new JsonSerializer();
        serializer.Converters.Add(new BitWardenEncryptedStringConverter());

        using (var sr = new StreamReader(await response.Content.ReadAsStreamAsync()))
        using (var jsonTextReader = new JsonTextReader(sr))
        {
            _WebSession = serializer.Deserialize<BitWardenWebSessionLoginModel>(jsonTextReader);
        }

        Console.WriteLine($"Token: {_WebSession.AccessToken}");

        return true;
    }

    public async Task<Stream> SyncAsync()
    {
        _HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _WebSession.AccessToken);
        var response = await _HttpClient.GetAsync("/api/sync?excludeDomains=true");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync();
    }

    public int UserKdfIterations => _WebSession.KdfIterations;
}
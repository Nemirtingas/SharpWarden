using System.Net.Http.Headers;
using System.Text;
using SharpWarden.BitWardenWebSession.Models;
using SharpWarden.WebClient.Models;
using Newtonsoft.Json.Linq;
using SharpWarden.BitWardenDatabaseSession.Models;
using SharpWarden.BitWardenDatabaseSession.Models.FolderItem;
using SharpWarden.BitWardenDatabaseSession.Models.CipherItem;
using SharpWarden.BitWardenDatabaseSession.Models.ProfileItem;
using SharpWarden.BitWardenDatabaseSession;
using SharpWarden.BitWardenDatabaseSession.Services;
using SharpWarden.WebClient.Exceptions;

namespace SharpWarden.WebClient;

public class WebClient
{
    private Guid _Guid;
    private string _DeviceVersion;
    private readonly HttpClient _HttpClient;
    private LoginModel _WebSession;
    private string _Username;
    private readonly ISessionJsonConverterService _JsonSerializer;

    public const string Fido2KeyCipherMinimumVersion = "2023.10.0";
    public const string SSHKeyCipherMinimumVersion = "2024.12.0";
    public const string DenyLegacyUserMinimumVersion = "2025.6.0";

    public const string BitWardenComHostUrl = "https://vault.bitwarden.com";
    public const string BitWardenEUHostUrl = "https://vault.bitwarden.eu";

    /// <summary>
    /// If <paramref name="deviceVersion"/> is not at least <see cref="SSHKeyCipherMinimumVersion"/>, ssh keys will not be returned by <see cref="GetDatabaseAsync"/>.
    /// </summary>
    /// <param name="jsonSerializer"></param>
    /// <param name="bitwardenHostUrl"></param>
    /// <param name="deviceVersion"></param>
    /// <param name="deviceId"></param>
    public WebClient(
        ISessionJsonConverterService jsonSerializer,
        string bitwardenHostUrl,
        string deviceVersion,
        Guid? deviceId = null)
    {
        _JsonSerializer = jsonSerializer;
        _Guid = deviceId ?? Guid.NewGuid();
        _DeviceVersion = string.IsNullOrWhiteSpace(deviceVersion) ? DenyLegacyUserMinimumVersion : deviceVersion;
        _WebSession = new LoginModel();
        _HttpClient = new HttpClient{ BaseAddress = new Uri(bitwardenHostUrl) };
        _HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:138.0) Gecko/20100101 Firefox/138.0");
        _HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("device-type", $"{(int)DeviceType.Sdk}");
        _HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Bitwarden-Client-Name", "web");
        _HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Bitwarden-Client-Version", _DeviceVersion);
    }

    private T _Deserialize<T>(Stream stream)
        => _JsonSerializer.Deserialize<T>(stream);

    private string _Serialize<T>(T value)
        => _JsonSerializer.Serialize(value);

    private StringContent _APIModelToContent<T>(T apiModel)
    {
        return new StringContent(_Serialize(apiModel), Encoding.UTF8, new MediaTypeHeaderValue("application/json", "charset=utf-8"));
    }

    private async Task<HttpResponseMessage> _PostAuthenticateAsync(byte[] passwordBytes, string otp)
    {
        if (string.IsNullOrWhiteSpace(_Username) || !(_WebSession?.KdfIterations > 0))
            throw new InvalidOperationException("Prelogin must be called prior to " + nameof(AuthenticateAsync) + " .");

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

        if (!string.IsNullOrWhiteSpace(otp))
            parameters.Add("newDeviceOtp", otp);

        var content = new FormUrlEncodedContent(parameters);
        var response = await _HttpClient.PostAsync("/identity/connect/token", content);

        if (response.StatusCode != System.Net.HttpStatusCode.BadRequest)
            response.EnsureSuccessStatusCode();

        return response;
    }

    public LoginModel GetWebSession() => _WebSession.Clone();

    public int UserKdfIterations => _WebSession.KdfIterations;
    public string UserKey => _WebSession.Key;
    public string UserPrivateKey => _WebSession.PrivateKey;

    public DateTime ExpiresAt { get; private set; }

    // Not sure how to make this work, it always returns false.
    //public async Task<bool> KnownDeviceAsync(string username)
    //{
    //    var request = new HttpRequestMessage(HttpMethod.Get, "/api/devices/knowndevice");
    //    request.Headers.TryAddWithoutValidation("x-device-identifier", _Guid.ToString());
    //    request.Headers.TryAddWithoutValidation("x-request-email", "???");
    //    var response = await _HttpClient.SendAsync(request);
    //    response.EnsureSuccessStatusCode();
    //    return bool.Parse(await response.Content.ReadAsStringAsync());
    //}

    public async Task PreloginAsync(string username)
    {
        var content = new StringContent(new JObject { { "email", username } }.ToString(), new UTF8Encoding(false), "application/json");
        var response = await _HttpClient.PostAsync("/identity/accounts/prelogin", content);
        response.EnsureSuccessStatusCode();

        _WebSession.KdfIterations = _Deserialize<PreLoginModel>(await response.Content.ReadAsStreamAsync()).KdfIterations;
        _Username = username;
    }

    public Task AuthenticateAsync(string password)
        => AuthenticateAsync(password, null);

    /// <summary>
    /// </summary>
    /// <param name="password"></param>
    /// <param name="newDeviceOtpAsyncCallback">A callback that will be run if a device OTP is needed to login.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="BitWardenHttpRequestException"></exception>
    public async Task AuthenticateAsync(string password, Func<Task<string>> newDeviceOtpAsyncCallback)
    {
        if (string.IsNullOrWhiteSpace(_Username) || !(_WebSession?.KdfIterations > 0))
            throw new InvalidOperationException("Prelogin must be called prior to " + nameof(AuthenticateAsync) + " .");

        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var response = await _PostAuthenticateAsync(passwordBytes, null);
        var responseAt = DateTime.UtcNow;
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            var genericError = _Deserialize<ErrorResponseModel>(await response.Content.ReadAsStreamAsync());
            if (genericError.ErrorType == ErrorType.DeviceError && newDeviceOtpAsyncCallback != null)
            {
                var otp = await newDeviceOtpAsyncCallback();
                if (string.IsNullOrWhiteSpace(otp))
                    throw new BitWardenHttpRequestException(genericError.ErrorType ?? ErrorType.Unknown, genericError.Description);

                response = await _PostAuthenticateAsync(passwordBytes, otp);
                responseAt = DateTime.UtcNow;
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    genericError = _Deserialize<ErrorResponseModel>(await response.Content.ReadAsStreamAsync());
                    throw new BitWardenHttpRequestException(genericError.ErrorType ?? ErrorType.Unknown, genericError.Description);
                }
            }
            else
            {
                throw new BitWardenHttpRequestException(genericError.ErrorType ?? ErrorType.Unknown, genericError.Description);
            }
        }

        _WebSession = _Deserialize<LoginModel>(await response.Content.ReadAsStreamAsync());
        ExpiresAt = responseAt.AddSeconds(-_WebSession.ExpiresIn - 300);

        _HttpClient.DefaultRequestHeaders.Remove("Bearer");
        _HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _WebSession.AccessToken);
    }

    public async Task AuthenticateWithRefreshTokenAsync(string refreshToken)
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
        var responseAt = DateTime.UtcNow;
        response.EnsureSuccessStatusCode();

        _WebSession.UpdateSession(_Deserialize<RefreshModel>(await response.Content.ReadAsStreamAsync()));
        ExpiresAt = responseAt.AddSeconds(-_WebSession.ExpiresIn - 300);

        _HttpClient.DefaultRequestHeaders.Remove("Bearer");
        _HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _WebSession.AccessToken);

        var profile = await GetAccountProfileAsync();

        _WebSession.Key = profile.Key.CipherString;
        _WebSession.PrivateKey = profile.PrivateKey.CipherString;
    }

    public async Task AuthenticateWithApiKeyAsync(string clientId, string clientSecret)
    {
        var parameters = new Dictionary<string, string>{
            { "scope"           , "api"                    },
            { "grant_type"      , "client_credentials"     },
            { "deviceType"      , $"{(int)DeviceType.Sdk}" },
            { "deviceIdentifier", _Guid.ToString()         },
            { "deviceName"      , "SharpWarden"            },
            { "client_id"       , clientId                 },
            { "client_secret"   , clientSecret             },
        };

        var content = new FormUrlEncodedContent(parameters);

        var response = await _HttpClient.PostAsync("/identity/connect/token", content);
        var responseAt = DateTime.UtcNow;
        response.EnsureSuccessStatusCode();

        _WebSession.UpdateSession(_Deserialize<ApiKeyLoginModel>(await response.Content.ReadAsStreamAsync()));
        ExpiresAt = responseAt.AddSeconds(-_WebSession.ExpiresIn - 300);

        _HttpClient.DefaultRequestHeaders.Remove("Bearer");
        _HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _WebSession.AccessToken);

        var profile = await GetAccountProfileAsync();

        _WebSession.Key = profile.Key.CipherString;
        _WebSession.PrivateKey = profile.PrivateKey.CipherString;
    }

    #region Common API

    private async Task<Stream> _GetAPIAsync(string apiPath)
    {
        var response = await _HttpClient.GetAsync(apiPath);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync();
    }

    private async Task<T> _GetAPIAsync<T>(string apiPath)
    {
        var response = await _HttpClient.GetAsync(apiPath);
        response.EnsureSuccessStatusCode();
        var result = _Deserialize<T>(await response.Content.ReadAsStreamAsync());
        return result;
    }

    private async Task<ApiResultModel<List<T>>> _GetAllAPIAsync<T>(string apiPath) where T : ISessionAware
    {
        var response = await _HttpClient.GetAsync(apiPath);
        response.EnsureSuccessStatusCode();
        var result = _Deserialize<ApiResultModel<List<T>>>(await response.Content.ReadAsStreamAsync());

        return result;
    }

    private async Task<T> _CreateAPIAsync<T, U>(string apiPath, U apiModel) where T : ISessionAware
    {
        var content = _APIModelToContent(apiModel);

        var response = await _HttpClient.PostAsync(apiPath, content);
        response.EnsureSuccessStatusCode();
        var result = _Deserialize<T>(await response.Content.ReadAsStreamAsync());
        return result;
    }

    private async Task<T> _UpdateAPIAsync<T, U>(string apiPath, Guid id, U apiModel) where T : ISessionAware
    {
        var content = _APIModelToContent(apiModel);

        var response = await _HttpClient.PutAsync($"{apiPath}/{id}", content);
        response.EnsureSuccessStatusCode();
        var result = _Deserialize<T>(await response.Content.ReadAsStreamAsync());
        return result;
    }

    private async Task _DeleteAPIAsync(string apiPath)
    {
        var response = await _HttpClient.DeleteAsync(apiPath);
        response.EnsureSuccessStatusCode();
    }

    private async Task _MultiDeleteAPIAsync(string apiPath, IEnumerable<Guid> ids)
    {
        var apiModel = new MultiDeleteRequestAPIModel();

        var content = _APIModelToContent(apiModel);

        var response = await _HttpClient.PutAsync($"{apiPath}/delete", content);
        response.EnsureSuccessStatusCode();
    }

    #endregion

    #region CipherItem API

    const string CiphersApiPath = "/api/ciphers";

    private void _FillCipherItemAPIModel(CipherItemBaseRequestAPIModel apiModel, CipherItemModel itemModel)
    {
        apiModel.Name = itemModel.Name?.CipherString;
        apiModel.Notes = itemModel.Notes?.CipherString;
        apiModel.Favorite = itemModel.Favorite;
        apiModel.FolderId = itemModel.FolderId;
        apiModel.OrganizationId = itemModel.OrganizationId;
        apiModel.Fields = itemModel.Fields == null ? null : new List<CustomFieldAPIModel>(itemModel.Fields.Select(e => new CustomFieldAPIModel
        {
            Type = e.Type,
            Name = e.Name?.CipherString,
            Value = e.Value?.CipherString,
            LinkedId = e.LinkedId,
        }));
        apiModel.Reprompt = itemModel.Reprompt;
        apiModel.LastKnownRevisionDate = null;
    }

    private CipherItemLoginRequestAPIModel _BuildCipherItemLoginRequestAsync(CipherItemModel itemModel)
    {
        var apiModel = new CipherItemLoginRequestAPIModel
        {
            Login = new CipherLoginAPIModel
            {
                AutofillOnPageLoad = itemModel.Login.AutoFillOnPageLoad,
                Password = itemModel.Login.Password?.CipherString,
                PasswordRevisionDate = itemModel.Login.PasswordRevisionDate,
                TOTP = itemModel.Login.TOTP?.CipherString,
                Uris = itemModel.Login.Uris == null ? null : new List<CipherLoginUriAPIModel>(itemModel.Login.Uris.Select(e => new CipherLoginUriAPIModel
                {
                    Uri = e.Uri?.CipherString,
                    UriChecksum = e.UriChecksum?.CipherString,
                    Match = e.Match,
                })),
                Username = itemModel.Login.Username?.CipherString,
            }
        };
        _FillCipherItemAPIModel(apiModel, itemModel);
        return apiModel;
    }

    private CipherItemSecureNoteRequestAPIModel _BuildCipherItemSecureNoteRequestAsync(CipherItemModel itemModel)
    {
        var apiModel = new CipherItemSecureNoteRequestAPIModel
        {
            SecureNote = new CipherSecureNoteAPIModel
            {
                SecureNoteType = itemModel.SecureNote.Type,
            }
        };
        _FillCipherItemAPIModel(apiModel, itemModel);
        return apiModel;
    }

    private CipherItemCardRequestAPIModel _BuildCipherItemCardRequestAsync(CipherItemModel itemModel)
    {
        var apiModel = new CipherItemCardRequestAPIModel
        {
            Card = new CipherCardAPIModel
            {
                Brand = itemModel.Card.Brand?.CipherString,
                CardholderName = itemModel.Card.CardholderName?.CipherString,
                Code = itemModel.Card.Code?.CipherString,
                ExpMonth = itemModel.Card.ExpMonth?.CipherString,
                ExpYear = itemModel.Card.ExpYear?.CipherString,
                Number = itemModel.Card.Number?.CipherString,
            }
        };
        _FillCipherItemAPIModel(apiModel, itemModel);
        return apiModel;
    }

    private CipherItemIdentityRequestAPIModel _BuildCipherItemIdentityRequestAsync(CipherItemModel itemModel)
    {
        var apiModel = new CipherItemIdentityRequestAPIModel
        {
            Identity = new CipherIdentityAPIModel
            {
                Address1 = itemModel.Identity.Address1?.CipherString,
                Address2 = itemModel.Identity.Address2?.CipherString,
                Address3 = itemModel.Identity.Address3?.CipherString,
                City = itemModel.Identity.City?.CipherString,
                Company = itemModel.Identity.Company?.CipherString,
                Country = itemModel.Identity.Country?.CipherString,
                Email = itemModel.Identity.Email?.CipherString,
                FirstName = itemModel.Identity.FirstName?.CipherString,
                LastName = itemModel.Identity.LastName?.CipherString,
                LicenseNumber = itemModel.Identity.LicenseNumber?.CipherString,
                MiddleName = itemModel.Identity.MiddleName?.CipherString,
                PassportNumber = itemModel.Identity.PassportNumber?.CipherString,
                Phone = itemModel.Identity.Phone?.CipherString,
                PostalCode = itemModel.Identity.PostalCode?.CipherString,
                SSN = itemModel.Identity.SSN?.CipherString,
                State = itemModel.Identity.State?.CipherString,
                Title = itemModel.Identity.Title?.CipherString,
                Username = itemModel.Identity.Username?.CipherString,
            }
        };
        _FillCipherItemAPIModel(apiModel, itemModel);
        return apiModel;
    }

    public async Task<CipherItemModel> GetCipherItemAsync(Guid id)
    {
        return await _GetAPIAsync<CipherItemModel>($"{CiphersApiPath}/{id}");
    }

    public async Task<List<CipherItemModel>> GetCipherItemsAsync()
    {
        return (await _GetAllAPIAsync<CipherItemModel>(CiphersApiPath)).Data;
    }

    public async Task<AttachmentModel> GetCipherItemAttachmentAsync(Guid id, string attachmentId)
    {
        return await _GetAPIAsync<AttachmentModel>($"{CiphersApiPath}/{id}/attachment/{attachmentId}");
    }

    public async Task<AttachmentModel> CreateCipherItemAttachmentAsync(Guid id, string encryptedFileName, string encryptedKey, Stream attachmentStream)
    {
        var createAttachmentModel = new CipherItemCreateAttachmentRequestAPIModel
        {
            AdminRequest = false,
            FileSize = attachmentStream.Length,
            FileName = encryptedFileName,
            Key = encryptedKey
        };

        var content = _APIModelToContent(createAttachmentModel);

        var response = await _HttpClient.PostAsync($"{CiphersApiPath}/{id}/attachment/v2", content);
        response.EnsureSuccessStatusCode();
        var newAttachment = _Deserialize<CreateCipherItemAttachmentResponseAPIResultModel>(await response.Content.ReadAsStreamAsync());
        try
        {
            using var form = new MultipartFormDataContent();

            var fileContent = new StreamContent(attachmentStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            form.Add(fileContent, "data", encryptedFileName);

            response = await _HttpClient.PostAsync($"{CiphersApiPath}/{id}/attachment/{newAttachment.AttachmentId}", form);
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            try
            {
                await DeleteCipherItemAttachmentAsync(id, newAttachment.AttachmentId);
            }
            catch
            {

            }
            throw;
        }

        return newAttachment.CipherResponse.Attachments.Find(e => e.Id == newAttachment.AttachmentId);
    }

    public async Task DeleteCipherItemAttachmentAsync(Guid id, string attachmentId)
    {
        await _DeleteAPIAsync($"{CiphersApiPath}/{id}/attachment/{attachmentId}");
    }

    public async Task<CipherItemModel> CreateCipherItemAsync(CipherItemModel cipherItem)
    {
        switch (cipherItem.ItemType)
        {
            case CipherItemType.Login: return await _CreateAPIAsync<CipherItemModel, CipherItemLoginRequestAPIModel>(CiphersApiPath, _BuildCipherItemLoginRequestAsync(cipherItem));
            case CipherItemType.SecureNote: return await _CreateAPIAsync<CipherItemModel, CipherItemSecureNoteRequestAPIModel>(CiphersApiPath, _BuildCipherItemSecureNoteRequestAsync(cipherItem));
            case CipherItemType.Card: return await _CreateAPIAsync<CipherItemModel, CipherItemCardRequestAPIModel>(CiphersApiPath, _BuildCipherItemCardRequestAsync(cipherItem));
            case CipherItemType.Identity: return await _CreateAPIAsync<CipherItemModel, CipherItemIdentityRequestAPIModel>(CiphersApiPath, _BuildCipherItemIdentityRequestAsync(cipherItem));
        }

        throw new InvalidDataException($"Unhandled cipher item type: {cipherItem.ItemType}");
    }

    public async Task<CipherItemModel> UpdateCipherItemAsync<T>(Guid id, CipherItemModel cipherItem)
    {
        switch (cipherItem.ItemType)
        {
            case CipherItemType.Login     : return await _UpdateAPIAsync<CipherItemModel, CipherItemLoginRequestAPIModel>(CiphersApiPath, id, _BuildCipherItemLoginRequestAsync(cipherItem));
            case CipherItemType.SecureNote: return await _UpdateAPIAsync<CipherItemModel, CipherItemSecureNoteRequestAPIModel>(CiphersApiPath, id, _BuildCipherItemSecureNoteRequestAsync(cipherItem));
            case CipherItemType.Card      : return await _UpdateAPIAsync<CipherItemModel, CipherItemCardRequestAPIModel>(CiphersApiPath, id, _BuildCipherItemCardRequestAsync(cipherItem));
            case CipherItemType.Identity  : return await _UpdateAPIAsync<CipherItemModel, CipherItemIdentityRequestAPIModel>(CiphersApiPath, id, _BuildCipherItemIdentityRequestAsync(cipherItem));
        }

        throw new InvalidDataException($"Unhandled cipher item type: {cipherItem.ItemType}");
    }

    public async Task DeleteCipherItemAsync(Guid id)
    {
        await _DeleteAPIAsync($"{CiphersApiPath}/{id}");
    }

    public async Task DeleteCipherItemsAsync(IEnumerable<Guid> ids)
    {
        await _MultiDeleteAPIAsync(CiphersApiPath, ids);
    }

    #endregion

    #region Folder API

    const string FoldersApiPath = "/api/folders";

    private FolderRequestAPIModel _BuildFolderRequestAPIModel(string encryptedName)
    {
        return new FolderRequestAPIModel
        {
            Name = encryptedName
        };
    }

    public async Task<FolderItemModel> GetFolderAsync(Guid id)
    {
        return await _GetAPIAsync<FolderItemModel>($"{FoldersApiPath}/{id}");
    }

    public async Task<List<FolderItemModel>> GetFoldersAsync()
    {
        return (await _GetAllAPIAsync<FolderItemModel>(FoldersApiPath)).Data;
    }

    public async Task<FolderItemModel> CreateFolderAsync(string encryptedName)
    {
        return await _CreateAPIAsync<FolderItemModel, FolderRequestAPIModel>(FoldersApiPath, _BuildFolderRequestAPIModel(encryptedName));
    }

    public async Task<FolderItemModel> UpdateFolderAsync(Guid id, string encryptedName)
    {
        return await _UpdateAPIAsync<FolderItemModel, FolderRequestAPIModel>(FoldersApiPath, id, _BuildFolderRequestAPIModel(encryptedName));
    }

    public async Task DeleteFolderAsync(Guid id)
    {
        await _DeleteAPIAsync($"{FoldersApiPath}/{id}");
    }

    public async Task DeleteFoldersAsync(IEnumerable<Guid> ids)
    {
        await _MultiDeleteAPIAsync(FoldersApiPath, ids);
    }

    public async Task DeleteAllFolderAsync()
    {
        var response = await _HttpClient.DeleteAsync($"/api/folders/all");
        response.EnsureSuccessStatusCode();
    }

    #endregion

    #region Attachment API

    public async Task<Stream> GetAttachmentAsync(AttachmentModel attachment)
    {
        return await _GetAPIAsync(attachment.Url);
    }

    #endregion

    public async Task<ProfileItemModel> GetAccountProfileAsync()
    {
        return await _GetAPIAsync<ProfileItemModel>("/api/accounts/profile");
    }

    public async Task<DatabaseModel> GetDatabaseAsync(bool excludeDomains = true)
    {
        var strBool = excludeDomains ? "true" : "false"; // Because bool.ToString() is CamelCase
        return await _GetAPIAsync<DatabaseModel>($"/api/sync?excludeDomains={strBool}");
    }
}
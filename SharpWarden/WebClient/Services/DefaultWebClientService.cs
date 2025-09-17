// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

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
using SharpWarden.BitWardenDatabaseSession.Models.CollectionItem;

namespace SharpWarden.WebClient.Services;

public class DefaultWebClientService : IWebClientService, IDisposable
{
    private readonly Guid _guid;
    private readonly HttpClient _httpClient;
    private LoginModel _webSession;
    private string _username;
    private readonly ISessionJsonConverterService _jsonSerializer;
    private readonly string _baseUrl;

    private const int AuthTokenExpirationThreshold = 60;

    /// <summary>
    /// If <paramref name="deviceVersion"/> is not at least <see cref="SSHKeyCipherMinimumVersion"/>, ssh keys will not be returned by <see cref="GetDatabaseAsync"/>.
    /// </summary>
    /// <param name="jsonSerializer"></param>
    /// <param name="bitwardenHostUrl"></param>
    /// <param name="deviceVersion"></param>
    /// <param name="deviceId"></param>
    public DefaultWebClientService(
        ISessionJsonConverterService jsonSerializer,
        string bitwardenHostUrl,
        string deviceVersion,
        Guid? deviceId = null)
    {
        deviceVersion = string.IsNullOrWhiteSpace(deviceVersion) ? IWebClientService.DenyLegacyUserMinimumVersion : deviceVersion;

        _jsonSerializer = jsonSerializer;
        _guid = deviceId ?? Guid.NewGuid();
        _webSession = new LoginModel();
        _httpClient = new HttpClient();
        _baseUrl = bitwardenHostUrl.TrimEnd('/');
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:138.0) Gecko/20100101 Firefox/138.0");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("device-type", $"{(int)DeviceType.Sdk}");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Bitwarden-Client-Name", "web");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Bitwarden-Client-Version", deviceVersion);
    }

    private T _Deserialize<T>(Stream stream)
        => _jsonSerializer.Deserialize<T>(stream);

    private string _Serialize<T>(T value)
        => _jsonSerializer.Serialize(value);

    private StringContent _APIModelToContent<T>(T apiModel)
    {
        return new StringContent(_Serialize(apiModel), Encoding.UTF8, new MediaTypeHeaderValue("application/json", "utf-8"));
    }

    private async Task<HttpResponseMessage> _PostAuthenticateAsync(byte[] passwordBytes, string otp)
    {
        if (string.IsNullOrWhiteSpace(_username) || !(_webSession?.KdfIterations > 0))
            throw new InvalidOperationException("Prelogin must be called prior to " + nameof(AuthenticateAsync) + " .");

        var masterKey = BitWardenCipherService.ComputeMasterKey(Encoding.UTF8.GetBytes(_username), passwordBytes, _webSession.KdfIterations);
        var userPasswordHash = BitWardenCipherService.ComputeUserPasswordHash(masterKey, passwordBytes);

        var parameters = new Dictionary<string, string>{
            { "scope"           , "api offline_access" },
            { "client_id"       , "web" },
            { "deviceType"      , $"{(int)DeviceType.Sdk}" },
            { "deviceIdentifier", _guid.ToString() },
            { "deviceName"      , "SharpWarden" },
            { "grant_type"      , "password" },
            { "username"        , _username },
            { "password"        , Convert.ToBase64String(userPasswordHash) },
        };

        if (!string.IsNullOrWhiteSpace(otp))
            parameters.Add("newDeviceOtp", otp);

        var content = new FormUrlEncodedContent(parameters);
        var response = await _httpClient.PostAsync($"{_baseUrl}/identity/connect/token", content).ConfigureAwait(false);

        if (response.StatusCode != System.Net.HttpStatusCode.BadRequest)
            response.EnsureSuccessStatusCode();

        return response;
    }

    public LoginModel GetWebSession() => _webSession.Clone();

    public int UserKdfIterations => _webSession.KdfIterations;
    public string UserKey => _webSession.Key;
    public string UserPrivateKey => _webSession.PrivateKey;

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
        var response = await _httpClient.PostAsync($"{_baseUrl}/identity/accounts/prelogin", content).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        _webSession.KdfIterations = _Deserialize<PreLoginModel>(await response.Content.ReadAsStreamAsync().ConfigureAwait(false)).KdfIterations;
        _username = username;
    }

    public Task AuthenticateAsync(string password)
        => AuthenticateAsync(password, null);

    public async Task AuthenticateAsync(string password, Func<Task<string>> newDeviceOtpAsyncCallback)
    {
        if (string.IsNullOrWhiteSpace(_username) || !(_webSession?.KdfIterations > 0))
            throw new InvalidOperationException("Prelogin must be called prior to " + nameof(AuthenticateAsync) + " .");

        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var response = await _PostAuthenticateAsync(passwordBytes, null).ConfigureAwait(false);
        var responseAt = DateTime.UtcNow;
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            var genericError = _Deserialize<ErrorResponseModel>(await response.Content.ReadAsStreamAsync().ConfigureAwait(false));
            if (genericError.ErrorType == ErrorType.DeviceError && newDeviceOtpAsyncCallback != null)
            {
                var otp = await newDeviceOtpAsyncCallback().ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(otp))
                    throw new BitWardenHttpRequestException(genericError.ErrorType ?? ErrorType.Unknown, genericError.Description);

                response = await _PostAuthenticateAsync(passwordBytes, otp).ConfigureAwait(false);
                responseAt = DateTime.UtcNow;
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    genericError = _Deserialize<ErrorResponseModel>(await response.Content.ReadAsStreamAsync().ConfigureAwait(false));
                    throw new BitWardenHttpRequestException(genericError.ErrorType ?? ErrorType.Unknown, genericError.Description);
                }
            }
            else
            {
                throw new BitWardenHttpRequestException(genericError.ErrorType ?? ErrorType.Unknown, genericError.Description);
            }
        }

        _webSession = _Deserialize<LoginModel>(await response.Content.ReadAsStreamAsync().ConfigureAwait(false));
        ExpiresAt = responseAt.AddSeconds(_webSession.ExpiresIn - AuthTokenExpirationThreshold);

        _httpClient.DefaultRequestHeaders.Remove("Bearer");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _webSession.AccessToken);
    }

    public async Task AuthenticateWithRefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(_username) || !(_webSession?.KdfIterations > 0))
            throw new InvalidOperationException("Prelogin must be called prior to " + nameof(AuthenticateWithRefreshTokenAsync) + " .");

        var parameters = new Dictionary<string, string>{
            { "client_id"       , "web"           },
            { "grant_type"      , "refresh_token" },
            { "refresh_token"   , refreshToken    },
        };

        var content = new FormUrlEncodedContent(parameters);

        var response = await _httpClient.PostAsync($"{_baseUrl}/identity/connect/token", content).ConfigureAwait(false);
        var responseAt = DateTime.UtcNow;
        response.EnsureSuccessStatusCode();

        _webSession.UpdateSession(_Deserialize<RefreshModel>(await response.Content.ReadAsStreamAsync().ConfigureAwait(false)));
        ExpiresAt = responseAt.AddSeconds(_webSession.ExpiresIn - AuthTokenExpirationThreshold);

        _httpClient.DefaultRequestHeaders.Remove("Bearer");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _webSession.AccessToken);

        var profile = await GetAccountProfileAsync().ConfigureAwait(false);

        _webSession.Key = profile.Key.CipherString;
        _webSession.PrivateKey = profile.PrivateKey.CipherString;
    }

    public async Task AuthenticateWithApiKeyAsync(string clientId, string clientSecret)
    {
        var parameters = new Dictionary<string, string>{
            { "scope"           , "api"                    },
            { "grant_type"      , "client_credentials"     },
            { "deviceType"      , $"{(int)DeviceType.Sdk}" },
            { "deviceIdentifier", _guid.ToString()         },
            { "deviceName"      , "SharpWarden"            },
            { "client_id"       , clientId                 },
            { "client_secret"   , clientSecret             },
        };

        var content = new FormUrlEncodedContent(parameters);

        var response = await _httpClient.PostAsync($"{_baseUrl}/identity/connect/token", content).ConfigureAwait(false);
        var responseAt = DateTime.UtcNow;
        response.EnsureSuccessStatusCode();

        _webSession.UpdateSession(_Deserialize<ApiKeyLoginModel>(await response.Content.ReadAsStreamAsync().ConfigureAwait(false)));
        ExpiresAt = responseAt.AddSeconds(_webSession.ExpiresIn - AuthTokenExpirationThreshold);

        _httpClient.DefaultRequestHeaders.Remove("Bearer");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _webSession.AccessToken);

        var profile = await GetAccountProfileAsync().ConfigureAwait(false);

        _webSession.Key = profile.Key.CipherString;
        _webSession.PrivateKey = profile.PrivateKey.CipherString;
    }

    #region Common API

    private async Task<Stream> _GetAPIAsync(string apiPath)
    {
        var response = await _httpClient.GetAsync(apiPath).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
    }

    private async Task<T> _GetAPIAsync<T>(string apiPath)
    {
        var response = await _httpClient.GetAsync(apiPath).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var result = _Deserialize<T>(await response.Content.ReadAsStreamAsync().ConfigureAwait(false));
        return result;
    }

    private async Task<ApiResultModel<List<T>>> _GetAllAPIAsync<T>(string apiPath) where T : ISessionAware
    {
        var response = await _httpClient.GetAsync(apiPath).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var result = _Deserialize<ApiResultModel<List<T>>>(await response.Content.ReadAsStreamAsync().ConfigureAwait(false));

        return result;
    }

    private async Task<TReturn> _CreateAPIAsync<TReturn, TModel>(string apiPath, TModel apiModel) where TReturn : ISessionAware
    {
        var content = _APIModelToContent(apiModel);

        var response = await _httpClient.PostAsync(apiPath, content).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var result = _Deserialize<TReturn>(await response.Content.ReadAsStreamAsync().ConfigureAwait(false));
        return result;
    }

    private async Task<TReturn> _UpdateAPIAsync<TReturn, TModel>(string apiPath, Guid id, TModel apiModel) where TReturn : ISessionAware
    {
        var content = _APIModelToContent(apiModel);

        var response = await _httpClient.PutAsync($"{apiPath}/{id}", content).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var result = _Deserialize<TReturn>(await response.Content.ReadAsStreamAsync().ConfigureAwait(false));
        return result;
    }

    private async Task<CipherItemModel> _UpdateItemCollectionsAPIAsync(string apiPath, IEnumerable<Guid> newCollectionIds)
    {
        var content = _APIModelToContent(new CipherItemUpdateCollectionsRequestAPIModel
        {
            CollectionIds = newCollectionIds.ToList()
        });

        var response = await _httpClient.PutAsync($"{apiPath}", content).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var result = _Deserialize<CipherItemUpdateCollectionsResponseAPIModel>(await response.Content.ReadAsStreamAsync().ConfigureAwait(false));
        return result.Cipher;
    }

    private async Task _MoveTrashAPIAsync(string apiPath)
    {
        var response = await _httpClient.PutAsync($"{apiPath}", null).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    private async Task _RestoreAPIAsync(string apiPath)
    {
        var response = await _httpClient.PutAsync($"{apiPath}", null).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    private async Task _DeleteAPIAsync(string apiPath)
    {
        var response = await _httpClient.DeleteAsync(apiPath).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    private async Task _MultiDeleteAPIAsync(string apiPath, IEnumerable<Guid> ids)
    {
        var apiModel = new MultiDeleteRequestAPIModel
        {
            Ids = [.. ids]
        };

        var content = _APIModelToContent(apiModel);

        var response = await _httpClient.PutAsync($"{apiPath}/delete", content).ConfigureAwait(false);
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
        apiModel.PasswordHistory = itemModel.PasswordHistory == null ? null : new List<PasswordHistoryAPIModel>(itemModel.PasswordHistory.Select(e => new PasswordHistoryAPIModel
        {
            LastUsedDate = e.LastUsedDate,
            Password = e.Password.CipherString
        }));
        apiModel.Reprompt = itemModel.Reprompt;
        apiModel.LastKnownRevisionDate = itemModel.RevisionDate;
    }

    private CipherItemLoginRequestAPIModel _BuildCipherItemLoginRequest(CipherItemModel itemModel)
    {
        var apiModel = new CipherItemLoginRequestAPIModel
        {
            Login = new CipherLoginAPIModel
            {
                AutofillOnPageLoad = itemModel.Login.AutoFillOnPageLoad,
                Password = itemModel.Login.Password?.CipherString,
                PasswordRevisionDate = itemModel.Login.PasswordRevisionDate,
                TOTP = itemModel.Login.Totp?.CipherString,
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

    private CipherItemSecureNoteRequestAPIModel _BuildCipherItemSecureNoteRequest(CipherItemModel itemModel)
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

    private CipherItemCardRequestAPIModel _BuildCipherItemCardRequest(CipherItemModel itemModel)
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

    private CipherItemIdentityRequestAPIModel _BuildCipherItemIdentityRequest(CipherItemModel itemModel)
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
                SSN = itemModel.Identity.SecuritySocialNumber?.CipherString,
                State = itemModel.Identity.State?.CipherString,
                Title = itemModel.Identity.Title?.CipherString,
                Username = itemModel.Identity.Username?.CipherString,
            }
        };
        _FillCipherItemAPIModel(apiModel, itemModel);
        return apiModel;
    }

    private CipherItemSSHKeyRequestAPIModel _BuildCipherItemSSHKeyRequest(CipherItemModel itemModel)
    {
        var apiModel = new CipherItemSSHKeyRequestAPIModel
        {
            SSHKey = new CipherSSHKeyAPIModel
            {
                KeyFingerprint = itemModel.SshKey.KeyFingerprint.CipherString,
                PublicKey = itemModel.SshKey.PublicKey.CipherString,
                PrivateKey = itemModel.SshKey.PrivateKey.CipherString,
            }
        };
        _FillCipherItemAPIModel(apiModel, itemModel);
        return apiModel;
    }

    private CipherItemCreateRequestAPIModel<CipherItemLoginRequestAPIModel> _BuildCreateCipherItemLoginRequest(CipherItemModel cipherItem, IEnumerable<Guid> collectionIds)
    {
        var apiModel = new CipherItemCreateRequestAPIModel<CipherItemLoginRequestAPIModel>
        {
            Cipher = _BuildCipherItemLoginRequest(cipherItem),
            CollectionIds = collectionIds?.ToList() ?? new()
        };
        return apiModel;
    }

    private CipherItemCreateRequestAPIModel<CipherItemSecureNoteRequestAPIModel> _BuildCreateCipherItemSecureNoteRequest(CipherItemModel cipherItem, IEnumerable<Guid> collectionIds)
    {
        var apiModel = new CipherItemCreateRequestAPIModel<CipherItemSecureNoteRequestAPIModel>
        {
            Cipher = _BuildCipherItemSecureNoteRequest(cipherItem),
            CollectionIds = collectionIds?.ToList() ?? new()
        };
        return apiModel;
    }

    private CipherItemCreateRequestAPIModel<CipherItemCardRequestAPIModel> _BuildCreateCipherItemCardRequest(CipherItemModel cipherItem, IEnumerable<Guid> collectionIds)
    {
        var apiModel = new CipherItemCreateRequestAPIModel<CipherItemCardRequestAPIModel>
        {
            Cipher = _BuildCipherItemCardRequest(cipherItem),
            CollectionIds = collectionIds?.ToList() ?? new()
        };
        return apiModel;
    }

    private CipherItemCreateRequestAPIModel<CipherItemIdentityRequestAPIModel> _BuildCreateCipherItemIdentityRequest(CipherItemModel cipherItem, IEnumerable<Guid> collectionIds)
    {
        var apiModel = new CipherItemCreateRequestAPIModel<CipherItemIdentityRequestAPIModel>
        {
            Cipher = _BuildCipherItemIdentityRequest(cipherItem),
            CollectionIds = collectionIds?.ToList() ?? new()
        };
        return apiModel;
    }

    private CipherItemCreateRequestAPIModel<CipherItemSSHKeyRequestAPIModel> _BuildCreateCipherItemSSHKeyRequest(CipherItemModel cipherItem, IEnumerable<Guid> collectionIds)
    {
        var apiModel = new CipherItemCreateRequestAPIModel<CipherItemSSHKeyRequestAPIModel>
        {
            Cipher = _BuildCipherItemSSHKeyRequest(cipherItem),
            CollectionIds = collectionIds?.ToList() ?? new()
        };
        return apiModel;
    }

    public async Task<CipherItemModel> GetCipherItemAsync(Guid id)
    {
        return await _GetAPIAsync<CipherItemModel>($"{_baseUrl}{CiphersApiPath}/{id}").ConfigureAwait(false);
    }

    public async Task<List<CipherItemModel>> GetCipherItemsAsync()
    {
        return (await _GetAllAPIAsync<CipherItemModel>($"{_baseUrl}{CiphersApiPath}").ConfigureAwait(false)).Data;
    }

    public async Task<AttachmentModel> GetCipherItemAttachmentAsync(Guid id, string attachmentId)
    {
        return await _GetAPIAsync<AttachmentModel>($"{_baseUrl}{CiphersApiPath}/{id}/attachment/{attachmentId}").ConfigureAwait(false);
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

        var response = await _httpClient.PostAsync($"{_baseUrl}{CiphersApiPath}/{id}/attachment/v2", content).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var newAttachment = _Deserialize<CreateCipherItemAttachmentResponseAPIResultModel>(await response.Content.ReadAsStreamAsync().ConfigureAwait(false));
        try
        {
            using var form = new MultipartFormDataContent();

            var fileContent = new StreamContent(attachmentStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            form.Add(fileContent, "data", encryptedFileName);

            response = await _httpClient.PostAsync($"{_baseUrl}{CiphersApiPath}/{id}/attachment/{newAttachment.AttachmentId}", form).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            try
            {
                await DeleteCipherItemAttachmentAsync(id, newAttachment.AttachmentId).ConfigureAwait(false);
            }
            catch
            {
                // ignore
            }
            throw;
        }

        return newAttachment.CipherResponse.Attachments.Find(e => e.Id == newAttachment.AttachmentId);
    }

    public async Task DeleteCipherItemAttachmentAsync(Guid id, string attachmentId)
    {
        await _DeleteAPIAsync($"{_baseUrl}{CiphersApiPath}/{id}/attachment/{attachmentId}").ConfigureAwait(false);
    }

    public async Task<CipherItemModel> CreateCipherItemAsync(CipherItemModel cipherItem)
    {
        if (cipherItem.OrganizationId == null)
        {
            switch (cipherItem.ItemType)
            {
                case CipherItemType.Login     : return await _CreateAPIAsync<CipherItemModel, CipherItemCreateRequestAPIModel<CipherItemLoginRequestAPIModel>>($"{_baseUrl}{CiphersApiPath}/create", _BuildCreateCipherItemLoginRequest(cipherItem, null)).ConfigureAwait(false);
                case CipherItemType.SecureNote: return await _CreateAPIAsync<CipherItemModel, CipherItemCreateRequestAPIModel<CipherItemSecureNoteRequestAPIModel>>($"{_baseUrl}{CiphersApiPath}/create", _BuildCreateCipherItemSecureNoteRequest(cipherItem, null)).ConfigureAwait(false);
                case CipherItemType.Card      : return await _CreateAPIAsync<CipherItemModel, CipherItemCreateRequestAPIModel<CipherItemCardRequestAPIModel>>($"{_baseUrl}{CiphersApiPath}/create", _BuildCreateCipherItemCardRequest(cipherItem, null)).ConfigureAwait(false);
                case CipherItemType.Identity  : return await _CreateAPIAsync<CipherItemModel, CipherItemCreateRequestAPIModel<CipherItemIdentityRequestAPIModel>>($"{_baseUrl}{CiphersApiPath}/create", _BuildCreateCipherItemIdentityRequest(cipherItem, null)).ConfigureAwait(false);
                case CipherItemType.SshKey    : return await _CreateAPIAsync<CipherItemModel, CipherItemCreateRequestAPIModel<CipherItemSSHKeyRequestAPIModel>>($"{_baseUrl}{CiphersApiPath}/create", _BuildCreateCipherItemSSHKeyRequest(cipherItem, null)).ConfigureAwait(false);
            }
        }
        else
        {
            return await CreateOrganizationCipherItemAsync(cipherItem, cipherItem.CollectionsIds).ConfigureAwait(false);
        }

        throw new InvalidDataException($"Unhandled cipher item type: {cipherItem.ItemType}");
    }

    public async Task<CipherItemModel> CreateOrganizationCipherItemAsync(CipherItemModel cipherItem, IEnumerable<Guid> collectionIds)
    {
        if (cipherItem.OrganizationId == null)
            throw new InvalidDataException($"{nameof(CipherItemModel.OrganizationId)} must be set.");

        if (collectionIds?.Any() != true)
            throw new InvalidDataException($"{nameof(collectionIds)} must have at least 1 collection id.");
            
        switch (cipherItem.ItemType)
        {
            case CipherItemType.Login     : return await _CreateAPIAsync<CipherItemModel, CipherItemCreateRequestAPIModel<CipherItemLoginRequestAPIModel>>($"{_baseUrl}{CiphersApiPath}/create", _BuildCreateCipherItemLoginRequest(cipherItem, collectionIds)).ConfigureAwait(false);
            case CipherItemType.SecureNote: return await _CreateAPIAsync<CipherItemModel, CipherItemCreateRequestAPIModel<CipherItemSecureNoteRequestAPIModel>>($"{_baseUrl}{CiphersApiPath}/create", _BuildCreateCipherItemSecureNoteRequest(cipherItem, collectionIds)).ConfigureAwait(false);
            case CipherItemType.Card      : return await _CreateAPIAsync<CipherItemModel, CipherItemCreateRequestAPIModel<CipherItemCardRequestAPIModel>>($"{_baseUrl}{CiphersApiPath}/create", _BuildCreateCipherItemCardRequest(cipherItem, collectionIds)).ConfigureAwait(false);
            case CipherItemType.Identity  : return await _CreateAPIAsync<CipherItemModel, CipherItemCreateRequestAPIModel<CipherItemIdentityRequestAPIModel>>($"{_baseUrl}{CiphersApiPath}/create", _BuildCreateCipherItemIdentityRequest(cipherItem, collectionIds)).ConfigureAwait(false);
            case CipherItemType.SshKey    : return await _CreateAPIAsync<CipherItemModel, CipherItemCreateRequestAPIModel<CipherItemSSHKeyRequestAPIModel>>($"{_baseUrl}{CiphersApiPath}/create", _BuildCreateCipherItemSSHKeyRequest(cipherItem, collectionIds)).ConfigureAwait(false);
        }
        
        throw new InvalidDataException($"Unhandled cipher item type: {cipherItem.ItemType}");
    }

    public async Task<CipherItemModel> UpdateCipherItemAsync(Guid id, CipherItemModel cipherItem)
    {
        switch (cipherItem.ItemType)
        {
            case CipherItemType.Login     : return await _UpdateAPIAsync<CipherItemModel, CipherItemLoginRequestAPIModel>($"{_baseUrl}{CiphersApiPath}", id, _BuildCipherItemLoginRequest(cipherItem)).ConfigureAwait(false);
            case CipherItemType.SecureNote: return await _UpdateAPIAsync<CipherItemModel, CipherItemSecureNoteRequestAPIModel>($"{_baseUrl}{CiphersApiPath}", id, _BuildCipherItemSecureNoteRequest(cipherItem)).ConfigureAwait(false);
            case CipherItemType.Card      : return await _UpdateAPIAsync<CipherItemModel, CipherItemCardRequestAPIModel>($"{_baseUrl}{CiphersApiPath}", id, _BuildCipherItemCardRequest(cipherItem)).ConfigureAwait(false);
            case CipherItemType.Identity  : return await _UpdateAPIAsync<CipherItemModel, CipherItemIdentityRequestAPIModel>($"{_baseUrl}{CiphersApiPath}", id, _BuildCipherItemIdentityRequest(cipherItem)).ConfigureAwait(false);
        }

        throw new InvalidDataException($"Unhandled cipher item type: {cipherItem.ItemType}");
    }

    public async Task<CipherItemModel> UpdateCipherItemCollectionsAsync(Guid id, List<Guid> newCollections)
    {
        if (!(newCollections?.Count > 0))
            throw new InvalidDataException($"New collections ids must not be empty");

        return await _UpdateItemCollectionsAPIAsync($"{_baseUrl}{CiphersApiPath}/{id}/collections_v2", newCollections).ConfigureAwait(false);
    }

    public async Task MoveToTrashCipherItemAsync(Guid id)
    {
        await _MoveTrashAPIAsync($"{_baseUrl}{CiphersApiPath}/{id}/delete").ConfigureAwait(false);
    }

    public async Task RestoreCipherItemAsync(Guid id)
    {
        await _RestoreAPIAsync($"{_baseUrl}{CiphersApiPath}/{id}/restore").ConfigureAwait(false);
    }

    public async Task DeleteCipherItemAsync(Guid id)
    {
        await _DeleteAPIAsync($"{_baseUrl}{CiphersApiPath}/{id}").ConfigureAwait(false);
    }

    public async Task DeleteCipherItemsAsync(IEnumerable<Guid> ids)
    {
        await _MultiDeleteAPIAsync(CiphersApiPath, ids).ConfigureAwait(false);
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
        return await _GetAPIAsync<FolderItemModel>($"{_baseUrl}{FoldersApiPath}/{id}").ConfigureAwait(false);
    }

    public async Task<List<FolderItemModel>> GetFoldersAsync()
    {
        return (await _GetAllAPIAsync<FolderItemModel>($"{_baseUrl}{FoldersApiPath}").ConfigureAwait(false)).Data;
    }

    public async Task<FolderItemModel> CreateFolderAsync(string encryptedName)
    {
        return await _CreateAPIAsync<FolderItemModel, FolderRequestAPIModel>($"{_baseUrl}{FoldersApiPath}", _BuildFolderRequestAPIModel(encryptedName)).ConfigureAwait(false);
    }

    public async Task<FolderItemModel> UpdateFolderAsync(Guid id, string encryptedName)
    {
        return await _UpdateAPIAsync<FolderItemModel, FolderRequestAPIModel>($"{_baseUrl}{FoldersApiPath}", id, _BuildFolderRequestAPIModel(encryptedName)).ConfigureAwait(false);
    }

    public async Task DeleteFolderAsync(Guid id)
    {
        await _DeleteAPIAsync($"{_baseUrl}{FoldersApiPath}/{id}").ConfigureAwait(false);
    }

    public async Task DeleteFoldersAsync(IEnumerable<Guid> ids)
    {
        await _MultiDeleteAPIAsync($"{_baseUrl}{FoldersApiPath}", ids).ConfigureAwait(false);
    }

    public async Task DeleteAllFolderAsync()
    {
        var response = await _httpClient.DeleteAsync($"{_baseUrl}{FoldersApiPath}/all").ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    #endregion

    #region Organization API

    const string OrganizationsApiPath = "/api/organizations";

    public async Task<CollectionItemModel> CreateCollectionAsync(Guid organizationId, string encryptedName, List<UserCollectionPermissionsModel> users, List<UserCollectionPermissionsModel> groups)
    {
        var apiModel = new CollectionCreateRequestAPIModel
        {
            Name = encryptedName,
            Users = users,
            Groups = groups
        };

        return await _CreateAPIAsync<CollectionItemModel, CollectionCreateRequestAPIModel>($"{_baseUrl}{OrganizationsApiPath}/{organizationId}/collections", apiModel).ConfigureAwait(false);
    }

    public async Task<List<CollectionItemModel>> GetCollectionsAsync(Guid organizationId)
    {
        return await _GetAPIAsync<List<CollectionItemModel>>($"{_baseUrl}{OrganizationsApiPath}/{organizationId}/collections/details").ConfigureAwait(false);
    }

    public async Task DeleteCollectionAsync(Guid organizationId, Guid collectionId)
    {
        await _DeleteAPIAsync($"{_baseUrl}{OrganizationsApiPath}/{organizationId}/collections/{collectionId}").ConfigureAwait(false);
    }

    #endregion

    #region Attachment API

    public async Task<Stream> GetAttachmentAsync(AttachmentModel attachment)
    {
        return await _GetAPIAsync(attachment.Url).ConfigureAwait(false);
    }

    #endregion

    public async Task<ProfileItemModel> GetAccountProfileAsync()
    {
        return await _GetAPIAsync<ProfileItemModel>($"{_baseUrl}/api/accounts/profile").ConfigureAwait(false);
    }

    public async Task<DatabaseModel> GetDatabaseAsync(bool excludeDomains = true)
    {
        var strBool = excludeDomains ? "true" : "false"; // Because bool.ToString() is CamelCase
        return await _GetAPIAsync<DatabaseModel>($"{_baseUrl}/api/sync?excludeDomains={strBool}").ConfigureAwait(false);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}

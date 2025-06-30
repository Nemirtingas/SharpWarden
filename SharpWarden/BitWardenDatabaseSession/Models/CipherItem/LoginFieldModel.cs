// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Services;

namespace SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

public class LoginFieldModel : ISessionAware
{
    private IUserCryptoService _CryptoService;

    public LoginFieldModel()
    {
    }

    public LoginFieldModel(IUserCryptoService cryptoService)
    {
        SetCryptoService(cryptoService);
    }

    public bool HasSession() => _CryptoService != null;

    public void SetCryptoService(IUserCryptoService cryptoService)
    {
        _CryptoService = cryptoService;

        Password?.SetCryptoService(_CryptoService);
        Uri?.SetCryptoService(_CryptoService);
        TOTP?.SetCryptoService(_CryptoService);
        Username?.SetCryptoService(_CryptoService);

        if (Uris != null)
            foreach (var uri in Uris)
                uri.SetCryptoService(_CryptoService);
    }

    [JsonProperty("autofillOnPageLoad")]
    public bool? AutoFillOnPageLoad { get; set; }

    [JsonProperty("password")]
    public EncryptedString Password { get; set; }

    [JsonProperty("passwordRevisionDate")]
    public DateTime? PasswordRevisionDate { get; set; }

    [JsonProperty("totp")]
    public EncryptedString TOTP { get; set; }

    [JsonProperty("uri")]
    public EncryptedString Uri { get; set; }

    [JsonProperty("uris")]
    public List<UriModel> Uris { get; set; } = new();

    [JsonProperty("username")]
    public EncryptedString Username { get; set; }
}
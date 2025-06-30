// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Services;

namespace SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

public class PasswordHistoryModel : ISessionAware
{
    private IUserCryptoService _CryptoService;

    public PasswordHistoryModel()
    {
    }

    public PasswordHistoryModel(IUserCryptoService cryptoService)
    {
        SetCryptoService(cryptoService);
    }

    public bool HasSession() => _CryptoService != null;

    public void SetCryptoService(IUserCryptoService cryptoService)
    {
        _CryptoService = cryptoService;

        Password?.SetCryptoService(_CryptoService);
    }

    [JsonProperty("lastUsedDate")]
    public DateTime? LastUsedDate { get; set; }

    [JsonProperty("password")]
    public EncryptedString Password { get; set; }
}
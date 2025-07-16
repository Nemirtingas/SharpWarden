// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Services;

namespace SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

public class PasswordHistoryModel : ISessionAware
{
    private IUserCryptoService _cryptoService;

    public PasswordHistoryModel()
    {
    }

    public PasswordHistoryModel(IUserCryptoService cryptoService)
    {
        SetCryptoService(cryptoService);
    }

    public bool HasSession() => _cryptoService != null;

    public void SetCryptoService(IUserCryptoService cryptoService)
    {
        _cryptoService = cryptoService;

        Password?.SetCryptoService(_cryptoService);
    }

    [JsonProperty("lastUsedDate")]
    public DateTime? LastUsedDate { get; set; }

    [JsonProperty("password")]
    public EncryptedString Password { get; set; }
}
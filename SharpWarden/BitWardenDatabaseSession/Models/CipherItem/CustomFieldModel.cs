// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Services;

namespace SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

public class CustomFieldModel : ISessionAware
{
    private IUserCryptoService _cryptoService;

    public CustomFieldModel()
    {
    }

    public CustomFieldModel(IUserCryptoService cryptoService)
    {
        SetCryptoService(cryptoService);
    }

    public bool HasSession() => _cryptoService != null;

    public void SetCryptoService(IUserCryptoService cryptoService)
    {
        _cryptoService = cryptoService;

        Name?.SetCryptoService(_cryptoService);
        Value?.SetCryptoService(_cryptoService);
    }

    [JsonProperty("linkedId")]
    public CustomFieldLinkType? LinkedId { get; set; }

    [JsonProperty("name")]
    public EncryptedString Name { get; set; }

    [JsonProperty("type")]
    public CustomFieldType Type { get; set; }

    [JsonProperty("value")]
    public EncryptedString Value { get; set; }
}
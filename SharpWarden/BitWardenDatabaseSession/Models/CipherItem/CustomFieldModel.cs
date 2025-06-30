// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Services;

namespace SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

public class CustomFieldModel
{
    private IUserCryptoService _CryptoService;

    public CustomFieldModel()
    {
    }

    public CustomFieldModel(IUserCryptoService cryptoService)
    {
        SetCryptoService(cryptoService);
    }

    public bool HasSession() => _CryptoService != null;

    public void SetCryptoService(IUserCryptoService cryptoService)
    {
        _CryptoService = cryptoService;

        Name?.SetCryptoService(_CryptoService);
        Value?.SetCryptoService(_CryptoService);
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
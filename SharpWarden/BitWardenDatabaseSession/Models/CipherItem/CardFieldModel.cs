// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Services;

namespace SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

public class CardFieldModel : ISessionAware
{
    private IUserCryptoService _CryptoService;

    public CardFieldModel()
    {
    }

    public CardFieldModel(IUserCryptoService cryptoService)
    {
        SetCryptoService(cryptoService);
    }

    public bool HasSession() => _CryptoService != null;

    public void SetCryptoService(IUserCryptoService cryptoService)
    {
        _CryptoService = cryptoService;

        CardholderName?.SetCryptoService(_CryptoService);
        Brand?.SetCryptoService(_CryptoService);
        Number?.SetCryptoService(_CryptoService);
        ExpMonth?.SetCryptoService(_CryptoService);
        ExpYear?.SetCryptoService(_CryptoService);
        Code?.SetCryptoService(_CryptoService);
    }

    [JsonProperty("cardholderName")]
    public EncryptedString CardholderName { get; set; }

    [JsonProperty("brand")]
    public EncryptedString Brand { get; set; }

    [JsonProperty("number")]
    public EncryptedString Number { get; set; }

    [JsonProperty("expMonth")]
    public EncryptedString ExpMonth { get; set; }

    [JsonProperty("expYear")]
    public EncryptedString ExpYear { get; set; }

    [JsonProperty("code")]
    public EncryptedString Code { get; set; }
}
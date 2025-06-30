// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Services;

namespace SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

public class SSHKeyFieldModel : ISessionAware
{
    private IUserCryptoService _CryptoService;

    public SSHKeyFieldModel()
    {
    }

    public SSHKeyFieldModel(IUserCryptoService cryptoService)
    {
        SetCryptoService(cryptoService);
    }

    public bool HasSession() => _CryptoService != null;

    public void SetCryptoService(IUserCryptoService cryptoService)
    {
        _CryptoService = cryptoService;

        KeyFingerprint?.SetCryptoService(_CryptoService);
        PrivateKey?.SetCryptoService(_CryptoService);
        PublicKey?.SetCryptoService(_CryptoService);
    }

    [JsonProperty("keyFingerprint")]
    public EncryptedString KeyFingerprint { get; set; }

    [JsonProperty("privateKey")]
    public EncryptedString PrivateKey { get; set; }

    [JsonProperty("publicKey")]
    public EncryptedString PublicKey { get; set; }
}
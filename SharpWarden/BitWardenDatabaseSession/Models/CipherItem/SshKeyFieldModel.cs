// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Services;

namespace SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

public class SshKeyFieldModel : ISessionAware
{
    private IUserCryptoService _cryptoService;

    public SshKeyFieldModel()
    {
    }

    public SshKeyFieldModel(IUserCryptoService cryptoService)
    {
        SetCryptoService(cryptoService);
    }

    public bool HasSession() => _cryptoService != null;

    public void SetCryptoService(IUserCryptoService cryptoService)
    {
        _cryptoService = cryptoService;

        KeyFingerprint?.SetCryptoService(_cryptoService);
        PrivateKey?.SetCryptoService(_cryptoService);
        PublicKey?.SetCryptoService(_cryptoService);
    }

    [JsonProperty("keyFingerprint")]
    public EncryptedString KeyFingerprint { get; set; }

    [JsonProperty("privateKey")]
    public EncryptedString PrivateKey { get; set; }

    [JsonProperty("publicKey")]
    public EncryptedString PublicKey { get; set; }
}
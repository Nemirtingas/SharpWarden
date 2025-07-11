// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;

namespace SharpWarden.BitWardenWebSession.Models;

public class LoginModel
{
    [JsonProperty("ForcePasswordReset")]
    public bool ForcePasswordReset { get; set; }

    [JsonProperty("Kdf")]
    public KdfType Kdf { get; set; }

    [JsonProperty("KdfIterations")]
    public int KdfIterations { get; set; }

    [JsonProperty("KdfMemory")]
    public int? KdfMemory { get; set; }

    [JsonProperty("KdfParallelism")]
    public int? KdfParallelism { get; set; }

    [JsonProperty("Key")]
    public string Key { get; set; }

    //"MasterPasswordPolicy": {
    //  "enforceOnLogin": false,
    //  "minComplexity": 4,
    //  "minLength": 12,
    //  "object": "masterPasswordPolicy",
    //  "requireLower": true,
    //  "requireNumbers": true,
    //  "requireSpecial": true,
    //  "requireUpper": true
    //},

    [JsonProperty("PrivateKey")]
    public string PrivateKey { get; set; }

    [JsonProperty("ResetMasterPassword")]
    public bool ResetMasterPassword { get; set; }

    //"UserDecryptionOptions": {
    //    "HasMasterPassword": true,
    //    "Object": "userDecryptionOptions"
    //},

    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; }

    [JsonProperty("scope")]
    public string Scope { get; set; }

    [JsonProperty("token_type")]
    public string TokenType { get; set; }

    public LoginModel Clone()
        => new LoginModel
        {
            ForcePasswordReset = ForcePasswordReset,
            KdfIterations = KdfIterations,
            KdfMemory = KdfMemory,
            KdfParallelism = KdfParallelism,
            Key = Key,
            PrivateKey = PrivateKey,
            ResetMasterPassword = ResetMasterPassword,
            AccessToken = AccessToken,
            ExpiresIn = ExpiresIn,
            RefreshToken = RefreshToken,
            Scope = Scope,
            TokenType = TokenType,
        };

    public void UpdateSession(RefreshModel refreshModel)
    {
        AccessToken = refreshModel.AccessToken;
        ExpiresIn = refreshModel.ExpiresIn;
        RefreshToken = refreshModel.RefreshToken;
        Scope = refreshModel.Scope;
        TokenType = refreshModel.TokenType;
    }

    public void UpdateSession(ApiKeyLoginModel apiKeyLoginModel)
    {
        AccessToken = apiKeyLoginModel.AccessToken;
        ExpiresIn = apiKeyLoginModel.ExpiresIn;
        Kdf = apiKeyLoginModel.Kdf;
        KdfIterations = apiKeyLoginModel.KdfIterations;
        KdfMemory = apiKeyLoginModel.KdfMemory;
        KdfParallelism = apiKeyLoginModel.KdfParallelism;
        Key = apiKeyLoginModel.Key;
        PrivateKey = apiKeyLoginModel.PrivateKey;
        ForcePasswordReset = apiKeyLoginModel.ForcePasswordReset;
        ResetMasterPassword = apiKeyLoginModel.ResetMasterPassword;
        Scope = apiKeyLoginModel.Scope;
        TokenType = apiKeyLoginModel.TokenType;
    }
}
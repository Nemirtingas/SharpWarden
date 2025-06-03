using Newtonsoft.Json;

namespace BitwardenSharp.Models;

class BitWardenWebSessionLoginModel
{
    [JsonProperty("ForcePasswordReset")]
    public bool ForcePasswordReset { get; set; }

    [JsonProperty("Kdf")]
    public int Kdf { get; set; }

    [JsonProperty("KdfIterations")]
    public int KdfIterations { get; set; }

    [JsonProperty("KdfMemory")]
    public object KdfMemory { get; set; }

    [JsonProperty("KdfParallelism")]
    public object KdfParallelism { get; set; }

    [JsonProperty("Key")]
    public BitWardenEncryptedString Key { get; set; }

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
    public BitWardenEncryptedString PrivateKey { get; set; }

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
}
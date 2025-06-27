using Newtonsoft.Json;

namespace SharpWarden.BitWardenWebSession.Models;

public class ApiKeyLoginModel
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonProperty("token_type")]
    public string TokenType { get; set; }

    [JsonProperty("scope")]
    public string Scope { get; set; }

    [JsonProperty("PrivateKey")]
    public string PrivateKey { get; set; }

    [JsonProperty("Key")]
    public string Key { get; set; }

    //[JsonProperty("MasterPasswordPolicy")]
    //public object MasterPasswordPolicy { get; set; }

    [JsonProperty("ForcePasswordReset")]
    public bool ForcePasswordReset { get; set; }

    [JsonProperty("ResetMasterPassword")]
    public bool ResetMasterPassword { get; set; }

    [JsonProperty("Kdf")]
    public KdfType Kdf { get; set; }

    [JsonProperty("KdfIterations")]
    public int KdfIterations { get; set; }

    [JsonProperty("KdfMemory")]
    public int? KdfMemory { get; set; }

    [JsonProperty("KdfParallelism")]
    public int? KdfParallelism { get; set; }

    //[JsonProperty("UserDecryptionOptions")]
    //public object UserDecryptionOptions { get; set; }
}
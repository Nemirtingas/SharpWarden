using Newtonsoft.Json;

namespace SharpWarden.BitWardenWebSession.Models;

class PreLoginModel
{
    [JsonProperty("kdf")]
    public KdfType Kdf { get; set; }

    [JsonProperty("kdfIterations")]
    public int KdfIterations { get; set; }

    [JsonProperty("kdfMemory")]
    public int? KdfMemory { get; set; }
    
    [JsonProperty("kdfParallelism")]
    public int? KdfParallelism { get; set; }
}
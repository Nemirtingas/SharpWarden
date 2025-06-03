using Newtonsoft.Json;

namespace BitwardenSharp.Models;

class BitWardenWebSessionPreLoginModel
{
    [JsonProperty("kdf")]
    public int Kdf { get; set; }

    [JsonProperty("kdfIterations")]
    public int KdfIterations { get; set; }

    //[JsonProperty("kdfMemory")]
    //public object kdfMemory { get; set; }
    //
    //[JsonProperty("kdfParallelism")]
    //public object kdfParallelism { get; set; }
}
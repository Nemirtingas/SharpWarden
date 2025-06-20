using Newtonsoft.Json;

namespace SharpWarden.WebClient.Models;

public class CipherItemCreateAttachmentRequestAPIModel
{
    [JsonProperty("adminRequest")]
    public bool AdminRequest { get; set; }
        
    [JsonProperty("fileSize")]
    public long FileSize { get; set; }

    [JsonProperty("fileName")]
    public string FileName { get; set; }

    [JsonProperty("key")]
    public string Key { get; set; }
}
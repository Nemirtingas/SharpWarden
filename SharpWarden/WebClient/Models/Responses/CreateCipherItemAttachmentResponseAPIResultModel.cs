using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

namespace SharpWarden.WebClient.Models;

public class CreateCipherItemAttachmentResponseAPIResultModel : ApiResultModel
{
    [JsonProperty("attachmentId")]
    public string AttachmentId { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("fileUploadType")]
    public FileUploadType FileUploadType { get; set; }

    [JsonProperty("cipherResponse")]
    public CipherItemModel CipherResponse { get; set; }
}
using Newtonsoft.Json;

namespace BitwardenSharp.Models;

public class BitWardenItemLoginFieldModel
{
    [JsonProperty("autofillOnPageLoad")]
    public bool? AutoFillOnPageLoad { get; set; }

    [JsonProperty("password")]
    public BitWardenEncryptedString Password { get; set; }

    [JsonProperty("passwordRevisionDate")]
    public object PasswordRevisionDate { get; set; }

    [JsonProperty("totp")]
    public object TOTP { get; set; }

    [JsonProperty("uri")]
    public object Uri { get; set; }

    [JsonProperty("uris")]
    public object Uris { get; set; }

    [JsonProperty("username")]
    public BitWardenEncryptedString Username { get; set; }
}
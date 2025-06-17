using Newtonsoft.Json;

namespace SharpWarden.BitWardenDatabase.CipherItem.Models;

public class LoginFieldModel
{
    [JsonProperty("autofillOnPageLoad")]
    public bool? AutoFillOnPageLoad { get; set; }

    [JsonProperty("password")]
    public string Password { get; set; }

    [JsonProperty("passwordRevisionDate")]
    public object PasswordRevisionDate { get; set; }

    [JsonProperty("totp")]
    public object TOTP { get; set; }

    [JsonProperty("uri")]
    public string Uri { get; set; }

    [JsonProperty("uris")]
    public List<UrlModel> Uris { get; set; } = new();

    [JsonProperty("username")]
    public string Username { get; set; }
}
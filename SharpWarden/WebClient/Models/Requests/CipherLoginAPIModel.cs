using Newtonsoft.Json;

namespace SharpWarden.WebClient.Models;

public class CipherLoginAPIModel
{
    [JsonProperty("uris")]
    public List<CipherLoginUriAPIModel> Uris { get; set; }

    [JsonProperty("username")]
    public string Username { get; set; }
        
    [JsonProperty("password")]
    public string Password { get; set; }

    [JsonProperty("passwordRevisionDate")]
    public DateTime? PasswordRevisionDate { get; set; }

    [JsonProperty("totp")]
    public string TOTP { get; set; }

    [JsonProperty("autofillOnPageLoad")]
    public bool? AutofillOnPageLoad { get; set; }
}
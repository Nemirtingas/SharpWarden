using Newtonsoft.Json;

namespace SharpWarden.WebClient.Models;

public class CipherCardAPIModel
{
    [JsonProperty("cardholderName")]
    public string CardholderName { get; set; }

    [JsonProperty("brand")]
    public string Brand { get; set; }

    [JsonProperty("number")]
    public string Number { get; set; }

    [JsonProperty("expMonth")]
    public string ExpMonth { get; set; }

    [JsonProperty("expYear")]
    public string ExpYear { get; set; }

    [JsonProperty("code")]
    public string Code { get; set; }
}
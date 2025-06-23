using Newtonsoft.Json;

namespace SharpWarden.WebClient.Models;

public class ErrorResponseModel
{
    [JsonProperty("error")]
    public ErrorType? ErrorType { get; set; }

    [JsonProperty("error_description")]
    public string Description { get; set; }
}

public class ErrorResponseModel<T> : ErrorResponseModel
{
    [JsonProperty("ErrorModel")]
    public T ErrorModel { get; set; }
}
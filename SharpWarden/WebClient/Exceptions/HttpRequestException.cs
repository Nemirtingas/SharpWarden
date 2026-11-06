// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SharpWarden.WebClient.Exceptions;

/// <summary>
/// Error returned by the Bitwarden server, with the message it sent.
/// </summary>
public class BitWardenHttpRequestException : HttpRequestException
{
    /// <summary>
    /// Error code sent by the identity server (e.g. <see cref="ErrorType.InvalidGrant"/> for a wrong password).
    /// </summary>
    public ErrorType ErrorType { get; }

    /// <summary>
    /// Raw response body (truncated), for diagnostics.
    /// </summary>
    public string ResponseContent { get; }

    public BitWardenHttpRequestException(ErrorType errorType, string message) :
        this(errorType, message, null, null)
    {
    }

    public BitWardenHttpRequestException(ErrorType errorType, string message, HttpStatusCode? statusCode, string responseContent) :
        base(message, null, statusCode)
    {
        ErrorType = errorType;
        ResponseContent = responseContent;
    }

    private const int MaxResponseContentLength = 4096;

    /// <summary>
    /// Builds the exception from a failed response, keeping the error message sent by the server.
    /// </summary>
    public static async Task<BitWardenHttpRequestException> FromResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);

        string body = null;
        try
        {
            body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException)
        {
            // Keep the status code even if the body cannot be read.
        }

        var (errorType, serverMessage) = ParseErrorBody(body);

        var message = $"{(int)response.StatusCode} {response.ReasonPhrase}";
        if (!string.IsNullOrWhiteSpace(serverMessage))
            message += $": {serverMessage}";

        if (body?.Length > MaxResponseContentLength)
            body = body[..MaxResponseContentLength];

        return new BitWardenHttpRequestException(errorType, message, response.StatusCode, body);
    }

    /// <summary>
    /// Extracts the error code and message from the error formats used by Bitwarden and Vaultwarden:
    /// identity errors (<c>error</c>/<c>error_description</c>), API errors (<c>message</c>, <c>validationErrors</c>)
    /// and Vaultwarden's <c>ErrorModel.Message</c>.
    /// </summary>
    public static (ErrorType ErrorType, string Message) ParseErrorBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return (ErrorType.Unknown, null);

        JObject json;
        try
        {
            json = JObject.Parse(body);
        }
        catch (JsonException)
        {
            // Not JSON (e.g. an HTML error page from a proxy).
            return (ErrorType.Unknown, null);
        }

        var errorType = ErrorType.Unknown;
        var error = json.Value<string>("error");
        if (!string.IsNullOrEmpty(error))
        {
            foreach (var value in Enum.GetValues<ErrorType>())
            {
                var member = typeof(ErrorType).GetField(value.ToString())?.GetCustomAttribute<EnumMemberAttribute>();
                if (string.Equals(member?.Value, error, StringComparison.OrdinalIgnoreCase))
                    errorType = value;
            }
        }

        string NonEmpty(JToken token) => token?.Type == JTokenType.String && !string.IsNullOrWhiteSpace((string)token) ? (string)token : null;

        var message =
            NonEmpty(json["message"]) ??
            NonEmpty(json["Message"]) ??
            NonEmpty(json["ErrorModel"]?["Message"]) ??
            NonEmpty(json["error_description"]) ??
            (json["validationErrors"] as JObject)?.Properties()
                .SelectMany(p => p.Value is JArray array ? array.Select(NonEmpty) : [NonEmpty(p.Value)])
                .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s)) ??
            error;

        return (errorType, message);
    }
}

using System.Net;
using SharpWarden.WebClient;
using SharpWarden.WebClient.Exceptions;

namespace SharpWarden.Tests;

/// <summary>
/// Offline tests of the server error parsing.
/// </summary>
[TestClass]
public sealed class HttpErrorTest
{
    [TestMethod]
    public void ParsesIdentityError()
    {
        var (type, message) = BitWardenHttpRequestException.ParseErrorBody(
            "{\"error\":\"invalid_grant\",\"error_description\":\"invalid_username_or_password\",\"ErrorModel\":{\"Message\":\"Username or password is incorrect. Try again.\",\"Object\":\"error\"}}");

        Assert.AreEqual(ErrorType.InvalidGrant, type);
        Assert.AreEqual("Username or password is incorrect. Try again.", message);
    }

    [TestMethod]
    public void ParsesDeviceError()
    {
        var (type, _) = BitWardenHttpRequestException.ParseErrorBody("{\"error\":\"device_error\",\"error_description\":\"New device verification required.\"}");

        Assert.AreEqual(ErrorType.DeviceError, type);
    }

    [TestMethod]
    public void ParsesVaultwardenApiError()
    {
        // Vaultwarden error format (e.g. the 422 returned when encryptedFor is missing).
        var (type, message) = BitWardenHttpRequestException.ParseErrorBody(
            "{\"message\":\"Invalid user cipher\",\"error\":\"\",\"error_description\":\"\",\"validationErrors\":{\"\":[\"Invalid user cipher\"]},\"ErrorModel\":{\"Message\":\"Invalid user cipher\",\"Object\":\"error\"},\"object\":\"error\"}");

        Assert.AreEqual(ErrorType.Unknown, type);
        Assert.AreEqual("Invalid user cipher", message);
    }

    [TestMethod]
    public void ParsesValidationErrors()
    {
        var (_, message) = BitWardenHttpRequestException.ParseErrorBody("{\"message\":null,\"validationErrors\":{\"Name\":[\"The Name field is required.\"]}}");

        Assert.AreEqual("The Name field is required.", message);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("<html><body>502 Bad Gateway</body></html>")]
    [DataRow("[1,2,3]")]
    public void IgnoresNonJsonOrUnexpectedBody(string body)
    {
        var (type, message) = BitWardenHttpRequestException.ParseErrorBody(body);

        Assert.AreEqual(ErrorType.Unknown, type);
        Assert.IsNull(message);
    }

    [TestMethod]
    public async Task ExceptionKeepsStatusCodeMessageAndBody()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
        {
            ReasonPhrase = "Unprocessable Entity",
            Content = new StringContent("{\"message\":\"Invalid user cipher\"}"),
        };

        var exception = await BitWardenHttpRequestException.FromResponseAsync(response);

        Assert.AreEqual(HttpStatusCode.UnprocessableEntity, exception.StatusCode);
        Assert.AreEqual("422 Unprocessable Entity: Invalid user cipher", exception.Message);
        Assert.AreEqual("{\"message\":\"Invalid user cipher\"}", exception.ResponseContent);
    }
}

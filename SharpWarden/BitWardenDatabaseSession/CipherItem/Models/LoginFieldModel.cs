using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Models;

namespace SharpWarden.BitWardenDatabaseSession.CipherItem.Models;

public class LoginFieldModel : IDatabaseSessionModel
{
    private DatabaseSession _DatabaseSession;
    private Guid? _OrganizationId;

    public LoginFieldModel(DatabaseSession databaseSession)
    {
        SetDatabaseSession(databaseSession);
    }

    public bool HasSession() => _DatabaseSession != null;

    public void SetDatabaseSession(DatabaseSession databaseSession)
    {
        _DatabaseSession = databaseSession;

        Password = new EncryptedString(Password.CipherString, databaseSession);
        Uri = new EncryptedString(Uri.CipherString, databaseSession);

        if (Uris != null)
            foreach (var uri in Uris)
                uri.SetDatabaseSession(_DatabaseSession);

        Username = new EncryptedString(Username.CipherString, databaseSession);
    }

    public void SetDatabaseSession(DatabaseSession databaseSession, Guid? organizationId)
    {
        _DatabaseSession = databaseSession;
        _OrganizationId = organizationId;

        Password = new EncryptedString(Password.CipherString, _DatabaseSession, _OrganizationId);
        Uri = new EncryptedString(Uri.CipherString, _DatabaseSession, _OrganizationId);

        if (Uris != null)
            foreach (var uri in Uris)
                uri.SetDatabaseSession(_DatabaseSession, _OrganizationId);

        Username = new EncryptedString(Username.CipherString, _DatabaseSession, _OrganizationId);
    }

    [JsonProperty("autofillOnPageLoad")]
    public bool? AutoFillOnPageLoad { get; set; }

    [JsonProperty("password")]
    public EncryptedString Password { get; set; }

    [JsonProperty("passwordRevisionDate")]
    public object PasswordRevisionDate { get; set; }

    [JsonProperty("totp")]
    public object TOTP { get; set; }

    [JsonProperty("uri")]
    public EncryptedString Uri { get; set; }

    [JsonProperty("uris")]
    public List<UrlModel> Uris { get; set; } = new();

    [JsonProperty("username")]
    public EncryptedString Username { get; set; }
}
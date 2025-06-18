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

        Password?.SetDatabaseSession(_DatabaseSession);
        Uri?.SetDatabaseSession(_DatabaseSession);
        TOTP?.SetDatabaseSession(_DatabaseSession);
        Username?.SetDatabaseSession(_DatabaseSession);

        if (Uris != null)
            foreach (var uri in Uris)
                uri.SetDatabaseSession(_DatabaseSession);
    }

    public void SetDatabaseSession(DatabaseSession databaseSession, Guid? organizationId)
    {
        _DatabaseSession = databaseSession;
        _OrganizationId = organizationId;

        Password?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
        Uri?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
        TOTP?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
        Username?.SetDatabaseSession(_DatabaseSession, _OrganizationId);

        if (Uris != null)
            foreach (var uri in Uris)
                uri.SetDatabaseSession(_DatabaseSession, _OrganizationId);
    }

    [JsonProperty("autofillOnPageLoad")]
    public bool? AutoFillOnPageLoad { get; set; }

    [JsonProperty("password")]
    public EncryptedString Password { get; set; }

    [JsonProperty("passwordRevisionDate")]
    public DateTime? PasswordRevisionDate { get; set; }

    [JsonProperty("totp")]
    public EncryptedString TOTP { get; set; }

    [JsonProperty("uri")]
    public EncryptedString Uri { get; set; }

    [JsonProperty("uris")]
    public List<UriModel> Uris { get; set; } = new();

    [JsonProperty("username")]
    public EncryptedString Username { get; set; }
}
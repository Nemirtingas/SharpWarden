using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Models;

namespace SharpWarden.BitWardenDatabaseSession.CipherItem.Models;

public class PasswordHistoryModel : IDatabaseSessionModel
{
    private DatabaseSession _DatabaseSession;
    private Guid? _OrganizationId;

    public PasswordHistoryModel(DatabaseSession databaseSession)
    {
        SetDatabaseSession(databaseSession);
    }

    public bool HasSession() => _DatabaseSession != null;

    public void SetDatabaseSession(DatabaseSession databaseSession)
    {
        _DatabaseSession = databaseSession;

        Password?.SetDatabaseSession(_DatabaseSession);
    }

    public void SetDatabaseSession(DatabaseSession databaseSession, Guid? organizationId)
    {
        _DatabaseSession = databaseSession;
        _OrganizationId = organizationId;

        Password?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
    }

    [JsonProperty("lastUsedDate")]
    public DateTime? LastUsedDate { get; set; }

    [JsonProperty("password")]
    public EncryptedString Password { get; set; }
}
using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Models;

namespace SharpWarden.BitWardenDatabaseSession.CipherItem.Models;

public class UriModel : IDatabaseSessionModel
{
    private DatabaseSession _DatabaseSession;
    private Guid? _OrganizationId;

    public UriModel(DatabaseSession databaseSession)
    {
        SetDatabaseSession(databaseSession);
    }

    public bool HasSession() => _DatabaseSession != null;

    public void SetDatabaseSession(DatabaseSession databaseSession)
    {
        _DatabaseSession = databaseSession;

        Uri?.SetDatabaseSession(_DatabaseSession);
        UriChecksum?.SetDatabaseSession(_DatabaseSession);
    }

    public void SetDatabaseSession(DatabaseSession databaseSession, Guid? organizationId)
    {
        _DatabaseSession = databaseSession;
        _OrganizationId = organizationId;

        Uri?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
        UriChecksum?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
    }

    [JsonProperty("match")]
    public UriMatchType? Match { get; set; }

    [JsonProperty("uri")]
    public EncryptedString Uri { get; set; }

    [JsonProperty("uriChecksum")]
    public EncryptedString UriChecksum { get; set; }
}
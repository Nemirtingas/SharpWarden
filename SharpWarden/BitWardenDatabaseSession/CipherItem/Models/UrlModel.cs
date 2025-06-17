using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Models;

namespace SharpWarden.BitWardenDatabaseSession.CipherItem.Models;

public class UrlModel : IDatabaseSessionModel
{
    private DatabaseSession _DatabaseSession;
    private Guid? _OrganizationId;

    public UrlModel(DatabaseSession databaseSession)
    {
        SetDatabaseSession(databaseSession);
    }

    public bool HasSession() => _DatabaseSession != null;

    public void SetDatabaseSession(DatabaseSession databaseSession)
    {
        _DatabaseSession = databaseSession;

        Uri = new EncryptedString(Uri.CipherString, databaseSession);
        UriChecksum = new EncryptedString(UriChecksum.CipherString, databaseSession);
    }

    public void SetDatabaseSession(DatabaseSession databaseSession, Guid? organizationId)
    {
        _DatabaseSession = databaseSession;
        _OrganizationId = organizationId;

        Uri = new EncryptedString(Uri.CipherString, databaseSession);
        UriChecksum = new EncryptedString(UriChecksum.CipherString, databaseSession);
    }

    [JsonProperty("match")]
    public UrlMatchType? Match { get; set; }

    [JsonProperty("uri")]
    public EncryptedString Uri { get; set; }

    [JsonProperty("uriChecksum")]
    public EncryptedString UriChecksum { get; set; }
}
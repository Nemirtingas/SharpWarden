using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Models;

namespace SharpWarden.BitWardenDatabaseSession.CipherItem.Models;

public class CustomFieldModel : IDatabaseSessionModel
{
    private DatabaseSession _DatabaseSession;
    private Guid? _OrganizationId;

    public CustomFieldModel(DatabaseSession databaseSession)
    {
        SetDatabaseSession(databaseSession);
    }

    public bool HasSession() => _DatabaseSession != null;

    public void SetDatabaseSession(DatabaseSession databaseSession)
    {
        _DatabaseSession = databaseSession;

        Name?.SetDatabaseSession(_DatabaseSession);
        Value?.SetDatabaseSession(_DatabaseSession);
    }

    public void SetDatabaseSession(DatabaseSession databaseSession, Guid? organizationId)
    {
        _DatabaseSession = databaseSession;
        _OrganizationId = organizationId;

        Name?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
        Value?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
    }

    [JsonProperty("linkedId")]
    public CustomFieldLinkType? LinkedId { get; set; }

    [JsonProperty("name")]
    public EncryptedString Name { get; set; }

    [JsonProperty("type")]
    public CustomFieldType Type { get; set; }

    [JsonProperty("value")]
    public EncryptedString Value { get; set; }
}
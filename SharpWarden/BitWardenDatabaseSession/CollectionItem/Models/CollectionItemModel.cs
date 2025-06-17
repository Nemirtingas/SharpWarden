using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Models;

namespace SharpWarden.BitWardenDatabaseSession.CollectionItem.Models;

public class CollectionItemModel : IDatabaseSessionModel
{
    private DatabaseSession _DatabaseSession;

    public CollectionItemModel(DatabaseSession databaseSession)
    {
        SetDatabaseSession(databaseSession);
    }

    public bool HasSession() => _DatabaseSession != null;

    public void SetDatabaseSession(DatabaseSession databaseSession)
    {
        _DatabaseSession = databaseSession;

        Name = new EncryptedString(Name.CipherString, databaseSession);
    }

    public void SetDatabaseSession(DatabaseSession databaseSession, Guid? organizationId)
        => SetDatabaseSession(databaseSession);

    [JsonProperty("externalId")]
    public Guid? ExternalId { get; set; }

    [JsonProperty("hidePasswords")]
    public bool HidePasswords { get; set; }

    [JsonProperty("id")]
    public Guid? Id { get; set; }

    [JsonProperty("manage")]
    public bool Manage { get; set; }

    [JsonProperty("name")]
    public EncryptedString Name { get; set; }

    [JsonProperty("object")]
    public ObjectType ObjectType { get; set; } = ObjectType.CollectionDetails;

    [JsonProperty("organizationId")]
    public Guid? OrganizationId { get; set; }

    [JsonProperty("readOnly")]
    public bool ReadOnly { get; set; }
}
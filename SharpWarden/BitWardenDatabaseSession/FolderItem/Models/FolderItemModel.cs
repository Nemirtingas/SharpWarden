using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Models;

namespace SharpWarden.BitWardenDatabaseSession.FolderItem.Models;

public class FolderItemModel : IDatabaseSessionModel
{
    private DatabaseSession _DatabaseSession;

    public FolderItemModel(DatabaseSession databaseSession)
    {
        SetDatabaseSession(databaseSession);
    }

    public bool HasSession() => _DatabaseSession != null;

    public void SetDatabaseSession(DatabaseSession databaseSession)
    {
        _DatabaseSession = databaseSession;

        Name?.SetDatabaseSession(_DatabaseSession);
    }

    public void SetDatabaseSession(DatabaseSession databaseSession, Guid? organizationId)
        => SetDatabaseSession(databaseSession);

    [JsonProperty("id")]
    public Guid? Id { get; set; }

    [JsonProperty("name")]
    public EncryptedString Name { get; set; }

    [JsonProperty("object")]
    public ObjectType ObjectType { get; set; } = ObjectType.Folder;

    [JsonProperty("revisionDate")]
    public DateTime? RevisionDate { get; set; }
}
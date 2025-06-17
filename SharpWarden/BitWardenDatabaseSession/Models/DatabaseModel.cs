using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.CipherItem.Models;
using SharpWarden.BitWardenDatabaseSession.CollectionItem.Models;
using SharpWarden.BitWardenDatabaseSession.FolderItem.Models;
using SharpWarden.BitWardenDatabaseSession.ProfileItem.Models;

namespace SharpWarden.BitWardenDatabaseSession.Models;

public class DatabaseModel : IDatabaseSessionModel
{
    private DatabaseSession _DatabaseSession;

    public DatabaseModel(DatabaseSession databaseSession)
    {
        _DatabaseSession = databaseSession;
        Items = new();
        Folders = new();
        Collections = new();
        Profile = new(databaseSession);
    }

    public bool HasSession() => _DatabaseSession != null;

    public void SetDatabaseSession(DatabaseSession databaseSession)
    {
        _DatabaseSession = databaseSession;

        foreach (var item in Items)
            item.SetDatabaseSession(_DatabaseSession);

        foreach (var item in Folders)
            item.SetDatabaseSession(_DatabaseSession);

        foreach (var item in Collections)
            item.SetDatabaseSession(_DatabaseSession);

        Profile?.SetDatabaseSession(_DatabaseSession);
    }

    public void SetDatabaseSession(DatabaseSession databaseSession, Guid? organizationId)
    {
        _DatabaseSession = databaseSession;

        foreach (var item in Items)
            item.SetDatabaseSession(_DatabaseSession, organizationId);

        foreach (var item in Folders)
            item.SetDatabaseSession(_DatabaseSession, organizationId);

        foreach (var item in Collections)
            item.SetDatabaseSession(_DatabaseSession, organizationId);

        Profile?.SetDatabaseSession(_DatabaseSession, organizationId);
    }

    [JsonProperty("ciphers")]
    public List<CipherItemModel> Items { get; set; }

    [JsonProperty("folders")]
    public List<FolderItemModel> Folders { get; set; }

    [JsonProperty("collections")]
    public List<CollectionItemModel> Collections { get; set; }

    [JsonProperty("profile")]
    public ProfileItemModel Profile { get; set; }
}
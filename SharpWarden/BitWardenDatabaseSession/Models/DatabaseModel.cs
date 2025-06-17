using SharpWarden.BitWardenDatabaseSession.CipherItem.Models;
using SharpWarden.BitWardenDatabaseSession.CollectionItem.Models;
using SharpWarden.BitWardenDatabaseSession.FolderItem.Models;
using SharpWarden.BitWardenDatabaseSession.ProfileItem.Models;

namespace SharpWarden.BitWardenDatabaseSession.Models;

public class DatabaseModel
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

    public DatabaseModel(DatabaseSession databaseSession, BitWardenDatabase.Models.DatabaseModel databaseModel)
    {
        _DatabaseSession = databaseSession;

        Items = databaseModel.Items == null ? null : new List<CipherItemModel>(databaseModel.Items.Select(e => new CipherItemModel(_DatabaseSession, e.OrganizationId, e)));
        Folders = databaseModel.Folders == null ? null : new List<FolderItemModel>(databaseModel.Folders.Select(e => new FolderItemModel(_DatabaseSession, e)));
        Collections = databaseModel.Collections == null ? null : new List<CollectionItemModel>(databaseModel.Collections.Select(e => new CollectionItemModel(_DatabaseSession, e)));
        Profile = databaseModel.Profile == null ? null : new ProfileItemModel(databaseSession, databaseModel.Profile);
    }

    public List<CipherItemModel> Items { get; set; }

    public List<FolderItemModel> Folders { get; set; }

    public List<CollectionItemModel> Collections { get; set; }

    public ProfileItemModel Profile { get; set; }

    public BitWardenDatabase.Models.DatabaseModel ToDatabaseModel()
    {
        return new BitWardenDatabase.Models.DatabaseModel
        {
            Items = Items == null ? null : new List<BitWardenDatabase.CipherItem.Models.CipherItemModel>(Items.Select(e => e.ToDatabaseModel())),
            Folders = Folders == null ? null : new List<BitWardenDatabase.FolderItem.Models.FolderItemModel>(Folders.Select(e => e.ToDatabaseModel())),
            Collections = Collections == null ? null : new List<BitWardenDatabase.CollectionItem.Models.CollectionItemModel>(Collections.Select(e => e.ToDatabaseModel())),
            Profile = Profile?.ToDatabaseModel(),
        };
    }
}
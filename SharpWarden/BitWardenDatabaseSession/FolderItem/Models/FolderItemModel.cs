using SharpWarden.BitWardenDatabase;

namespace SharpWarden.BitWardenDatabaseSession.FolderItem.Models;

public class FolderItemModel
{
    private DatabaseSession _DatabaseSession;

    public FolderItemModel(DatabaseSession databaseSession)
    {
        _DatabaseSession = databaseSession;
    }

    public FolderItemModel(DatabaseSession databaseSession, BitWardenDatabase.FolderItem.Models.FolderItemModel databaseModel)
    {
        _DatabaseSession = databaseSession;

        Id = databaseModel.Id;
        _Name = databaseModel.Name;
        RevisionDate = databaseModel.RevisionDate;
    }

    public Guid? Id { get; set; }

    private string _Name;
    public string Name
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(null, _Name);
        set => _Name = _DatabaseSession.CryptClearStringWithMasterKey(null, value);
    }

    public DateTimeOffset? RevisionDate { get; set; }

    public BitWardenDatabase.FolderItem.Models.FolderItemModel ToDatabaseModel()
    {
        return new BitWardenDatabase.FolderItem.Models.FolderItemModel
        {
            Id = Id,
            Name = _Name,
            ObjectType = ObjectType.Folder,
            RevisionDate = RevisionDate,
        };
    }
}
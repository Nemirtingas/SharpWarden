using SharpWarden.BitWardenDatabase;

namespace SharpWarden.BitWardenDatabaseSession.CollectionItem.Models;

public class CollectionItemModel
{
    private DatabaseSession _DatabaseSession;

    public CollectionItemModel(DatabaseSession databaseSession)
    {
        _DatabaseSession = databaseSession;
    }

    public CollectionItemModel(DatabaseSession databaseSession, BitWardenDatabase.CollectionItem.Models.CollectionItemModel databaseModel)
    {
        _DatabaseSession = databaseSession;

        ExternalId = databaseModel.Id;
        HidePasswords = databaseModel.HidePasswords;
        Id = databaseModel.Id;
        Manage = databaseModel.Manage;
        _Name = databaseModel.Name;

        OrganizationId = databaseModel.OrganizationId;
        ReadOnly = databaseModel.ReadOnly;
    }

    public Guid? ExternalId { get; set; }

    public bool HidePasswords { get; set; }

    public Guid? Id { get; set; }

    public bool Manage { get; set; }

    private string _Name;
    public string Name
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(OrganizationId, _Name);
        set => _Name = _DatabaseSession.CryptClearStringWithMasterKey(OrganizationId, value);
    }

    public Guid? OrganizationId { get; set; }

    public bool ReadOnly { get; set; }
    
        public BitWardenDatabase.CollectionItem.Models.CollectionItemModel ToDatabaseModel()
    {
        return new BitWardenDatabase.CollectionItem.Models.CollectionItemModel
        {
            ExternalId = ExternalId,
            HidePasswords = HidePasswords,
            Id = Id,
            Manage = Manage,
            Name = _Name,
            ObjectType = ObjectType.CollectionDetails,
            OrganizationId = OrganizationId,
            ReadOnly = ReadOnly,
        };
    }
}
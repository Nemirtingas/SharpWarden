using SharpWarden.BitWardenDatabase.CipherItem.Models;

namespace SharpWarden.BitWardenDatabaseSession.CipherItem.Models;

public class CustomFieldModel
{
    private DatabaseSession _DatabaseSession;
    private Guid? _OrganizationId;

    public CustomFieldModel(DatabaseSession databaseSession, Guid? organizationId, BitWardenDatabase.CipherItem.Models.CustomFieldModel databaseModel)
    {
        _DatabaseSession = databaseSession;
        _OrganizationId = organizationId;

        LinkedId = databaseModel.LinkedId;
        _Name = databaseModel.Name;
        Type = databaseModel.Type;
        _Value = databaseModel.Value;
    }
    public CustomFieldLinkType? LinkedId { get; set; }

    private string _Name;
    public string Name
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _Name);
        set => _Name = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    public CustomFieldType Type { get; set; }

    private string _Value;
    public string Value
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _Value);
        set => _Value = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    public BitWardenDatabase.CipherItem.Models.CustomFieldModel ToDatabaseModel()
    {
        return new BitWardenDatabase.CipherItem.Models.CustomFieldModel
        {
            LinkedId = LinkedId,
            Name = _Name,
            Type = Type,
            Value = _Value,
        };
    }
}
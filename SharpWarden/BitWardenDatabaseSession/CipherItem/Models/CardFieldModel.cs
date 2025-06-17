namespace SharpWarden.BitWardenDatabaseSession.CipherItem.Models;

public class CardFieldModel
{
    private DatabaseSession _DatabaseSession;
    private Guid? _OrganizationId;

    public CardFieldModel(DatabaseSession databaseSession, Guid? organizationId, BitWardenDatabase.CipherItem.Models.CardFieldModel databaseModel)
    {
        _DatabaseSession = databaseSession;
        _OrganizationId = organizationId;

        _CardholderName = databaseModel.CardholderName;
        _Brand = databaseModel.Brand;
        _Number = databaseModel.Number;
        _ExpMonth = databaseModel.ExpMonth;
        _ExpYear = databaseModel.ExpYear;
        _Code = databaseModel.Code;
    }

    private string _CardholderName;
    public string CardholderName
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _CardholderName);
        set => _CardholderName = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    private string _Brand;
    public string Brand
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _Brand);
        set => _Brand = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    private string _Number;
    public string Number
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _Number);
        set => _Number = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    private string _ExpMonth;
    public string ExpMonth
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _ExpMonth);
        set => _ExpMonth = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    private string _ExpYear;
    public string ExpYear
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _ExpYear);
        set => _ExpYear = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    private string _Code;
    public string Code
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _Code);
        set => _Code = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }
    
    public BitWardenDatabase.CipherItem.Models.CardFieldModel ToDatabaseModel()
    {
        return new BitWardenDatabase.CipherItem.Models.CardFieldModel
        {
            CardholderName = _CardholderName,
            Brand = _Brand,
            Number = _Number,
            ExpMonth = _ExpMonth,
            ExpYear = _ExpYear,
            Code = _Code,
        };
    }
}
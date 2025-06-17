namespace SharpWarden.BitWardenDatabaseSession.CipherItem.Models;

public class IdentityFieldModel
{
    private DatabaseSession _DatabaseSession;
    private Guid? _OrganizationId;

    public IdentityFieldModel(DatabaseSession databaseSession, Guid? organizationId, BitWardenDatabase.CipherItem.Models.IdentityFieldModel databaseModel)
    {
        _DatabaseSession = databaseSession;
        _OrganizationId = organizationId;

        _Address1 = databaseModel.Address1;
        _Address2 = databaseModel.Address2;
        _Address3 = databaseModel.Address3;
        _City = databaseModel.City;
        _Company = databaseModel.Company;
        _Country = databaseModel.Country;
        _Email = databaseModel.Email;
        _FirstName = databaseModel.FirstName;
        _LastName = databaseModel.LastName;
        _LicenseNumber = databaseModel.LicenseNumber;
        _MiddleName = databaseModel.MiddleName;
        _PassportNumber = databaseModel.PassportNumber;
        _Phone = databaseModel.Phone;
        _PostalCode = databaseModel.PostalCode;
        _SSN = databaseModel.SSN;
        _State = databaseModel.SSN;
        _Title = databaseModel.Title;
    }

    private string _Address1;
    public string Address1
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _Address1);
        set => _Address1 = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    private string _Address2;
    public string Address2
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _Address2);
        set => _Address2 = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    private string _Address3;
    public string Address3
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _Address3);
        set => _Address3 = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    private string _City;
    public string City
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _City);
        set => _City = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    private string _Company;
    public string Company
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _Company);
        set => _Company = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    private string _Country;
    public string Country
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _Country);
        set => _Country = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    private string _Email;
    public string Email
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _Email);
        set => _Email = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    private string _FirstName;
    public string FirstName
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _FirstName);
        set => _FirstName = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    private string _LastName;
    public string LastName
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _LastName);
        set => _LastName = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    private string _LicenseNumber;
    public string LicenseNumber
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _LicenseNumber);
        set => _LicenseNumber = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    private string _MiddleName;
    public string MiddleName
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _MiddleName);
        set => _MiddleName = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    private string _PassportNumber;
    public string PassportNumber
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _PassportNumber);
        set => _PassportNumber = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    private string _Phone;
    public string Phone
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _Phone);
        set => _Phone = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    private string _PostalCode;
    public string PostalCode
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _PostalCode);
        set => _PostalCode = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    private string _SSN;
    public string SSN
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _SSN);
        set => _SSN = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    private string _State;
    public string State
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _State);
        set => _State = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    private string _Title;
    public string Title
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _Title);
        set => _Title = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    private string _Username;
    public string Username
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _Username);
        set => _Username = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    public BitWardenDatabase.CipherItem.Models.IdentityFieldModel ToDatabaseModel()
    {
        return new BitWardenDatabase.CipherItem.Models.IdentityFieldModel
        {
            Address1 = _Address1,
            Address2 = _Address2,
            Address3 = _Address3,
            City = _City,
            Company = _Company,
            Country = _Country,
            Email = _Email,
            FirstName = _FirstName,
            LastName = _LastName,
            LicenseNumber = _LicenseNumber,
            MiddleName = _MiddleName,
            PassportNumber = _PassportNumber,
            Phone = _Phone,
            PostalCode = _PostalCode,
            SSN = _SSN,
            State = _SSN,
            Title = _Title,
        };
    }
}
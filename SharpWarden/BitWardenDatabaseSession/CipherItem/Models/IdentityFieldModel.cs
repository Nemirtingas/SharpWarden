using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Models;

namespace SharpWarden.BitWardenDatabaseSession.CipherItem.Models;

public class IdentityFieldModel : IDatabaseSessionModel
{
    private DatabaseSession _DatabaseSession;
    private Guid? _OrganizationId;

    public IdentityFieldModel(DatabaseSession databaseSession)
    {
        SetDatabaseSession(databaseSession);
    }

    public bool HasSession() => _DatabaseSession != null;

    public void SetDatabaseSession(DatabaseSession databaseSession)
    {
        _DatabaseSession = databaseSession;

        Address1 = new EncryptedString(Address1.CipherString, databaseSession);
        Address2 = new EncryptedString(Address2.CipherString, databaseSession);
        Address3 = new EncryptedString(Address3.CipherString, databaseSession);
        City = new EncryptedString(City.CipherString, databaseSession);
        Company = new EncryptedString(Company.CipherString, databaseSession);
        Country = new EncryptedString(Country.CipherString, databaseSession);
        Email = new EncryptedString(Email.CipherString, databaseSession);
        FirstName = new EncryptedString(FirstName.CipherString, databaseSession);
        LastName = new EncryptedString(LastName.CipherString, databaseSession);
        LicenseNumber = new EncryptedString(LicenseNumber.CipherString, databaseSession);
        MiddleName = new EncryptedString(MiddleName.CipherString, databaseSession);
        PassportNumber = new EncryptedString(PassportNumber.CipherString, databaseSession);
        Phone = new EncryptedString(Phone.CipherString, databaseSession);
        PostalCode = new EncryptedString(PostalCode.CipherString, databaseSession);
        SSN = new EncryptedString(SSN.CipherString, databaseSession);
        State = new EncryptedString(State.CipherString, databaseSession);
        Title = new EncryptedString(Title.CipherString, databaseSession);
    }

    public void SetDatabaseSession(DatabaseSession databaseSession, Guid? organizationId)
    {
        _DatabaseSession = databaseSession;
        _OrganizationId = organizationId;

        Address1 = new EncryptedString(Address1.CipherString, databaseSession, _OrganizationId);
        Address2 = new EncryptedString(Address2.CipherString, databaseSession, _OrganizationId);
        Address3 = new EncryptedString(Address3.CipherString, databaseSession, _OrganizationId);
        City = new EncryptedString(City.CipherString, databaseSession, _OrganizationId);
        Company = new EncryptedString(Company.CipherString, databaseSession, _OrganizationId);
        Country = new EncryptedString(Country.CipherString, databaseSession, _OrganizationId);
        Email = new EncryptedString(Email.CipherString, databaseSession, _OrganizationId);
        FirstName = new EncryptedString(FirstName.CipherString, databaseSession, _OrganizationId);
        LastName = new EncryptedString(LastName.CipherString, databaseSession, _OrganizationId);
        LicenseNumber = new EncryptedString(LicenseNumber.CipherString, databaseSession, _OrganizationId);
        MiddleName = new EncryptedString(MiddleName.CipherString, databaseSession, _OrganizationId);
        PassportNumber = new EncryptedString(PassportNumber.CipherString, databaseSession, _OrganizationId);
        Phone = new EncryptedString(Phone.CipherString, databaseSession, _OrganizationId);
        PostalCode = new EncryptedString(PostalCode.CipherString, databaseSession, _OrganizationId);
        SSN = new EncryptedString(SSN.CipherString, databaseSession, _OrganizationId);
        State = new EncryptedString(State.CipherString, databaseSession, _OrganizationId);
        Title = new EncryptedString(Title.CipherString, databaseSession, _OrganizationId);
    }

    [JsonProperty("address1")]
    public EncryptedString Address1 { get; set; }

    [JsonProperty("address2")]
    public EncryptedString Address2 { get; set; }

    [JsonProperty("address3")]
    public EncryptedString Address3 { get; set; }

    [JsonProperty("city")]
    public EncryptedString City { get; set; }

    [JsonProperty("company")]
    public EncryptedString Company { get; set; }

    [JsonProperty("country")]
    public EncryptedString Country { get; set; }

    [JsonProperty("email")]
    public EncryptedString Email { get; set; }

    [JsonProperty("firstName")]
    public EncryptedString FirstName { get; set; }

    [JsonProperty("lastName")]
    public EncryptedString LastName { get; set; }

    [JsonProperty("licenseNumber")]
    public EncryptedString LicenseNumber { get; set; }

    [JsonProperty("middleName")]
    public EncryptedString MiddleName { get; set; }

    [JsonProperty("passportNumber")]
    public EncryptedString PassportNumber { get; set; }

    [JsonProperty("phone")]
    public EncryptedString Phone { get; set; }

    [JsonProperty("postalCode")]
    public EncryptedString PostalCode { get; set; }

    [JsonProperty("ssn")]
    public EncryptedString SSN { get; set; }

    [JsonProperty("state")]
    public EncryptedString State { get; set; }

    [JsonProperty("title")]
    public EncryptedString Title { get; set; }

    [JsonProperty("username")]
    public EncryptedString Username { get; set; }
}
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

        Address1?.SetDatabaseSession(_DatabaseSession);
        Address2?.SetDatabaseSession(_DatabaseSession);
        Address3?.SetDatabaseSession(_DatabaseSession);
        City?.SetDatabaseSession(_DatabaseSession);
        Company?.SetDatabaseSession(_DatabaseSession);
        Country?.SetDatabaseSession(_DatabaseSession);
        Email?.SetDatabaseSession(_DatabaseSession);
        FirstName?.SetDatabaseSession(_DatabaseSession);
        LastName?.SetDatabaseSession(_DatabaseSession);
        LicenseNumber?.SetDatabaseSession(_DatabaseSession);
        MiddleName?.SetDatabaseSession(_DatabaseSession);
        PassportNumber?.SetDatabaseSession(_DatabaseSession);
        Phone?.SetDatabaseSession(_DatabaseSession);
        PostalCode?.SetDatabaseSession(_DatabaseSession);
        SSN?.SetDatabaseSession(_DatabaseSession);
        State?.SetDatabaseSession(_DatabaseSession);
        Title?.SetDatabaseSession(_DatabaseSession);
    }

    public void SetDatabaseSession(DatabaseSession databaseSession, Guid? organizationId)
    {
        _DatabaseSession = databaseSession;
        _OrganizationId = organizationId;

        Address1?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
        Address2?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
        Address3?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
        City?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
        Company?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
        Country?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
        Email?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
        FirstName?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
        LastName?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
        LicenseNumber?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
        MiddleName?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
        PassportNumber?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
        Phone?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
        PostalCode?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
        SSN?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
        State?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
        Title?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
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
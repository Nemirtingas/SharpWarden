using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Models;

namespace SharpWarden.BitWardenDatabaseSession.CipherItem.Models;

public class CardFieldModel : IDatabaseSessionModel
{
    private DatabaseSession _DatabaseSession;
    private Guid? _OrganizationId;

    public CardFieldModel(DatabaseSession databaseSession)
    {
        SetDatabaseSession(databaseSession);
    }

    public bool HasSession() => _DatabaseSession != null;

    public void SetDatabaseSession(DatabaseSession databaseSession)
    {
        _DatabaseSession = databaseSession;

        CardholderName = new EncryptedString(CardholderName.CipherString, _DatabaseSession);
        Brand = new EncryptedString(Brand.CipherString, _DatabaseSession);
        Number = new EncryptedString(Number.CipherString, _DatabaseSession);
        ExpMonth = new EncryptedString(ExpMonth.CipherString, _DatabaseSession);
        ExpYear = new EncryptedString(ExpYear.CipherString, _DatabaseSession);
        Code = new EncryptedString(Code.CipherString, _DatabaseSession);
    }

    public void SetDatabaseSession(DatabaseSession databaseSession, Guid? organizationId)
    {
        _DatabaseSession = databaseSession;
        _OrganizationId = organizationId;

        CardholderName = new EncryptedString(CardholderName.CipherString, _DatabaseSession, _OrganizationId);
        Brand = new EncryptedString(Brand.CipherString, _DatabaseSession, _OrganizationId);
        Number = new EncryptedString(Number.CipherString, _DatabaseSession, _OrganizationId);
        ExpMonth = new EncryptedString(ExpMonth.CipherString, _DatabaseSession, _OrganizationId);
        ExpYear = new EncryptedString(ExpYear.CipherString, _DatabaseSession, _OrganizationId);
        Code = new EncryptedString(Code.CipherString, _DatabaseSession, _OrganizationId);
    }

    [JsonProperty("cardholderName")]
    public EncryptedString CardholderName { get; set; }

    [JsonProperty("brand")]
    public EncryptedString Brand { get; set; }

    [JsonProperty("number")]
    public EncryptedString Number { get; set; }

    [JsonProperty("expMonth")]
    public EncryptedString ExpMonth { get; set; }

    [JsonProperty("expYear")]
    public EncryptedString ExpYear { get; set; }

    [JsonProperty("code")]
    public EncryptedString Code { get; set; }
}
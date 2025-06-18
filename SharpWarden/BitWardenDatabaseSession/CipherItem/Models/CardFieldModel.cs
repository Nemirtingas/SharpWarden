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

        CardholderName?.SetDatabaseSession(_DatabaseSession);
        Brand?.SetDatabaseSession(_DatabaseSession);
        Number?.SetDatabaseSession(_DatabaseSession);
        ExpMonth?.SetDatabaseSession(_DatabaseSession);
        ExpYear?.SetDatabaseSession(_DatabaseSession);
        Code?.SetDatabaseSession(_DatabaseSession);
    }

    public void SetDatabaseSession(DatabaseSession databaseSession, Guid? organizationId)
    {
        _DatabaseSession = databaseSession;
        _OrganizationId = organizationId;

        CardholderName?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
        Brand?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
        Number?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
        ExpMonth?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
        ExpYear?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
        Code?.SetDatabaseSession(_DatabaseSession, _OrganizationId);
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
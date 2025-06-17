using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Models;

namespace SharpWarden.BitWardenDatabaseSession.CipherItem.Models;

public class AttachmentModel : IDatabaseSessionModel
{
    private DatabaseSession _DatabaseSession;
    private Guid? _OrganizationId;

    public AttachmentModel(DatabaseSession databaseSession)
    {
        SetDatabaseSession(databaseSession);
    }

    public bool HasSession() => _DatabaseSession != null;

    public void SetDatabaseSession(DatabaseSession databaseSession)
    {
        _DatabaseSession = databaseSession;

        FileName = new EncryptedString(FileName.CipherString, _DatabaseSession);
        Key = new EncryptedString(Key.CipherString, _DatabaseSession);
    }

    public void SetDatabaseSession(DatabaseSession databaseSession, Guid? organizationId)
    {
        _DatabaseSession = databaseSession;
        _OrganizationId = organizationId;

        FileName = new EncryptedString(FileName.CipherString, _DatabaseSession, _OrganizationId);
        Key = new EncryptedString(Key.CipherString, _DatabaseSession, _OrganizationId);
    }

    [JsonProperty("fileName")]
    public EncryptedString FileName { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("key")]
    public EncryptedString Key { get; set; }

    [JsonProperty("object")]
    public ObjectType ObjectType { get; set; } = ObjectType.Attachment;

    [JsonProperty("size")]
    public int Size { get; set; }

    [JsonProperty("sizeName")]
    public string SizeName { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }
}
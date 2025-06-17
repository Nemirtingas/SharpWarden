using SharpWarden.BitWardenDatabase;

namespace SharpWarden.BitWardenDatabaseSession.CipherItem.Models;

public class AttachmentModel
{
    private DatabaseSession _DatabaseSession;
    private Guid? _OrganizationId;

    public AttachmentModel(DatabaseSession databaseSession, Guid? organizationId, BitWardenDatabase.CipherItem.Models.AttachmentModel databaseModel)
    {
        _DatabaseSession = databaseSession;
        _OrganizationId = organizationId;

        _FileName = databaseModel.FileName;
        Id = databaseModel.Id;
        _Key = databaseModel.Key;
        Size = databaseModel.Size;
        SizeName = databaseModel.SizeName;
        Url = databaseModel.Url;
    }

    private string _FileName;
    public string FileName
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _FileName);
        set => _FileName = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    public string Id { get; set; }

    private string _Key;
    public byte[] Key
    {
        get => _DatabaseSession.GetClearBytesWithMasterKey(_OrganizationId, _Key);
        set => _Key = _DatabaseSession.CryptClearBytesWithMasterKey(_OrganizationId, value);
    }

    public int Size { get; set; }

    public string SizeName { get; set; }

    public string Url { get; set; }

    public BitWardenDatabase.CipherItem.Models.AttachmentModel ToDatabaseModel()
    {
        return new BitWardenDatabase.CipherItem.Models.AttachmentModel
        {
            FileName = _FileName,
            Id = Id,
            Key = _Key,
            ObjectType = ObjectType.Attachment,
            Size = Size,
            SizeName = SizeName,
            Url = Url
        };
    }
}
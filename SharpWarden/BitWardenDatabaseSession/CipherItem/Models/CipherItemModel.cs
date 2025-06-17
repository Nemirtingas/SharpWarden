using SharpWarden.BitWardenDatabase;

namespace SharpWarden.BitWardenDatabaseSession.CipherItem.Models;

public class CipherItemModel
{
    private DatabaseSession _DatabaseSession;
    private Guid? _OrganizationId;

    public CipherItemModel(DatabaseSession databaseSession, Guid? organizationId)
    {
        _DatabaseSession = databaseSession;
        _OrganizationId = organizationId;
        PasswordHistory = new();
    }

    public CipherItemModel(DatabaseSession databaseSession, Guid? organizationId, BitWardenDatabase.CipherItem.Models.CipherItemModel databaseModel)
    {
        _DatabaseSession = databaseSession;
        _OrganizationId = organizationId;

        if (databaseModel.Attachments != null)
            Attachments = new List<AttachmentModel>(databaseModel.Attachments.Select(e => new AttachmentModel(databaseSession, organizationId, e)));

        if (databaseModel.Card != null)
            Card = new CardFieldModel(databaseSession, organizationId, databaseModel.Card);

        if (databaseModel.CollectionsIds != null)
            CollectionsIds = new List<Guid>(databaseModel.CollectionsIds);

        CreationDate = databaseModel.CreationDate;
        DeletedDate = databaseModel.DeletedDate;
        Edit = databaseModel.Edit;
        Favorite = databaseModel.Favorite;

        if (databaseModel.Fields != null)
            Fields = new List<CustomFieldModel>(databaseModel.Fields.Select(e => new CustomFieldModel(databaseSession, organizationId, e)));

        FolderId = databaseModel.FolderId;
        Id = databaseModel.Id;

        if (databaseModel.Identity != null)
            Identity = new IdentityFieldModel(databaseSession, organizationId, databaseModel.Identity);

        if (databaseModel.Login != null)
            Login = new LoginFieldModel(databaseSession, organizationId, databaseModel.Login);

        _Name = databaseModel.Name;
        _Notes = databaseModel.Notes;
        OrganizationUseTopt = databaseModel.OrganizationUseTopt;
        PasswordHistory = new List<BitWardenDatabase.CipherItem.Models.PasswordHistoryModel>(databaseModel.PasswordHistory);
        Reprompt = databaseModel.Reprompt;
        RevisionDate = databaseModel.RevisionDate;

        if (databaseModel.SecureNote != null)
            SecureNote = new SecureNoteFieldModel(databaseSession, organizationId, databaseModel.SecureNote);

        _SshKey = databaseModel.SshKey;
        ItemType = databaseModel.ItemType;
        ViewPassword = databaseModel.ViewPassword;
    }

    public List<AttachmentModel> Attachments { get; set; }

    public CardFieldModel Card { get; set; }

    public List<Guid> CollectionsIds { get; set; }

    public DateTimeOffset? CreationDate { get; set; }

    public DateTimeOffset? DeletedDate { get; set; }

    public bool Edit { get; set; }

    public bool Favorite { get; set; }

    public List<CustomFieldModel> Fields { get; set; }

    public Guid? FolderId { get; set; }

    public Guid Id { get; set; }

    public IdentityFieldModel Identity { get; set; }

    // Probably an encrypted string
    public string Key { get; set; }

    public LoginFieldModel Login { get; set; }

    private string _Name;
    public string Name
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _Name);
        set => _Name = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    private string _Notes;
    public string Notes
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _Notes);
        set => _Notes = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    public Guid? OrganizationId { get; set; }

    public bool OrganizationUseTopt { get; set; }

    public List<BitWardenDatabase.CipherItem.Models.PasswordHistoryModel> PasswordHistory { get; set; }

    public int Reprompt { get; set; }

    public DateTimeOffset? RevisionDate { get; set; }

    public SecureNoteFieldModel SecureNote { get; set; }

    private string _SshKey;
    public string SshKey
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _SshKey);
        set => _SshKey = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    public BitWardenDatabase.CipherItem.Models.CipherItemType ItemType { get; set; }

    public bool ViewPassword { get; set; }

    public BitWardenDatabase.CipherItem.Models.CipherItemModel ToDatabaseModel()
    {
        return new BitWardenDatabase.CipherItem.Models.CipherItemModel
        {
            Attachments = Attachments == null ? null : new List<BitWardenDatabase.CipherItem.Models.AttachmentModel>(Attachments.Select(e => e.ToDatabaseModel())),
            Card = Card?.ToDatabaseModel(),
            CollectionsIds = new List<Guid>(CollectionsIds),
            CreationDate = CreationDate,
            DeletedDate = DeletedDate,
            Edit = Edit,
            Favorite = Favorite,
            Fields = Fields == null ? null : new List<BitWardenDatabase.CipherItem.Models.CustomFieldModel>(Fields.Select(e => e.ToDatabaseModel())),
            FolderId = FolderId,
            Id = Id,
            Identity = Identity?.ToDatabaseModel(),
            Key = Key,
            Login = Login?.ToDatabaseModel(),
            Name = _Name,
            Notes = _Notes,
            ObjectType = ObjectType.CipherDetails,
            OrganizationId = OrganizationId,
            OrganizationUseTopt = OrganizationUseTopt,
            PasswordHistory = new List<BitWardenDatabase.CipherItem.Models.PasswordHistoryModel>(PasswordHistory),
            Reprompt = Reprompt,
            RevisionDate = RevisionDate,
            SecureNote = SecureNote?.ToDatabaseModel(),
            SshKey = _SshKey,
            ItemType = ItemType,
            ViewPassword = ViewPassword,
        };
    }
}

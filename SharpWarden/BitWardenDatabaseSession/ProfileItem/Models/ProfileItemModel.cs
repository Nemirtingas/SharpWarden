using SharpWarden.BitWardenDatabase;

namespace SharpWarden.BitWardenDatabaseSession.ProfileItem.Models;

public class ProfileItemModel
{
    private DatabaseSession _DatabaseSession;

    public ProfileItemModel(DatabaseSession databaseSession)
    {
        _DatabaseSession = databaseSession;

        Organizations = new();
    }

    public ProfileItemModel(DatabaseSession databaseSession, BitWardenDatabase.ProfileItem.Models.ProfileItemModel databaseModel)
    {
        _DatabaseSession = databaseSession;

        _Status = databaseModel._Status;
        AvatarColor = databaseModel.AvatarColor;
        CreationDate = databaseModel.CreationDate;
        Culture = databaseModel.Culture;
        Email = databaseModel.Email;
        EmailVerified = databaseModel.EmailVerified;
        ForcePasswordReset = databaseModel.ForcePasswordReset;
        Id = databaseModel.Id;
        _Key = databaseModel.Key;
        MasterPasswordHint = databaseModel.MasterPasswordHint;
        Name = databaseModel.Name;
        ObjectType = databaseModel.ObjectType;
        Organizations = databaseModel.Organizations == null ? null : new List<OrganizationModel>(databaseModel.Organizations.Select(e => new OrganizationModel(e)));
        Premium = databaseModel.Premium;
        PremiumFromOrganization = databaseModel.PremiumFromOrganization;
        _PrivateKey = databaseModel.PrivateKey;
        ProviderOrganizations = databaseModel.ProviderOrganizations;
        Providers = databaseModel.Providers;
        SecurityStamp = databaseModel.SecurityStamp;
        TwoFactorEnabled = databaseModel.TwoFactorEnabled;
        UsesKeyConnector = databaseModel.UsesKeyConnector;
    }

    public string _Status { get; set; }

    public string AvatarColor { get; set; }

    public DateTimeOffset? CreationDate { get; set; }

    public string Culture { get; set; }

    public string Email { get; set; }

    public bool EmailVerified { get; set; }

    public bool ForcePasswordReset { get; set; }

    public Guid? Id { get; set; }

    private string _Key;
    /// <summary>
    /// Don't update this unless you know what you're doing! It will invalidate your whole database!
    /// </summary>
    public string Key
    {
        get => _Key;
        set => throw new NotImplementedException();
    }

    public string MasterPasswordHint { get; set; }

    public string Name { get; set; }

    public ObjectType ObjectType { get; } = ObjectType.Profile;

    public List<OrganizationModel> Organizations { get; }

    public bool Premium { get; set; }

    public bool PremiumFromOrganization { get; set; }

    private string _PrivateKey;
    public byte[] PrivateKey
    {
        get => _DatabaseSession.GetClearBytesWithMasterKey(null, _PrivateKey);
        set => _PrivateKey = _DatabaseSession.CryptClearBytesWithMasterKey(null, value);
    }

    public List<object> ProviderOrganizations { get; set; }

    public List<object> Providers { get; set; }

    public Guid? SecurityStamp { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public bool UsesKeyConnector { get; set; }

    public BitWardenDatabase.ProfileItem.Models.ProfileItemModel ToDatabaseModel()
    {
        return new BitWardenDatabase.ProfileItem.Models.ProfileItemModel
        {
            _Status = _Status,
            AvatarColor = AvatarColor,
            CreationDate = CreationDate,
            Culture = Culture,
            Email = Email,
            EmailVerified = EmailVerified,
            ForcePasswordReset = ForcePasswordReset,
            Id = Id,
            Key = _Key,
            MasterPasswordHint = MasterPasswordHint,
            Name = Name,
            ObjectType = ObjectType,
            Organizations = Organizations == null ? null : new List<BitWardenDatabase.ProfileItem.Models.OrganizationModel>(Organizations.Select(e => e.ToDatabaseModel())),
            Premium = Premium,
            PremiumFromOrganization = PremiumFromOrganization,
            PrivateKey = _PrivateKey,
            ProviderOrganizations = ProviderOrganizations,
            Providers = Providers,
            SecurityStamp = SecurityStamp,
            TwoFactorEnabled = TwoFactorEnabled,
            UsesKeyConnector = UsesKeyConnector,
        };
    }
 }
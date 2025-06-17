namespace SharpWarden.BitWardenDatabaseSession.CipherItem.Models;

public class LoginFieldModel
{
    private DatabaseSession _DatabaseSession;
    private Guid? _OrganizationId;

    public LoginFieldModel(DatabaseSession databaseSession, Guid? organizationId, BitWardenDatabase.CipherItem.Models.LoginFieldModel databaseModel)
    {
        _DatabaseSession = databaseSession;
        _OrganizationId = organizationId;

        AutoFillOnPageLoad = databaseModel.AutoFillOnPageLoad;
        _Password = databaseModel.Password;
        PasswordRevisionDate = databaseModel.PasswordRevisionDate;
        TOTP = databaseModel.TOTP;
        _Uri = databaseModel.Uri;
        if (databaseModel.Uris != null)
            Uris = new List<UrlModel>(databaseModel.Uris.Select(e => new UrlModel(databaseSession, organizationId, e)));

        _Username = databaseModel.Username;
    }

    public bool? AutoFillOnPageLoad { get; set; }

    private string _Password;
    public string Password
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _Password);
        set => _Password = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    public object PasswordRevisionDate { get; set; }

    public object TOTP { get; set; }

    private string _Uri;
    public string Uri
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _Uri);
        set => _Uri = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    public List<UrlModel> Uris { get; set; }

    private string _Username;
    public string Username
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _Username);
        set => _Username = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    public BitWardenDatabase.CipherItem.Models.LoginFieldModel ToDatabaseModel()
    {
        return new BitWardenDatabase.CipherItem.Models.LoginFieldModel
        {
            AutoFillOnPageLoad = AutoFillOnPageLoad,
            Password = _Password,
            PasswordRevisionDate = PasswordRevisionDate,
            TOTP = TOTP,
            Uri = _Uri,
            Uris = new List<BitWardenDatabase.CipherItem.Models.UrlModel>(Uris.Select(e => e.ToDatabaseModel())),
            Username = _Username,
        };
    }
}
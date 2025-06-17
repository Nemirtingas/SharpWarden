namespace SharpWarden.BitWardenDatabaseSession.CipherItem.Models;

public class UrlModel
{
    private DatabaseSession _DatabaseSession;
    private Guid? _OrganizationId;

    public UrlModel(DatabaseSession databaseSession, Guid? organizationId, BitWardenDatabase.CipherItem.Models.UrlModel databaseModel)
    {
        _DatabaseSession = databaseSession;
        _OrganizationId = organizationId;

        Match = databaseModel.Match;
        _Uri = databaseModel.Uri;
        _UriChecksum = databaseModel.UriChecksum;
    }

    public BitWardenDatabase.CipherItem.Models.UrlMatchType? Match { get; set; }

    private string _Uri;
    public string Uri
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _Uri);
        set => _Uri = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    private string _UriChecksum;
    public string UriChecksum
    {
        get => _DatabaseSession.GetClearStringWithMasterKey(_OrganizationId, _UriChecksum);
        set => _UriChecksum = _DatabaseSession.CryptClearStringWithMasterKey(_OrganizationId, value);
    }

    public BitWardenDatabase.CipherItem.Models.UrlModel ToDatabaseModel()
    {
        return new BitWardenDatabase.CipherItem.Models.UrlModel
        {
            Match = Match,
            Uri = _Uri,
            UriChecksum = _UriChecksum,
        };
    }
}
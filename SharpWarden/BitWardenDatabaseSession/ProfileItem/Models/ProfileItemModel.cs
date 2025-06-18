using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Models;

namespace SharpWarden.BitWardenDatabaseSession.ProfileItem.Models;

public class ProfileItemModel : IDatabaseSessionModel
{
    private DatabaseSession _DatabaseSession;

    public ProfileItemModel(DatabaseSession databaseSession)
    {
        Organizations = new();

        SetDatabaseSession(databaseSession);
    }

    public bool HasSession() => _DatabaseSession != null;

    public void SetDatabaseSession(DatabaseSession databaseSession)
    {
        _DatabaseSession = databaseSession;

        Key?.SetDatabaseSession(_DatabaseSession);
        PrivateKey?.SetDatabaseSession(_DatabaseSession);
    }

    public void SetDatabaseSession(DatabaseSession databaseSession, Guid? organizationId)
        => SetDatabaseSession(databaseSession, organizationId);

    [JsonProperty("_status")]
    public string _Status { get; set; }

    [JsonProperty("avatarColor")]
    public string AvatarColor { get; set; }

    [JsonProperty("creationDate")]
    public DateTime? CreationDate { get; set; }

    [JsonProperty("culture")]
    public string Culture { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("emailVerified")]
    public bool EmailVerified { get; set; }

    [JsonProperty("forcePasswordReset")]
    public bool ForcePasswordReset { get; set; }

    [JsonProperty("id")]
    public Guid? Id { get; set; }

    /// <summary>
    /// Don't update this unless you know what you're doing! It will invalidate your whole database!
    /// </summary>
    [JsonProperty("key")]
    public EncryptedString Key { get; set; }

    [JsonProperty("masterPasswordHint")]
    public string MasterPasswordHint { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("object")]
    public ObjectType ObjectType { get; set; } = ObjectType.Profile;

    [JsonProperty("organizations")]
    public List<OrganizationModel> Organizations { get; set; } = new();

    [JsonProperty("premium")]
    public bool Premium { get; set; }

    [JsonProperty("premiumFromOrganization")]
    public bool PremiumFromOrganization { get; set; }

    [JsonProperty("privateKey")]
    public EncryptedString PrivateKey { get; set; }

    [JsonProperty("providerOrganizations")]
    public List<object> ProviderOrganizations { get; set; }

    [JsonProperty("providers")]
    public List<object> Providers { get; set; }

    [JsonProperty("securityStamp")]
    public Guid? SecurityStamp { get; set; }

    [JsonProperty("twoFactorEnabled")]
    public bool TwoFactorEnabled { get; set; }

    [JsonProperty("usesKeyConnector")]
    public bool UsesKeyConnector { get; set; }
 }
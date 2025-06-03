using Newtonsoft.Json;

namespace BitwardenSharp.Models;

public class BitWardenProfileModel
{
    [JsonProperty("_status")]
    public string _Status { get; set; }

    [JsonProperty("avatarColor")]
    public string AvatarColor { get; set; }
    
    [JsonProperty("creationDate")]
    public DateTimeOffset? CreationDate { get; set; }

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

    [JsonProperty("key")]
    public string Key { get; set; }

    [JsonProperty("masterPasswordHint")]
    public string MasterPasswordHint { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("object")]
    public BitWardenObjectType ObjectType { get; set; }

    [JsonProperty("organizations")]
    public List<BitWardenProfileOrganizationModel> Organizations { get; set; } = new();

    [JsonProperty("premium")]
    public bool Premium { get; set; }

    [JsonProperty("premiumFromOrganization")]
    public bool PremiumFromOrganization { get; set; }

    [JsonProperty("privateKey")]
    public string PrivateKey { get; set; }

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
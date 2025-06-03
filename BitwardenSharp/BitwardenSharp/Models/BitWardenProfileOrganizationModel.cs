using Newtonsoft.Json;

namespace BitwardenSharp.Models;

public class BitWardenProfileOrganizationModel
{
    [JsonProperty("accessSecretsManager")]
    public bool AccessSecretsManager { get; set; }

    [JsonProperty("allowAdminAccessToAllCollectionItems")]
    public bool AllowAdminAccessToAllCollectionItems { get; set; }

    [JsonProperty("enabled")]
    public bool Enabled { get; set; }

    [JsonProperty("familySponsorshipAvailable")]
    public bool FamilySponsorshipAvailable { get; set; }

    [JsonProperty("familySponsorshipFriendlyName")]
    public string FamilySponsorshipFriendlyName { get; set; }

    [JsonProperty("familySponsorshipLastSyncDate")]
    public DateTimeOffset? FamilySponsorshipLastSyncDate { get; set; }

    [JsonProperty("familySponsorshipToDelete")]
    public bool? FamilySponsorshipToDelete { get; set; }

    [JsonProperty("familySponsorshipValidUntil")]
    public DateTimeOffset? FamilySponsorshipValidUntil { get; set; }

    [JsonProperty("hasPublicAndPrivateKeys")]
    public bool HasPublicAndPrivateKeys { get; set; }

    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("identifier")]
    public string Identifier { get; set; }

    [JsonProperty("key")]
    public BitWardenEncryptedString Key { get; set; }

    [JsonProperty("keyConnectorEnabled")]
    public bool KeyConnectorEnabled { get; set; }

    [JsonProperty("keyConnectorUrl")]
    public string KeyConnectorUrl { get; set; }

    [JsonProperty("limitCollectionCreation")]
    public bool LimitCollectionCreation { get; set; }

    [JsonProperty("limitCollectionCreationDeletion")]
    public bool LimitCollectionCreationDeletion { get; set; }

    [JsonProperty("limitCollectionDeletion")]
    public bool LimitCollectionDeletion { get; set; }

    [JsonProperty("maxCollections")]
    public int? MaxCollections { get; set; }

    [JsonProperty("maxStorageGb")]
    public int MaxStorageGb { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("object")]
    public BitWardenObjectType ObjectType { get; set; }

    [JsonProperty("organizationUserId")]
    public Guid OrganizationUserId { get; set; }

    [JsonProperty("permissions")]
    public BitWardenProfileOrganizationPermissionsModel Permissions { get; set; }

    [JsonProperty("planProductType")]
    public int PlanProductType { get; set; }

    [JsonProperty("productTierType")]
    public int ProductTierType { get; set; }

    [JsonProperty("providerId")]
    public Guid? ProviderId { get; set; }

    [JsonProperty("providerName")]
    public string ProviderName { get; set; }

    [JsonProperty("providerType")]
    public int? ProviderType { get; set; }

    [JsonProperty("resetPasswordEnrolled")]
    public bool ResetPasswordEnrolled { get; set; }

    [JsonProperty("seats")]
    public object Seats { get; set; }

    [JsonProperty("selfHost")]
    public bool SelfHost { get; set; }

    [JsonProperty("SsoBound")]
    public bool ssoBound { get; set; }

    [JsonProperty("status")]
    public int Status { get; set; }

    [JsonProperty("type")]
    public int Type { get; set; }

    [JsonProperty("use2fa")]
    public bool Use2fa { get; set; }

    [JsonProperty("useActivateAutofillPolicy")]
    public bool UseActivateAutofillPolicy { get; set; }

    [JsonProperty("useApi")]
    public bool UseApi { get; set; }

    [JsonProperty("useCustomPermissions")]
    public bool UseCustomPermissions { get; set; }

    [JsonProperty("useDirectory")]
    public bool UseDirectory { get; set; }

    [JsonProperty("useEvents")]
    public bool UseEvents { get; set; }

    [JsonProperty("useGroups")]
    public bool UseGroups { get; set; }

    [JsonProperty("useKeyConnector")]
    public bool UseKeyConnector { get; set; }

    [JsonProperty("usePasswordManager")]
    public bool UsePasswordManager { get; set; }

    [JsonProperty("usePolicies")]
    public bool UsePolicies { get; set; }

    [JsonProperty("useResetPassword")]
    public bool UseResetPassword { get; set; }

    [JsonProperty("useScim")]
    public bool UseScim { get; set; }

    [JsonProperty("useSecretsManager")]
    public bool UseSecretsManager { get; set; }
    
    [JsonProperty("useSso")]
    public bool UseSso { get; set; }

    [JsonProperty("useTotp")]
    public bool UseTotp { get; set; }

    [JsonProperty("userId")]
    public Guid UserId { get; set; }
    
    [JsonProperty("userIsManagedByOrganization")]
    public bool UserIsManagedByOrganization { get; set; }

    [JsonProperty("usersGetPremium")]
    public bool UsersGetPremium { get; set; }
}
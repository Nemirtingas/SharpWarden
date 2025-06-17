using SharpWarden.BitWardenDatabase;

namespace SharpWarden.BitWardenDatabaseSession.ProfileItem.Models;

public class OrganizationModel
{
    public OrganizationModel(BitWardenDatabase.ProfileItem.Models.OrganizationModel databaseModel)
    {
        AccessSecretsManager = databaseModel.AccessSecretsManager;
        AllowAdminAccessToAllCollectionItems = databaseModel.AllowAdminAccessToAllCollectionItems;
        Enabled = databaseModel.Enabled;
        FamilySponsorshipAvailable = databaseModel.FamilySponsorshipAvailable;
        FamilySponsorshipFriendlyName = databaseModel.FamilySponsorshipFriendlyName;
        FamilySponsorshipLastSyncDate = databaseModel.FamilySponsorshipLastSyncDate;
        FamilySponsorshipToDelete = databaseModel.FamilySponsorshipToDelete;
        FamilySponsorshipValidUntil = databaseModel.FamilySponsorshipValidUntil;
        HasPublicAndPrivateKeys = databaseModel.HasPublicAndPrivateKeys;
        Id = databaseModel.Id;
        Identifier = databaseModel.Identifier;
        _Key = databaseModel.Key;
        KeyConnectorEnabled = databaseModel.KeyConnectorEnabled;
        KeyConnectorUrl = databaseModel.KeyConnectorUrl;
        LimitCollectionCreation = databaseModel.LimitCollectionCreation;
        LimitCollectionCreationDeletion = databaseModel.LimitCollectionCreationDeletion;
        LimitCollectionDeletion = databaseModel.LimitCollectionDeletion;
        MaxCollections = databaseModel.MaxCollections;
        MaxStorageGb = databaseModel.MaxStorageGb;
        Name = databaseModel.Name;
        OrganizationUserId = databaseModel.OrganizationUserId;
        Permissions = databaseModel.Permissions == null ? null : new OrganizationPermissionsModel(databaseModel.Permissions);
        PlanProductType = databaseModel.PlanProductType;
        ProductTierType = databaseModel.ProductTierType;
        ProviderId = databaseModel.ProviderId;
        ProviderName = databaseModel.ProviderName;
        ProviderType = databaseModel.ProviderType;
        ResetPasswordEnrolled = databaseModel.ResetPasswordEnrolled;
        Seats = databaseModel.Seats;
        SelfHost = databaseModel.SelfHost;
        SSOBound = databaseModel.SSOBound;
        Status = databaseModel.Status;
        Type = databaseModel.Type;
        Use2FA = databaseModel.Use2FA;
        UseActivateAutofillPolicy = databaseModel.UseActivateAutofillPolicy;
        UseAPI = databaseModel.UseAPI;
        UseCustomPermissions = databaseModel.UseCustomPermissions;
        UseDirectory = databaseModel.UseDirectory;
        UseEvents = databaseModel.UseEvents;
        UseGroups = databaseModel.UseGroups;
        UseKeyConnector = databaseModel.UseKeyConnector;
        UsePasswordManager = databaseModel.UsePasswordManager;
        UsePolicies = databaseModel.UsePolicies;
        UseResetPassword = databaseModel.UseResetPassword;
        UseSCIM = databaseModel.UseSCIM;
        UseSecretsManager = databaseModel.UseSecretsManager;
        UseSSO = databaseModel.UseSSO;
        UseTOTP = databaseModel.UseTOTP;
        UserId = databaseModel.UserId;
        UserIsManagedByOrganization = databaseModel.UserIsManagedByOrganization;
        UsersGetPremium = databaseModel.UsersGetPremium;
    }

    public bool AccessSecretsManager { get; set; }

    public bool AllowAdminAccessToAllCollectionItems { get; set; }

    public bool Enabled { get; set; }

    public bool FamilySponsorshipAvailable { get; set; }

    public string FamilySponsorshipFriendlyName { get; set; }

    public DateTimeOffset? FamilySponsorshipLastSyncDate { get; set; }

    public bool? FamilySponsorshipToDelete { get; set; }

    public DateTimeOffset? FamilySponsorshipValidUntil { get; set; }

    public bool HasPublicAndPrivateKeys { get; set; }

    public Guid Id { get; set; }

    public string Identifier { get; set; }

    private string _Key;
    public string Key
    {
        get => _Key;
        set => throw new NotImplementedException();
    }

    public bool KeyConnectorEnabled { get; set; }

    public string KeyConnectorUrl { get; set; }

    public bool LimitCollectionCreation { get; set; }

    public bool LimitCollectionCreationDeletion { get; set; }

    public bool LimitCollectionDeletion { get; set; }

    public int? MaxCollections { get; set; }

    public int MaxStorageGb { get; set; }

    public string Name { get; set; }

    public ObjectType ObjectType { get; } = ObjectType.ProfileOrganization;

    public Guid OrganizationUserId { get; set; }

    public OrganizationPermissionsModel Permissions { get; set; }

    public int PlanProductType { get; set; }

    public int ProductTierType { get; set; }

    public Guid? ProviderId { get; set; }

    public string ProviderName { get; set; }

    public int? ProviderType { get; set; }

    public bool ResetPasswordEnrolled { get; set; }

    public object Seats { get; set; }

    public bool SelfHost { get; set; }

    public bool SSOBound { get; set; }

    public int Status { get; set; }

    public int Type { get; set; }

    public bool Use2FA { get; set; }

    public bool UseActivateAutofillPolicy { get; set; }

    public bool UseAPI { get; set; }

    public bool UseCustomPermissions { get; set; }

    public bool UseDirectory { get; set; }

    public bool UseEvents { get; set; }

    public bool UseGroups { get; set; }

    public bool UseKeyConnector { get; set; }

    public bool UsePasswordManager { get; set; }

    public bool UsePolicies { get; set; }

    public bool UseResetPassword { get; set; }

    public bool UseSCIM { get; set; }

    public bool UseSecretsManager { get; set; }

    public bool UseSSO { get; set; }

    public bool UseTOTP { get; set; }

    public Guid UserId { get; set; }

    public bool UserIsManagedByOrganization { get; set; }

    public bool UsersGetPremium { get; set; }
    
    public BitWardenDatabase.ProfileItem.Models.OrganizationModel ToDatabaseModel()
    {
        return new BitWardenDatabase.ProfileItem.Models.OrganizationModel
        {
            AccessSecretsManager = AccessSecretsManager,
            AllowAdminAccessToAllCollectionItems = AllowAdminAccessToAllCollectionItems,
            Enabled = Enabled,
            FamilySponsorshipAvailable = FamilySponsorshipAvailable,
            FamilySponsorshipFriendlyName = FamilySponsorshipFriendlyName,
            FamilySponsorshipLastSyncDate = FamilySponsorshipLastSyncDate,
            FamilySponsorshipToDelete = FamilySponsorshipToDelete,
            FamilySponsorshipValidUntil = FamilySponsorshipValidUntil,
            HasPublicAndPrivateKeys = HasPublicAndPrivateKeys,
            Id = Id,
            Identifier = Identifier,
            Key = _Key,
            KeyConnectorEnabled = KeyConnectorEnabled,
            KeyConnectorUrl = KeyConnectorUrl,
            LimitCollectionCreation = LimitCollectionCreation,
            LimitCollectionCreationDeletion = LimitCollectionCreationDeletion,
            LimitCollectionDeletion = LimitCollectionDeletion,
            MaxCollections = MaxCollections,
            MaxStorageGb = MaxStorageGb,
            Name = Name,
            ObjectType = ObjectType,
            OrganizationUserId = OrganizationUserId,
            Permissions = Permissions?.ToDatabaseModel(),
            PlanProductType = PlanProductType,
            ProductTierType = ProductTierType,
            ProviderId = ProviderId,
            ProviderName = ProviderName,
            ProviderType = ProviderType,
            ResetPasswordEnrolled = ResetPasswordEnrolled,
            Seats = Seats,
            SelfHost = SelfHost,
            SSOBound = SSOBound,
            Status = Status,
            Type = Type,
            Use2FA = Use2FA,
            UseActivateAutofillPolicy = UseActivateAutofillPolicy,
            UseAPI = UseAPI,
            UseCustomPermissions = UseCustomPermissions,
            UseDirectory = UseDirectory,
            UseEvents = UseEvents,
            UseGroups = UseGroups,
            UseKeyConnector = UseKeyConnector,
            UsePasswordManager = UsePasswordManager,
            UsePolicies = UsePolicies,
            UseResetPassword = UseResetPassword,
            UseSCIM = UseSCIM,
            UseSecretsManager = UseSecretsManager,
            UseSSO = UseSSO,
            UseTOTP = UseTOTP,
            UserId = UserId,
            UserIsManagedByOrganization = UserIsManagedByOrganization,
            UsersGetPremium = UsersGetPremium,
        };
    }
  }
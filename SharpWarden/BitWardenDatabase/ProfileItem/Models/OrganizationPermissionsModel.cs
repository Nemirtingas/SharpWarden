using Newtonsoft.Json;

namespace SharpWarden.BitWardenDatabase.ProfileItem.Models;

public class OrganizationPermissionsModel
{
    [JsonProperty("accessEventLogs")]
    public bool AccessEventLogs { get; set; }

    [JsonProperty("accessImportExport")]
    public bool AccessImportExport { get; set; }

    [JsonProperty("accessReports")]
    public bool AccessReports { get; set; }

    [JsonProperty("createNewCollections")]
    public bool CreateNewCollections { get; set; }

    [JsonProperty("deleteAnyCollection")]
    public bool DeleteAnyCollection { get; set; }

    [JsonProperty("editAnyCollection")]
    public bool EditAnyCollection { get; set; }

    [JsonProperty("manageGroups")]
    public bool ManageGroups { get; set; }

    [JsonProperty("managePolicies")]
    public bool ManagePolicies { get; set; }

    [JsonProperty("manageResetPassword")]
    public bool ManageResetPassword { get; set; }

    [JsonProperty("manageScim")]
    public bool ManageSCIM { get; set; }

    [JsonProperty("manageSso")]
    public bool ManageSSO { get; set; }

    [JsonProperty("manageUsers")]
    public bool ManageUsers { get; set; }
}
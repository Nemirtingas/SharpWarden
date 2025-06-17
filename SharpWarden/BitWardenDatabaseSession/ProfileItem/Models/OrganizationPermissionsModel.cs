namespace SharpWarden.BitWardenDatabaseSession.ProfileItem.Models;

public class OrganizationPermissionsModel
{
    public OrganizationPermissionsModel(BitWardenDatabase.ProfileItem.Models.OrganizationPermissionsModel databaseModel)
    {
        AccessEventLogs = databaseModel.AccessEventLogs;
        AccessImportExport = databaseModel.AccessImportExport;
        AccessReports = databaseModel.AccessReports;
        CreateNewCollections = databaseModel.CreateNewCollections;
        DeleteAnyCollection = databaseModel.DeleteAnyCollection;
        EditAnyCollection = databaseModel.EditAnyCollection;
        ManageGroups = databaseModel.ManageGroups;
        ManagePolicies = databaseModel.ManagePolicies;
        ManageResetPassword = databaseModel.ManageResetPassword;
        ManageSCIM = databaseModel.ManageSCIM;
        ManageSSO = databaseModel.ManageSSO;
        ManageUsers = databaseModel.ManageUsers;
    }

    public bool AccessEventLogs { get; set; }

    public bool AccessImportExport { get; set; }

    public bool AccessReports { get; set; }

    public bool CreateNewCollections { get; set; }

    public bool DeleteAnyCollection { get; set; }

    public bool EditAnyCollection { get; set; }

    public bool ManageGroups { get; set; }

    public bool ManagePolicies { get; set; }

    public bool ManageResetPassword { get; set; }

    public bool ManageSCIM { get; set; }

    public bool ManageSSO { get; set; }

    public bool ManageUsers { get; set; }

    public BitWardenDatabase.ProfileItem.Models.OrganizationPermissionsModel ToDatabaseModel()
    {
        return new BitWardenDatabase.ProfileItem.Models.OrganizationPermissionsModel
        {
            AccessEventLogs = AccessEventLogs,
            AccessImportExport = AccessImportExport,
            AccessReports = AccessReports,
            CreateNewCollections = CreateNewCollections,
            DeleteAnyCollection = DeleteAnyCollection,
            EditAnyCollection = EditAnyCollection,
            ManageGroups = ManageGroups,
            ManagePolicies = ManagePolicies,
            ManageResetPassword = ManageResetPassword,
            ManageSCIM = ManageSCIM,
            ManageSSO = ManageSSO,
            ManageUsers = ManageUsers,
        };
    }
}
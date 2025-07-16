// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;

namespace SharpWarden.BitWardenDatabaseSession.Models.ProfileItem;

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
    public bool ManageScim { get; set; }

    [JsonProperty("manageSso")]
    public bool ManageSso { get; set; }

    [JsonProperty("manageUsers")]
    public bool ManageUsers { get; set; }
}
// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;

namespace SharpWarden.BitWardenDatabaseSession.Models.CollectionItem;

public class UserCollectionPermissionsModel
{
    [JsonProperty("hidePasswords")]
    public bool HidePasswords { get; set; }

    [JsonProperty("id")]
    public Guid? Id { get; set; }

    [JsonProperty("manage")]
    public bool Manage { get; set; }

    [JsonProperty("readOnly")]
    public bool ReadOnly { get; set; }
}
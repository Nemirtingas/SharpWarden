// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

namespace SharpWarden.WebClient.Models;

public abstract class CipherItemBaseRequestAPIModel
{
    [JsonProperty("type")]
    public abstract CipherItemType ItemType { get; }

    [JsonProperty("folderId")]
    public Guid? FolderId { get; set; }

    [JsonProperty("organizationId")]
    public Guid? OrganizationId { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("notes")]
    public string Notes { get; set; }

    [JsonProperty("fields")]
    public List<CustomFieldAPIModel> Fields { get; set; }

    [JsonProperty("favorite")]
    public bool Favorite { get; set; }

    [JsonProperty("lastKnownRevisionDate")]
    public DateTime? LastKnownRevisionDate { get; set; }

    [JsonProperty("reprompt")]
    public CipherRepromptType Reprompt { get; set; }
}
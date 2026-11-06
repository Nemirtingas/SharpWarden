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

    /// <summary>
    /// Id of the user whose keys encrypted the cipher. Required by recent servers
    /// (Vaultwarden rejects create/update requests without it with a 422).
    /// </summary>
    [JsonProperty("encryptedFor")]
    public Guid? EncryptedFor { get; set; }

    /// <summary>
    /// Per-cipher encryption key. Must be sent back on update: the server replaces the stored key
    /// with this value, so omitting it would make a cipher created by another client unreadable.
    /// </summary>
    [JsonProperty("key")]
    public string Key { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("notes")]
    public string Notes { get; set; }

    [JsonProperty("fields")]
    public List<CustomFieldAPIModel> Fields { get; set; }

    [JsonProperty("passwordHistory")]
    public List<PasswordHistoryAPIModel> PasswordHistory { get; set; }

    [JsonProperty("favorite")]
    public bool Favorite { get; set; }

    [JsonProperty("lastKnownRevisionDate")]
    public DateTime? LastKnownRevisionDate { get; set; }

    [JsonProperty("reprompt")]
    public CipherRepromptType Reprompt { get; set; }
}

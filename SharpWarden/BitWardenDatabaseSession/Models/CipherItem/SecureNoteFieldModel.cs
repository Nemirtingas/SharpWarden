// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;

namespace SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

public class SecureNoteFieldModel
{
    // Always 0? The not is stored in the 'notes' field in the item.
    [JsonProperty("type")]
    public SecureNoteType Type { get; set; }
}
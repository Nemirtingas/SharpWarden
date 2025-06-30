// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

namespace SharpWarden.WebClient.Models;

public class CipherLoginUriAPIModel
{
    [JsonProperty("uri")]
    public string Uri { get; set; }
    
    [JsonProperty("uriChecksum")]
    public string UriChecksum { get; set; }

    [JsonProperty("match")]
    public UriMatchType? Match { get; set; }
}
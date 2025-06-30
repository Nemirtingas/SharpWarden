// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;

namespace SharpWarden.BitWardenWebSession.Models;

class PreLoginModel
{
    [JsonProperty("kdf")]
    public KdfType Kdf { get; set; }

    [JsonProperty("kdfIterations")]
    public int KdfIterations { get; set; }

    [JsonProperty("kdfMemory")]
    public int? KdfMemory { get; set; }
    
    [JsonProperty("kdfParallelism")]
    public int? KdfParallelism { get; set; }
}
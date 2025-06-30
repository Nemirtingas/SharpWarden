// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;

namespace SharpWarden.WebClient.Models;

public class CipherItemCreateAttachmentRequestAPIModel
{
    [JsonProperty("adminRequest")]
    public bool AdminRequest { get; set; }
        
    [JsonProperty("fileSize")]
    public long FileSize { get; set; }

    [JsonProperty("fileName")]
    public string FileName { get; set; }

    [JsonProperty("key")]
    public string Key { get; set; }
}
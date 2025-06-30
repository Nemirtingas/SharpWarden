// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

namespace SharpWarden.WebClient.Models;

public class CreateCipherItemAttachmentResponseAPIResultModel : ApiResultModel
{
    [JsonProperty("attachmentId")]
    public string AttachmentId { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("fileUploadType")]
    public FileUploadType FileUploadType { get; set; }

    [JsonProperty("cipherResponse")]
    public CipherItemModel CipherResponse { get; set; }
}
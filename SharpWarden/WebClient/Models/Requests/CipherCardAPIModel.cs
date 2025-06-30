// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;

namespace SharpWarden.WebClient.Models;

public class CipherCardAPIModel
{
    [JsonProperty("cardholderName")]
    public string CardholderName { get; set; }

    [JsonProperty("brand")]
    public string Brand { get; set; }

    [JsonProperty("number")]
    public string Number { get; set; }

    [JsonProperty("expMonth")]
    public string ExpMonth { get; set; }

    [JsonProperty("expYear")]
    public string ExpYear { get; set; }

    [JsonProperty("code")]
    public string Code { get; set; }
}
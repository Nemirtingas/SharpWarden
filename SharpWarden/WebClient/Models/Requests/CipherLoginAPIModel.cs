// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;

namespace SharpWarden.WebClient.Models;

public class CipherLoginAPIModel
{
    [JsonProperty("uris")]
    public List<CipherLoginUriAPIModel> Uris { get; set; }

    [JsonProperty("username")]
    public string Username { get; set; }
        
    [JsonProperty("password")]
    public string Password { get; set; }

    [JsonProperty("passwordRevisionDate")]
    public DateTime? PasswordRevisionDate { get; set; }

    [JsonProperty("totp")]
    public string TOTP { get; set; }

    [JsonProperty("autofillOnPageLoad")]
    public bool? AutofillOnPageLoad { get; set; }
}
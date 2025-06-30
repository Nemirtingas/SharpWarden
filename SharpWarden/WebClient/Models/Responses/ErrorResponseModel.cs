// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;

namespace SharpWarden.WebClient.Models;

public class ErrorResponseModel
{
    [JsonProperty("error")]
    public ErrorType? ErrorType { get; set; }

    [JsonProperty("error_description")]
    public string Description { get; set; }
}

public class ErrorResponseModel<T> : ErrorResponseModel
{
    [JsonProperty("ErrorModel")]
    public T ErrorModel { get; set; }
}
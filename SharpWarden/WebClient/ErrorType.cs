// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using System.Runtime.Serialization;

namespace SharpWarden.WebClient;

public enum ErrorType
{
    Unknown,
    [EnumMember(Value = "device_error")]
    DeviceError,
}
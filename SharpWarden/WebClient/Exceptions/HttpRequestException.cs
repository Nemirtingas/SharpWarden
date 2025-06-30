// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

namespace SharpWarden.WebClient.Exceptions;

class BitWardenHttpRequestException : HttpRequestException
{
    public ErrorType ErrorType;

    public BitWardenHttpRequestException(ErrorType errorType, string message) :
        base(message)
    {
        ErrorType = errorType;
    }
}

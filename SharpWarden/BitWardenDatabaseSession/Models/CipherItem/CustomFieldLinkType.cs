// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

namespace SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

public enum CustomFieldLinkType
{
    // Login custom field link
    LoginUsername = 100,
    LoginPassword = 101,

    // SecureNote custom field link

    // Card custom field link
    CardOwner = 300,
    CardExpirationMonth = 301,
    CardExpirationDate = 302,
    CardVisualCryptogram = 303,
    CardBrand = 304,
    CardNumber = 305,

    // Identity custom field link
    IdentityTitle = 400,
    IdentityMiddleName = 401,
    IdentityAddress1 = 402,
    IdentityAddress2 = 403,
    IdentityAddress3 = 404,
    IdentityCity = 405,
    IdentityState = 406,
    IdentityPostalCode = 407,
    IdentityCountry = 408,
    IdentityCorporation = 409,
    IdentityEMail = 410,
    IdentityPhone = 411,
    IdentitySocialSecurityNumber = 412,
    IdentityUserName = 413,
    IdentityPassportNumber = 414,
    IdentityDrivingLicenseNumber = 415,
    IdentityFirstName = 416,
    IdentityLastName = 417,
    IdentityFirstAndLastName = 418,
}
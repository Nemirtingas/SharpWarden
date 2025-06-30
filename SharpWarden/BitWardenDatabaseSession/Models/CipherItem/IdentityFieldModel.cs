// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Services;

namespace SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

public class IdentityFieldModel : ISessionAware
{
    private IUserCryptoService _CryptoService;

    public IdentityFieldModel()
    {
    }

    public IdentityFieldModel(IUserCryptoService cryptoService)
    {
        SetCryptoService(cryptoService);
    }

    public bool HasSession() => _CryptoService != null;

    public void SetCryptoService(IUserCryptoService cryptoService)
    {
        _CryptoService = cryptoService;

        Address1?.SetCryptoService(_CryptoService);
        Address2?.SetCryptoService(_CryptoService);
        Address3?.SetCryptoService(_CryptoService);
        City?.SetCryptoService(_CryptoService);
        Company?.SetCryptoService(_CryptoService);
        Country?.SetCryptoService(_CryptoService);
        Email?.SetCryptoService(_CryptoService);
        FirstName?.SetCryptoService(_CryptoService);
        LastName?.SetCryptoService(_CryptoService);
        Username?.SetCryptoService(_CryptoService);
        LicenseNumber?.SetCryptoService(_CryptoService);
        MiddleName?.SetCryptoService(_CryptoService);
        PassportNumber?.SetCryptoService(_CryptoService);
        Phone?.SetCryptoService(_CryptoService);
        PostalCode?.SetCryptoService(_CryptoService);
        SSN?.SetCryptoService(_CryptoService);
        State?.SetCryptoService(_CryptoService);
        Title?.SetCryptoService(_CryptoService);
    }

    [JsonProperty("address1")]
    public EncryptedString Address1 { get; set; }

    [JsonProperty("address2")]
    public EncryptedString Address2 { get; set; }

    [JsonProperty("address3")]
    public EncryptedString Address3 { get; set; }

    [JsonProperty("city")]
    public EncryptedString City { get; set; }

    [JsonProperty("company")]
    public EncryptedString Company { get; set; }

    [JsonProperty("country")]
    public EncryptedString Country { get; set; }

    [JsonProperty("email")]
    public EncryptedString Email { get; set; }

    [JsonProperty("firstName")]
    public EncryptedString FirstName { get; set; }

    [JsonProperty("lastName")]
    public EncryptedString LastName { get; set; }

    [JsonProperty("licenseNumber")]
    public EncryptedString LicenseNumber { get; set; }

    [JsonProperty("middleName")]
    public EncryptedString MiddleName { get; set; }

    [JsonProperty("passportNumber")]
    public EncryptedString PassportNumber { get; set; }

    [JsonProperty("phone")]
    public EncryptedString Phone { get; set; }

    [JsonProperty("postalCode")]
    public EncryptedString PostalCode { get; set; }

    [JsonProperty("ssn")]
    public EncryptedString SSN { get; set; }

    [JsonProperty("state")]
    public EncryptedString State { get; set; }

    [JsonProperty("title")]
    public EncryptedString Title { get; set; }

    [JsonProperty("username")]
    public EncryptedString Username { get; set; }
}
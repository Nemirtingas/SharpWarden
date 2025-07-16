// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Services;

namespace SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

public class IdentityFieldModel : ISessionAware
{
    private IUserCryptoService _cryptoService;

    public IdentityFieldModel()
    {
    }

    public IdentityFieldModel(IUserCryptoService cryptoService)
    {
        SetCryptoService(cryptoService);
    }

    public bool HasSession() => _cryptoService != null;

    public void SetCryptoService(IUserCryptoService cryptoService)
    {
        _cryptoService = cryptoService;

        Address1?.SetCryptoService(_cryptoService);
        Address2?.SetCryptoService(_cryptoService);
        Address3?.SetCryptoService(_cryptoService);
        City?.SetCryptoService(_cryptoService);
        Company?.SetCryptoService(_cryptoService);
        Country?.SetCryptoService(_cryptoService);
        Email?.SetCryptoService(_cryptoService);
        FirstName?.SetCryptoService(_cryptoService);
        LastName?.SetCryptoService(_cryptoService);
        Username?.SetCryptoService(_cryptoService);
        LicenseNumber?.SetCryptoService(_cryptoService);
        MiddleName?.SetCryptoService(_cryptoService);
        PassportNumber?.SetCryptoService(_cryptoService);
        Phone?.SetCryptoService(_cryptoService);
        PostalCode?.SetCryptoService(_cryptoService);
        SecuritySocialNumber?.SetCryptoService(_cryptoService);
        State?.SetCryptoService(_cryptoService);
        Title?.SetCryptoService(_cryptoService);
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
    public EncryptedString SecuritySocialNumber { get; set; }

    [JsonProperty("state")]
    public EncryptedString State { get; set; }

    [JsonProperty("title")]
    public EncryptedString Title { get; set; }

    [JsonProperty("username")]
    public EncryptedString Username { get; set; }
}
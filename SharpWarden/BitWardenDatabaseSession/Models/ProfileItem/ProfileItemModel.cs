// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Services;

namespace SharpWarden.BitWardenDatabaseSession.Models.ProfileItem;

public class ProfileItemModel : ISessionAware
{
    private IUserCryptoService _CryptoService;

    public ProfileItemModel()
    {
        Organizations = new();
    }

    public ProfileItemModel(IUserCryptoService cryptoService)
    {
        Organizations = new();

        SetCryptoService(cryptoService);
    }

    public bool HasSession() => _CryptoService != null;

    public void SetCryptoService(IUserCryptoService cryptoService)
    {
        _CryptoService = cryptoService;

        Key?.SetCryptoService(_CryptoService);
        PrivateKey?.SetCryptoService(_CryptoService);
    }

    [JsonProperty("_status")]
    public string _Status { get; set; }

    [JsonProperty("avatarColor")]
    public string AvatarColor { get; set; }

    [JsonProperty("creationDate")]
    public DateTime? CreationDate { get; set; }

    [JsonProperty("culture")]
    public string Culture { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("emailVerified")]
    public bool EmailVerified { get; set; }

    [JsonProperty("forcePasswordReset")]
    public bool ForcePasswordReset { get; set; }

    [JsonProperty("id")]
    public Guid? Id { get; set; }

    /// <summary>
    /// Don't update this unless you know what you're doing! It will invalidate your whole database!
    /// </summary>
    [JsonProperty("key")]
    public EncryptedString Key { get; set; }

    [JsonProperty("masterPasswordHint")]
    public string MasterPasswordHint { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("object")]
    public ObjectType ObjectType { get; set; } = ObjectType.Profile;

    [JsonProperty("organizations")]
    public List<OrganizationModel> Organizations { get; set; } = new();

    [JsonProperty("premium")]
    public bool Premium { get; set; }

    [JsonProperty("premiumFromOrganization")]
    public bool PremiumFromOrganization { get; set; }

    [JsonProperty("privateKey")]
    public EncryptedString PrivateKey { get; set; }

    [JsonProperty("providerOrganizations")]
    public List<object> ProviderOrganizations { get; set; }

    [JsonProperty("providers")]
    public List<object> Providers { get; set; }

    [JsonProperty("securityStamp")]
    public string SecurityStamp { get; set; }

    [JsonProperty("twoFactorEnabled")]
    public bool TwoFactorEnabled { get; set; }

    [JsonProperty("usesKeyConnector")]
    public bool UsesKeyConnector { get; set; }
 }
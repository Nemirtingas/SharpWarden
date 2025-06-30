// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Services;

namespace SharpWarden.BitWardenDatabaseSession.Models.CollectionItem;

public class CollectionItemModel : ISessionAware
{
    private ConditionalCryptoService _ConditionalCryptoService;

    public CollectionItemModel()
    {
    }

    public CollectionItemModel(IUserCryptoService cryptoService)
    {
        SetCryptoService(cryptoService);
    }

    public bool HasSession() => _ConditionalCryptoService != null;

    public void SetCryptoService(IUserCryptoService cryptoService)
    {
        if (_ConditionalCryptoService == null)
        {
            _ConditionalCryptoService = new ConditionalCryptoService(cryptoService.KeyProviderService, () => this.OrganizationId);
        }
        _ConditionalCryptoService.CryptoService = cryptoService;

        Name?.SetCryptoService(_ConditionalCryptoService);
    }

    [JsonProperty("externalId")]
    public Guid? ExternalId { get; set; }

    [JsonProperty("hidePasswords")]
    public bool HidePasswords { get; set; }

    [JsonProperty("id")]
    public Guid? Id { get; set; }

    [JsonProperty("manage")]
    public bool Manage { get; set; }

    [JsonProperty("name")]
    public EncryptedString Name { get; set; }

    [JsonProperty("object")]
    public ObjectType ObjectType { get; set; } = ObjectType.CollectionDetails;

    [JsonProperty("organizationId")]
    public Guid? OrganizationId { get; set; }

    [JsonProperty("readOnly")]
    public bool ReadOnly { get; set; }
}
// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

namespace SharpWarden.BitWardenDatabaseSession.Services;

public class DefaultOrganizationCryptoFactoryService : IOrganizationCryptoFactoryService
{
    private readonly IKeyProviderService _keyProviderService;
    private readonly Dictionary<Guid, IUserCryptoService> _cache = new();

    public DefaultOrganizationCryptoFactoryService(IKeyProviderService keyProviderService)
    {
        _keyProviderService = keyProviderService;
    }

    public IUserCryptoService GetOrganizationCryptoService(Guid organizationId)
    {
        if (!_cache.TryGetValue(organizationId, out var organizationService))
        {
            _keyProviderService.GetUserKeys(organizationId);
            organizationService = new OrganizationCryptoService(_keyProviderService, organizationId);
            _cache.Add(organizationId, organizationService);
        }

        return organizationService;
    }
}
// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

namespace SharpWarden.BitWardenDatabaseSession.Services;

public class DefaultOrganizationCryptoFactoryService : IOrganizationCryptoFactoryService
{
    private readonly IKeyProviderService _KeyProviderService;
    private readonly Dictionary<Guid, IUserCryptoService> _Cache = new();

    public DefaultOrganizationCryptoFactoryService(IKeyProviderService keyProviderService)
    {
        _KeyProviderService = keyProviderService;
    }

    public IUserCryptoService GetOrganizationCryptoService(Guid organizationId)
    {
        if (!_Cache.TryGetValue(organizationId, out var organizationService))
        {
            _KeyProviderService.GetUserKeys(organizationId);
            organizationService = new OrganizationCryptoService(_KeyProviderService, organizationId);
            _Cache.Add(organizationId, organizationService);
        }

        return organizationService;
    }
}
// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Microsoft.Extensions.DependencyInjection;
using SharpWarden.BitWardenDatabaseSession.Services;
using SharpWarden.NotificationClient.Services;
using SharpWarden.WebClient.Services;

namespace SharpWarden;

public static class BitWardenHelper
{
    private static IServiceScope _CreateSessionScope(string hostBase, string notificationUri, Guid? webClientId)
    {
        var services = new ServiceCollection();

        var hostUriBase = new Uri(hostBase);

        services.AddScoped<ISessionJsonConverterService, DefaultSessionJsonConverterService>();
        services.AddScoped<IKeyProviderService, DefaultKeyProviderService>();
        services.AddScoped<ICryptoService, DefaultCryptoService>();
        services.AddScoped<IUserCryptoService, UserCryptoService>();
        services.AddScoped<IVaultService, DefaultVaultService>();
        services.AddScoped<IOrganizationCryptoFactoryService, DefaultOrganizationCryptoFactoryService>();
        services.AddScoped<IWebClientService, DefaultWebClientService>((services) =>
        {
            return new DefaultWebClientService(services.GetRequiredService<ISessionJsonConverterService>(), hostBase, null, webClientId);
        });
        services.AddScoped<INotificationClientService, DefaultNotificationClientService>((services) =>
        {
            var webClientService = services.GetRequiredService<IWebClientService>();
            return new DefaultNotificationClientService(notificationUri, webClientService.GetWebSession().AccessToken);
        });

        return services.BuildServiceProvider().CreateScope();
    }

    public static IServiceScope CreateSessionScope(string hostBase, string notificationUri)
        => _CreateSessionScope(hostBase, notificationUri, null);

    public static IServiceScope CreateSessionScope(string hostBase, string notificationUri, Guid webClientId)
        => _CreateSessionScope(hostBase, notificationUri, webClientId);
}
using Microsoft.Extensions.DependencyInjection;
using SharpWarden.BitWardenDatabaseSession.Services;
using SharpWarden.WebClient.Services;

namespace SharpWarden;

public static class BitWardenHelper
{
    private static IServiceScope _CreateSessionScope(string hostBase, Guid? webClientId)
    {
        var services = new ServiceCollection();

        services.AddScoped<ISessionJsonConverterService, DefaultSessionJsonConverterService>();
        services.AddScoped<IKeyProviderService, DefaultKeyProviderService>();
        services.AddScoped<ICryptoService, DefaultCryptoService>();
        services.AddScoped<IUserCryptoService, UserCryptoService>();
        services.AddScoped<IVaultService, DefaultVaultService>();
        services.AddScoped<IWebClientService, DefaultWebClientService>((services) =>
        {
            return new DefaultWebClientService(services.GetRequiredService<ISessionJsonConverterService>(), hostBase, null, webClientId);
        });

        return services.BuildServiceProvider().CreateScope();
    }

    public static IServiceScope CreateSessionScope(string hostBase)
        => _CreateSessionScope(hostBase, null);

    public static IServiceScope CreateSessionScope(string hostBase, Guid webClientId)
        => _CreateSessionScope(hostBase, webClientId);
}
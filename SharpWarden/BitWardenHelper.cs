using Microsoft.Extensions.DependencyInjection;
using SharpWarden.BitWardenDatabaseSession.Services;

namespace SharpWarden;

public static class BitWardenHelper
{
    public static IServiceScope CreateSessionScope(string hostBase)
    {
        var services = new ServiceCollection();

        services.AddScoped<ISessionJsonConverterService, DefaultSessionJsonConverterService>();
        services.AddScoped<IKeyProviderService, DefaultKeyProviderService>();
        services.AddScoped<ICryptoService, DefaultCryptoService>();
        services.AddScoped<IUserCryptoService, UserCryptoService>();
        services.AddScoped<IVaultService, DefaultVaultService>();
        services.AddScoped((services) => new WebClient.WebClient(services.GetRequiredService<ISessionJsonConverterService>(), hostBase));

        return services.BuildServiceProvider().CreateScope();
    }
}
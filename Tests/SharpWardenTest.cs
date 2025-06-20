using Microsoft.Extensions.DependencyInjection;
using SharpWarden.BitWardenDatabaseSession.Services;
using System.Text;

namespace Tests
{
    [TestClass]
    public sealed class SharpWardenTest
    {
        private static IServiceScope DatabaseSessionScope;
        private static SharpWarden.WebClient.WebClient VaultWebClient;
        private static IVaultService VaultService;
        private static IUserCryptoService UserCryptoService;
        private static string TestAccountEmail;
        private static string TestAccountPassword;


        static SharpWardenTest()
        {
            DatabaseSessionScope = SharpWarden.BitWardenHelper.CreateSessionScope("https://vault.bitwarden.eu");
            VaultWebClient = DatabaseSessionScope.ServiceProvider.GetRequiredService<SharpWarden.WebClient.WebClient>();
            VaultService = DatabaseSessionScope.ServiceProvider.GetRequiredService<IVaultService>();
            UserCryptoService = DatabaseSessionScope.ServiceProvider.GetRequiredService<IUserCryptoService>();
            TestAccountEmail = Environment.GetEnvironmentVariable("SharpWardenTestAccountEmail");
            TestAccountPassword = Environment.GetEnvironmentVariable("SharpWardenTestAccountPassword");
        }

        [TestMethod]
        public async Task _0001_TestPrelogin()
        {
            await VaultWebClient.PreloginAsync(TestAccountPassword);

            // Authenticating with credentials triggers a mail notification.
            // TODO: API key authentication
            //await VaultWebClient.AuthenticateAsync(password);
            //await VaultWebClient.AuthenticateWithRefreshTokenAsync(refreshToken);

            //VaultService.LoadBitWardenDatabase(
            //    Encoding.UTF8.GetBytes(password),
            //    VaultWebClient.UserKdfIterations,
            //    await VaultWebClient.GetDatabaseAsync());
        }
    }
}

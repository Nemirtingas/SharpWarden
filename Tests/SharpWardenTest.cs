using Microsoft.Extensions.DependencyInjection;
using SharpWarden.BitWardenDatabaseSession.Services;
using System.Text;

namespace Tests
{
    [TestClass]
    public sealed class SharpWardenTest
    {
        const string SharpWardenTestItemId = "1de1488c-f980-446a-bb5a-b30201265744";
        const string SharpWardenTestFolderId = "01c15505-4683-48c2-a3fb-b302013ba7a0";
        const string SharpWardenTestItemInFolderId = "348270df-0c8e-4e05-8997-b30201427190";

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
            TestAccountEmail = Environment.GetEnvironmentVariable("SHARP_WARDEN_TEST_ACCOUNT_NAME");
            TestAccountPassword = Environment.GetEnvironmentVariable("SHARP_WARDEN_TEST_ACCOUNT_PASSWORD");
        }

        [TestMethod]
        public async Task _0001_TestCredentialsLogin()
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(TestAccountEmail));
            Assert.IsFalse(string.IsNullOrWhiteSpace(TestAccountPassword));

            await VaultWebClient.PreloginAsync(TestAccountEmail);

            // Authenticating with credentials triggers a mail notification.
            // TODO: API key authentication
            await VaultWebClient.AuthenticateAsync(TestAccountPassword);
            //await VaultWebClient.AuthenticateWithRefreshTokenAsync(refreshToken);

            
        }

        [TestMethod]
        public async Task _0002_TestLoadDatabaseAsync()
        {
            VaultService.LoadBitWardenDatabase(
                Encoding.UTF8.GetBytes(TestAccountPassword),
                VaultWebClient.UserKdfIterations,
                await VaultWebClient.GetDatabaseAsync());
        }

        [TestMethod]
        public async Task _0003_TestCipherItemAsync()
        {
            var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(SharpWardenTestItemId));

            Assert.IsNotNull(item);
            Assert.AreEqual(item.Id.ToString(), SharpWardenTestItemId);
            Assert.AreEqual(item.Name.ClearString, "SharpWardenTestItem");
            Assert.AreEqual(item.Login.Username.ClearString, "TestUserName");
            Assert.AreEqual(item.Login.Password.ClearString, "TestPassword");
        }

        [TestMethod]
        public async Task _0004_TestFolderItemAsync()
        {
            var folder = VaultService.GetBitWardenDatabase().Folders.Find(e => e.Id == Guid.Parse(SharpWardenTestFolderId));

            Assert.IsNotNull(folder);
            Assert.AreEqual(folder.Id.ToString(), SharpWardenTestFolderId);
            Assert.AreEqual(folder.Name.ClearString, "SharpWardenTestFolder");
        }

        [TestMethod]
        public async Task _0005_TestItemInFolderAsync()
        {
            var item = VaultService.GetBitWardenDatabase().Items.Find(e => e.Id == Guid.Parse(SharpWardenTestItemInFolderId));

            Assert.IsNotNull(item);
            Assert.AreEqual(item.Id.ToString(), SharpWardenTestItemInFolderId);
            Assert.AreEqual(item.Name.ClearString, "SharpWardenTestItemInFolder");
            Assert.AreEqual(item.Login.Username.ClearString, "FolderUser");
            Assert.AreEqual(item.Login.Password.ClearString, "FolderPassword");
            Assert.AreEqual(item.FolderId, Guid.Parse(SharpWardenTestFolderId));
        }
    }
}

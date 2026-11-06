using System.Security.Cryptography;
using System.Text;
using SharpWarden;
using SharpWarden.BitWardenDatabaseSession.Models.CipherItem;
using SharpWarden.BitWardenDatabaseSession.Services;

namespace Tests;

/// <summary>
/// Offline tests of ciphers that have their own encryption key ("key" field), using a fake account
/// encrypted the same way as Bitwarden does.
/// </summary>
[TestClass]
public sealed class CipherItemKeyTest
{
    private const string Email = "user@example.com";
    private const string Password = "correct horse battery staple";
    private const int KdfIterations = BitWardenCipherService.MinimumPbkdf2Iterations;

    private static string Encrypt(string clear, byte[] key) => BitWardenCipherService.EncryptStringWithAesCbc256HmacSha256Base64(clear, key[..32], key[32..]);

    private static string Decrypt(string cipherString, byte[] key) => BitWardenCipherService.DecryptStringWithAesCbc256HmacSha256Base64(cipherString, key[..32], key[32..]);

    private sealed class FakeAccount
    {
        public required byte[] UserKey { get; init; }
        public required DefaultSessionJsonConverterService Json { get; init; }
    }

    private static FakeAccount CreateAccount()
    {
        var masterKey = BitWardenCipherService.ComputeMasterKey(Encoding.UTF8.GetBytes(Email), Encoding.UTF8.GetBytes(Password), KdfIterations);
        var stretchedEnc = BitWardenCipherService.HKDFSHA256(masterKey, "enc"u8.ToArray(), 32);
        var stretchedMac = BitWardenCipherService.HKDFSHA256(masterKey, "mac"u8.ToArray(), 32);

        var userKey = RandomNumberGenerator.GetBytes(64);
        using var rsa = RSA.Create(2048);

        var protectedUserKey = BitWardenCipherService.EncryptWithAesCbc256HmacSha256Base64(userKey, stretchedEnc, stretchedMac);
        var protectedPrivateKey = BitWardenCipherService.EncryptWithAesCbc256HmacSha256Base64(rsa.ExportPkcs8PrivateKey(), userKey[..32], userKey[32..]);

        var keyProvider = new DefaultKeyProviderService();
        keyProvider.LoadBitWardenUserKey(Encoding.UTF8.GetBytes(Email), Encoding.UTF8.GetBytes(Password), KdfIterations, protectedUserKey, protectedPrivateKey);

        return new FakeAccount
        {
            UserKey = userKey,
            Json = new DefaultSessionJsonConverterService(new UserCryptoService(keyProvider)),
        };
    }

    private static string SecureNoteJson(string name, string key, string attachmentKey = null)
    {
        var keyJson = key == null ? "null" : $"\"{key}\"";
        var attachments = attachmentKey == null ? "null" : $"[{{\"id\":\"a1\",\"key\":\"{attachmentKey}\",\"fileName\":\"{name}\"}}]";
        return $"{{\"id\":\"{Guid.NewGuid()}\",\"type\":2,\"name\":\"{name}\",\"key\":{keyJson},\"secureNote\":{{\"type\":0}},\"attachments\":{attachments}}}";
    }

    [TestMethod]
    public void CipherWithoutKeyUsesUserKey()
    {
        var account = CreateAccount();

        var item = account.Json.Deserialize<CipherItemModel>(SecureNoteJson(Encrypt("Plain item", account.UserKey), null));

        Assert.AreEqual("Plain item", item.Name.ClearString);
    }

    [TestMethod]
    public void CipherWithKeyIsDecryptedWithItsOwnKey()
    {
        var account = CreateAccount();
        var cipherKey = RandomNumberGenerator.GetBytes(64);
        var protectedCipherKey = BitWardenCipherService.EncryptWithAesCbc256HmacSha256Base64(cipherKey, account.UserKey[..32], account.UserKey[32..]);

        var item = account.Json.Deserialize<CipherItemModel>(SecureNoteJson(Encrypt("Keyed item", cipherKey), protectedCipherKey));

        Assert.AreEqual("Keyed item", item.Name.ClearString);
    }

    [TestMethod]
    public void CipherWithKeyIsEncryptedWithItsOwnKey()
    {
        var account = CreateAccount();
        var cipherKey = RandomNumberGenerator.GetBytes(64);
        var protectedCipherKey = BitWardenCipherService.EncryptWithAesCbc256HmacSha256Base64(cipherKey, account.UserKey[..32], account.UserKey[32..]);

        var item = account.Json.Deserialize<CipherItemModel>(SecureNoteJson(Encrypt("Keyed item", cipherKey), protectedCipherKey));
        item.Name.ClearString = "Renamed";

        // Other clients decrypt the fields with the cipher key: it must not be encrypted with the user key.
        Assert.AreEqual("Renamed", Decrypt(item.Name.CipherString, cipherKey));
        Assert.ThrowsExactly<CryptographicException>(() => Decrypt(item.Name.CipherString, account.UserKey));
    }

    [TestMethod]
    public void AttachmentOfKeyedCipherUsesCipherKey()
    {
        var account = CreateAccount();
        var cipherKey = RandomNumberGenerator.GetBytes(64);
        var attachmentKey = RandomNumberGenerator.GetBytes(64);
        var protectedCipherKey = BitWardenCipherService.EncryptWithAesCbc256HmacSha256Base64(cipherKey, account.UserKey[..32], account.UserKey[32..]);
        var protectedAttachmentKey = BitWardenCipherService.EncryptWithAesCbc256HmacSha256Base64(attachmentKey, cipherKey[..32], cipherKey[32..]);

        var item = account.Json.Deserialize<CipherItemModel>(SecureNoteJson(Encrypt("file.txt", cipherKey), protectedCipherKey, protectedAttachmentKey));

        CollectionAssert.AreEqual(attachmentKey, item.Attachments[0].Key.ClearBytes);
        Assert.AreEqual("file.txt", item.Attachments[0].FileName.ClearString);
    }

    [TestMethod]
    public void CipherKeyWithWrongLengthIsRejected()
    {
        var account = CreateAccount();
        var shortKey = RandomNumberGenerator.GetBytes(32);
        var protectedShortKey = BitWardenCipherService.EncryptWithAesCbc256HmacSha256Base64(shortKey, account.UserKey[..32], account.UserKey[32..]);

        var item = account.Json.Deserialize<CipherItemModel>(SecureNoteJson(Encrypt("x", account.UserKey), protectedShortKey));

        Assert.ThrowsExactly<CryptographicException>(() => item.Name.ClearString);
    }
}

using System.Security.Cryptography;
using System.Text;
using SharpWarden;
using SharpWarden.BitWardenDatabaseSession.Services;
using SharpWarden.BitWardenWebSession;

namespace Tests;

/// <summary>
/// Offline tests of the key derivation functions (PBKDF2 and Argon2id).
/// </summary>
[TestClass]
public sealed class KdfTest
{
    [TestMethod]
    public void Argon2idMatchesReferenceVector()
    {
        // Reference implementation test vector (phc-winner-argon2, src/test.c): Argon2id v1.3, t=2, m=2^16 KiB, p=1.
        var hash = BitWardenCipherService.Argon2id("password"u8.ToArray(), "somesalt"u8.ToArray(), 2, 65536, 1, 32);

        Assert.AreEqual("09316115d5cf24ed5a15a31a3ba326e5cf32edc24702987c02b6566f61913cf7", Convert.ToHexStringLower(hash));
    }

    [TestMethod]
    public void Argon2idMasterKeyUsesHashedEmailAndMemoryInMiB()
    {
        var email = "user@example.com"u8.ToArray();
        var password = "password"u8.ToArray();

        var masterKey = BitWardenCipherService.ComputeMasterKey(email, password, KdfParameters.Argon2id(3, 16, 2));

        var expected = BitWardenCipherService.Argon2id(password, SHA256.HashData(email), 3, 16 * 1024, 2, 32);
        CollectionAssert.AreEqual(expected, masterKey);
    }

    [TestMethod]
    public void Pbkdf2OverloadsAreEquivalent()
    {
        var email = "user@example.com"u8.ToArray();
        var password = "password"u8.ToArray();

        CollectionAssert.AreEqual(
            BitWardenCipherService.ComputeMasterKey(email, password, 600000),
            BitWardenCipherService.ComputeMasterKey(email, password, KdfParameters.Pbkdf2(600000)));
    }

    [TestMethod]
    [DataRow(1, 64, 4)]      // Too few iterations.
    [DataRow(11, 64, 4)]     // Too many iterations.
    [DataRow(3, 8, 4)]       // Too little memory.
    [DataRow(3, 4096, 4)]    // Too much memory (memory exhaustion from a malicious server).
    [DataRow(3, 64, 0)]      // No parallelism.
    [DataRow(3, 64, 64)]     // Too much parallelism.
    public void UnsafeArgon2idParametersAreRejected(int iterations, int memoryMiB, int parallelism)
    {
        Assert.ThrowsExactly<CryptographicException>(() => KdfParameters.Argon2id(iterations, memoryMiB, parallelism).Validate());
    }

    [TestMethod]
    public void Argon2idWithoutMemoryIsRejected()
    {
        Assert.ThrowsExactly<CryptographicException>(() => new KdfParameters { Type = KdfType.Argon2id, Iterations = 3 }.Validate());
    }

    [TestMethod]
    public void UnknownKdfIsRejected()
    {
        Assert.ThrowsExactly<NotSupportedException>(() => new KdfParameters { Type = (KdfType)42, Iterations = 600000 }.Validate());
    }

    [TestMethod]
    public void Argon2idAccountCanBeUnlocked()
    {
        const string email = "user@example.com";
        const string password = "correct horse battery staple";
        var kdf = KdfParameters.Argon2id(3, 16, 2);

        // Protect a user key the way Bitwarden does for an Argon2id account.
        var masterKey = BitWardenCipherService.ComputeMasterKey(Encoding.UTF8.GetBytes(email), Encoding.UTF8.GetBytes(password), kdf);
        var stretchedEnc = BitWardenCipherService.HKDFSHA256(masterKey, "enc"u8.ToArray(), 32);
        var stretchedMac = BitWardenCipherService.HKDFSHA256(masterKey, "mac"u8.ToArray(), 32);
        var userKey = RandomNumberGenerator.GetBytes(64);
        using var rsa = RSA.Create(2048);
        var protectedUserKey = BitWardenCipherService.EncryptWithAesCbc256HmacSha256Base64(userKey, stretchedEnc, stretchedMac);
        var protectedPrivateKey = BitWardenCipherService.EncryptWithAesCbc256HmacSha256Base64(rsa.ExportPkcs8PrivateKey(), userKey[..32], userKey[32..]);

        var keyProvider = new DefaultKeyProviderService();
        keyProvider.LoadBitWardenUserKey(Encoding.UTF8.GetBytes(email), Encoding.UTF8.GetBytes(password), kdf, protectedUserKey, protectedPrivateKey);

        CollectionAssert.AreEqual(userKey[..32], keyProvider.GetUserKeys(null).SymmetricKey);

        // The same account opened as PBKDF2 must fail (wrong master key).
        Assert.ThrowsExactly<CryptographicException>(() => new DefaultKeyProviderService().LoadBitWardenUserKey(
            Encoding.UTF8.GetBytes(email), Encoding.UTF8.GetBytes(password), 600000, protectedUserKey, protectedPrivateKey));
    }
}

using System.Security.Cryptography;
using System.Text;
using SharpWarden;

namespace Tests;

/// <summary>
/// Offline tests of the cryptographic primitives (no Bitwarden account required).
/// </summary>
[TestClass]
public sealed class BitWardenCipherServiceTest
{
    private static (byte[] Enc, byte[] Mac) NewKeys() => (RandomNumberGenerator.GetBytes(32), RandomNumberGenerator.GetBytes(32));

    [TestMethod]
    public void AesRoundTrip()
    {
        var (enc, mac) = NewKeys();
        var cipherString = BitWardenCipherService.EncryptStringWithAesCbc256HmacSha256Base64("Hello SharpWarden", enc, mac);

        Assert.StartsWith("2.", cipherString);
        Assert.AreEqual("Hello SharpWarden", BitWardenCipherService.DecryptStringWithAesCbc256HmacSha256Base64(cipherString, enc, mac));
    }

    [TestMethod]
    public void AesTamperedCiphertextIsRejected()
    {
        var (enc, mac) = NewKeys();
        var cipherString = BitWardenCipherService.EncryptStringWithAesCbc256HmacSha256Base64("Hello SharpWarden", enc, mac);

        var parts = cipherString[2..].Split('|');
        var data = Convert.FromBase64String(parts[1]);
        data[0] ^= 1;
        var tampered = $"2.{parts[0]}|{Convert.ToBase64String(data)}|{parts[2]}";

        Assert.ThrowsExactly<CryptographicException>(() => BitWardenCipherService.DecryptWithAesCbc256HmacSha256Base64(tampered, enc, mac));
    }

    [TestMethod]
    public void AesWrongMacKeyIsRejected()
    {
        var (enc, mac) = NewKeys();
        var cipherString = BitWardenCipherService.EncryptStringWithAesCbc256HmacSha256Base64("Hello SharpWarden", enc, mac);

        Assert.ThrowsExactly<CryptographicException>(() => BitWardenCipherService.DecryptWithAesCbc256HmacSha256Base64(cipherString, enc, RandomNumberGenerator.GetBytes(32)));
    }

    [TestMethod]
    [DataRow("2.")]
    [DataRow("2.AAAA")]
    [DataRow("2.AAAA|AAAA")]
    [DataRow("2.!!!!|AAAA|AAAA")]
    [DataRow("4.AAAA")]
    [DataRow("garbage")]
    public void AesMalformedCipherStringIsRejected(string cipherString)
    {
        var (enc, mac) = NewKeys();
        Assert.ThrowsExactly<InvalidDataException>(() => BitWardenCipherService.DecryptWithAesCbc256HmacSha256Base64(cipherString, enc, mac));
    }

    [TestMethod]
    public void AesInvalidKeyLengthIsRejected()
    {
        Assert.ThrowsExactly<CryptographicException>(() => BitWardenCipherService.EncryptWithAesCbc256HmacSha256Base64([1, 2, 3], new byte[16], new byte[32]));
    }

    [TestMethod]
    public void HkdfMatchesRfc5869Expand()
    {
        // Reference: hand written HKDF-Expand (RFC 5869, section 2.3).
        var prk = RandomNumberGenerator.GetBytes(32);
        var info = "enc"u8.ToArray();

        using var hmac = new HMACSHA256(prk);
        var t1 = hmac.ComputeHash([.. info, 1]);
        var t2 = hmac.ComputeHash([.. t1, .. info, 2]);
        var expected = t1.Concat(t2).ToArray();

        CollectionAssert.AreEqual(expected, BitWardenCipherService.HKDFSHA256(prk, info, 64));
        CollectionAssert.AreEqual(expected[..32], BitWardenCipherService.HKDFSHA256(prk, info, 32));
    }

    [TestMethod]
    public void MasterKeyRejectsWeakKdfIterations()
    {
        Assert.ThrowsExactly<CryptographicException>(() => BitWardenCipherService.ComputeMasterKey("user@example.com"u8.ToArray(), "password"u8.ToArray(), 1));
        Assert.HasCount(32, BitWardenCipherService.ComputeMasterKey("user@example.com"u8.ToArray(), "password"u8.ToArray(), BitWardenCipherService.MinimumPbkdf2Iterations));
    }

    [TestMethod]
    public async Task AttachmentRoundTrip()
    {
        var key = RandomNumberGenerator.GetBytes(64);
        var clear = RandomNumberGenerator.GetBytes(200_000);

        using var encrypted = new MemoryStream();
        await BitWardenCipherService.EncryptWithAesCbc256HmacSha256Base64ByteStreamAsync(new MemoryStream(clear), encrypted, key);

        encrypted.Position = 0;
        using var decrypted = new MemoryStream();
        await BitWardenCipherService.DecryptWithAesCbc256HmacSha256Base64ByteStreamAsync(encrypted, decrypted, key);

        CollectionAssert.AreEqual(clear, decrypted.ToArray());
    }

    [TestMethod]
    public async Task TamperedAttachmentReleasesNoPlaintext()
    {
        var key = RandomNumberGenerator.GetBytes(64);
        var clear = Encoding.UTF8.GetBytes(new string('A', 100_000));

        using var encrypted = new MemoryStream();
        await BitWardenCipherService.EncryptWithAesCbc256HmacSha256Base64ByteStreamAsync(new MemoryStream(clear), encrypted, key);

        var bytes = encrypted.ToArray();
        bytes[^1] ^= 1;

        using var decrypted = new MemoryStream();
        await Assert.ThrowsExactlyAsync<CryptographicException>(() => BitWardenCipherService.DecryptWithAesCbc256HmacSha256Base64ByteStreamAsync(new MemoryStream(bytes), decrypted, key));
        Assert.AreEqual(0L, decrypted.Length);
    }

    [TestMethod]
    public async Task AttachmentEncryptionCanBeCancelled()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => BitWardenCipherService.EncryptWithAesCbc256HmacSha256Base64ByteStreamAsync(
            new MemoryStream(new byte[1000]), new MemoryStream(), RandomNumberGenerator.GetBytes(64), cts.Token));
    }

    [TestMethod]
    public void RsaRoundTrip()
    {
        using var rsa = RSA.Create(2048);
        var cipherString = BitWardenCipherService.EncryptStringWithRsa2048OaepSha1Base64("secret", rsa.ExportSubjectPublicKeyInfo());

        Assert.StartsWith("4.", cipherString);
        Assert.AreEqual("secret", BitWardenCipherService.DecryptStringWithRsa2048OaepSha1Base64(cipherString, rsa.ExportPkcs8PrivateKey()));
    }

    [TestMethod]
    public void RsaKeyGenerationRejectsWeakKeySize()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => BitWardenHelper.GenerateRsaKey(1024));
    }
}

// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Argon2Parameters = Org.BouncyCastle.Crypto.Parameters.Argon2Parameters;
using SharpWarden.BitWardenWebSession;
using Org.BouncyCastle.Security;

namespace SharpWarden;

public static class BitWardenCipherService
{
    public class HmacAppendingStream : Stream
    {
        private readonly Stream _inner;
        private readonly HMAC _hmac;

        public HmacAppendingStream(Stream inner, HMAC hmac)
        {
            _inner = inner;
            _hmac = hmac;
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => _inner.Length;
        public override long Position { get => _inner.Position; set => throw new NotSupportedException(); }

        public override void Flush() => _inner.Flush();

        public override Task FlushAsync(CancellationToken cancellationToken) => _inner.FlushAsync(cancellationToken);

        public override void Write(byte[] buffer, int offset, int count)
        {
            _hmac.TransformBlock(buffer, offset, count, null, 0);
            _inner.Write(buffer, offset, count);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            _hmac.TransformBlock(buffer, offset, count, null, 0);
            await _inner.WriteAsync(buffer.AsMemory(offset, count), cancellationToken).ConfigureAwait(false);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            _hmac.TransformBlock(buffer.ToArray(), 0, buffer.Length, null, 0);
            _inner.Write(buffer);
        }

        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            _hmac.TransformBlock(buffer.ToArray(), 0, buffer.Length, null, 0);
            await _inner.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
        }

        // Not supported
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }

    public enum BitWardenCipherType
    {
        Unknown = -1,
        // Symmetric encryption types
        AesCbc256B64 = 0,
        // Type 1 was the unused and removed AesCbc128_HmacSha256_B64
        AesCbc256HmacSha256B64 = 2,
        // Cose is the encoding for the key used, but contained can be:
        // - XChaCha20Poly1305
        CoseEncrypt0 = 7,

        // Asymmetric encryption types. These never occur in the same places that the symmetric ones would
        // and can be split out into a separate enum.
        Rsa2048OaepSha256B64 = 3,
        Rsa2048OaepSha1B64 = 4,
        Rsa2048OaepSha256HmacSha256B64 = 5,
        Rsa2048OaepSha1HmacSha256B64 = 6,
    }

    /// <summary>
    /// Minimum PBKDF2 iteration count accepted when deriving the master key.
    /// Prevents a malicious or compromised server from downgrading the KDF to a trivially brute-forceable value.
    /// </summary>
    public const int MinimumPbkdf2Iterations = KdfParameters.MinimumPbkdf2Iterations;

    private const int AesKeySize = 32;
    private const int MacKeySize = 32;
    private const int IvSize = 16;
    private const int MacSize = 32;
    private const int StreamBufferSize = 81920;

    /// <summary>
    /// Derives the master key with PBKDF2-SHA256.
    /// </summary>
    public static byte[] ComputeMasterKey(byte[] userMail, byte[] userPassword, int kdfIterations)
        => ComputeMasterKey(userMail, userPassword, KdfParameters.Pbkdf2(kdfIterations));

    /// <summary>
    /// Derives the master key from the password, using the email as salt (SHA-256 of the email for Argon2id).
    /// </summary>
    public static byte[] ComputeMasterKey(byte[] userMail, byte[] userPassword, KdfParameters kdf)
    {
        ArgumentNullException.ThrowIfNull(userMail);
        ArgumentNullException.ThrowIfNull(userPassword);
        ArgumentNullException.ThrowIfNull(kdf);

        kdf.Validate();

        if (kdf.Type == KdfType.Argon2id)
            return Argon2id(userPassword, SHA256.HashData(userMail), kdf.Iterations, kdf.Memory.Value * 1024, kdf.Parallelism.Value, 32);

        return PBKDF2SHA256(userPassword, userMail, kdf.Iterations, 32);
    }

    /// <summary>
    /// Argon2id (version 0x13) as specified by RFC 9106.
    /// </summary>
    public static byte[] Argon2id(byte[] password, byte[] salt, int iterations, int memoryKiB, int parallelism, int length)
    {
        var parameters = new Argon2Parameters.Builder(Argon2Parameters.Argon2id)
            .WithVersion(Argon2Parameters.Version13)
            .WithIterations(iterations)
            .WithMemoryAsKB(memoryKiB)
            .WithParallelism(parallelism)
            .WithSalt(salt)
            .Build();

        var generator = new Argon2BytesGenerator();
        generator.Init(parameters);

        var result = new byte[length];
        generator.GenerateBytes(password, result);
        return result;
    }

    public static byte[] ComputeUserPasswordHash(byte[] userMasterKey, byte[] userPassword)
    {
        return PBKDF2SHA256(userMasterKey, userPassword, 1, 32);
    }

    public static byte[] PBKDF2SHA256(byte[] passwordBytes, byte[] salt, int iterations, int length)
    {
        return Rfc2898DeriveBytes.Pbkdf2(passwordBytes, salt, iterations, HashAlgorithmName.SHA256, length);
    }

    /// <summary>
    /// HKDF-Expand (RFC 5869) with SHA-256, using <paramref name="inputKey"/> as the pseudo random key.
    /// </summary>
    public static byte[] HKDFSHA256(byte[] inputKey, byte[] info, int length)
    {
        return HKDF.Expand(HashAlgorithmName.SHA256, inputKey, length, info);
    }

    private static void _CheckSymmetricKeys(byte[] encKey, byte[] macKey)
    {
        if (encKey == null || encKey.Length != AesKeySize)
            throw new CryptographicException("Invalid encryption key length.");

        if (macKey == null || macKey.Length != MacKeySize)
            throw new CryptographicException("Invalid MAC key length.");
    }

    private static void _CheckAttachmentKey(byte[] attachmentKey)
    {
        if (attachmentKey == null || attachmentKey.Length != AesKeySize + MacKeySize)
            throw new CryptographicException("Invalid attachment key length.");
    }

    private static byte[] _FromBase64(string value)
    {
        try
        {
            return Convert.FromBase64String(value);
        }
        catch (FormatException ex)
        {
            throw new InvalidDataException("Invalid base64 data in cipher string.", ex);
        }
    }

    private static byte[] _ParseMac(string value)
    {
        try
        {
            return Convert.FromBase64String(value);
        }
        catch (FormatException)
        {
            // Legacy workaround kept for compatibility with some malformed MAC encodings.
            // The result is still verified against the computed MAC in constant time.
            var pp = value.Split('/');
            pp[0] += "/";
            return _FromBase64(string.Join('/', pp));
        }
    }

    /// <summary>
    /// Decrypts a Bitwarden encrypted attachment.
    /// The whole ciphertext is authenticated before any plaintext is written to <paramref name="decryptedAttachment"/>,
    /// so <paramref name="attachment"/> must be seekable.
    /// </summary>
    public static async Task DecryptWithAesCbc256HmacSha256Base64ByteStreamAsync(Stream attachment, Stream decryptedAttachment, byte[] attachmentKey, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(attachment);
        ArgumentNullException.ThrowIfNull(decryptedAttachment);
        _CheckAttachmentKey(attachmentKey);

        if (!attachment.CanSeek)
            throw new NotSupportedException("The encrypted attachment stream must be seekable.");

        var keyEnc = attachmentKey[..AesKeySize];
        var keyMac = attachmentKey[AesKeySize..];

        var startPosition = attachment.Position;

        var type = (BitWardenCipherType)attachment.ReadByte();
        if (type != BitWardenCipherType.AesCbc256HmacSha256B64)
            throw new InvalidDataException("Unexpected cipher type.");

        var ciphertextLength = attachment.Length - startPosition - 1 - IvSize - MacSize;
        if (ciphertextLength <= 0 || ciphertextLength % 16 != 0)
            throw new InvalidDataException("Invalid attachment size.");

        var iv = new byte[IvSize];
        attachment.ReadExactly(iv);

        var macExpected = new byte[MacSize];
        attachment.ReadExactly(macExpected);

        var ciphertextPosition = attachment.Position;

        byte[] buffer = new byte[StreamBufferSize];
        int bytesRead;

        // First pass: authenticate the ciphertext before releasing any plaintext (encrypt-then-MAC).
        using (var hmac = IncrementalHash.CreateHMAC(HashAlgorithmName.SHA256, keyMac))
        {
            hmac.AppendData(iv);
            while ((bytesRead = await attachment.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
                hmac.AppendData(buffer, 0, bytesRead);

            if (!CryptographicOperations.FixedTimeEquals(hmac.GetHashAndReset(), macExpected))
                throw new CryptographicException("MAC mismatch - corrupted file or invalid key.");
        }

        // Second pass: decrypt.
        attachment.Position = ciphertextPosition;

        using var aes = Aes.Create();
        using var decryptor = aes.CreateDecryptor(keyEnc, iv);
        await using var cryptoStream = new CryptoStream(decryptedAttachment, decryptor, CryptoStreamMode.Write, leaveOpen: true);

        while ((bytesRead = await attachment.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
            await cryptoStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);

        await cryptoStream.FlushFinalBlockAsync(cancellationToken).ConfigureAwait(false);
    }

    public static async Task EncryptWithAesCbc256HmacSha256Base64ByteStreamAsync(Stream input, Stream output, byte[] attachmentKey, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);
        _CheckAttachmentKey(attachmentKey);

        if (!output.CanSeek)
            throw new NotSupportedException("The output stream must be seekable.");

        var keyEnc = attachmentKey[..AesKeySize];
        var keyMac = attachmentKey[AesKeySize..];

        byte[] iv = RandomNumberGenerator.GetBytes(IvSize);

        output.WriteByte((byte)BitWardenCipherType.AesCbc256HmacSha256B64);

        await output.WriteAsync(iv, cancellationToken).ConfigureAwait(false);

        long macPosition = output.Position;
        await output.WriteAsync(new byte[MacSize], cancellationToken).ConfigureAwait(false); // Placeholder

        using var hmac = new HMACSHA256(keyMac);
        hmac.TransformBlock(iv, 0, iv.Length, null, 0);

        using var aes = Aes.Create();
        using var encryptor = aes.CreateEncryptor(keyEnc, iv);
        await using (var cryptoStream = new CryptoStream(new HmacAppendingStream(output, hmac), encryptor, CryptoStreamMode.Write, leaveOpen: true))
        {
            byte[] buffer = new byte[StreamBufferSize];
            int bytesRead;
            while ((bytesRead = await input.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
                await cryptoStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
        }

        hmac.TransformFinalBlock([], 0, 0);

        var endPosition = output.Position;
        output.Position = macPosition;
        await output.WriteAsync(hmac.Hash.AsMemory(0, MacSize), cancellationToken).ConfigureAwait(false);
        output.Position = endPosition;
    }

    public static byte[] DecryptWithAesCbc256HmacSha256Base64(string cipherString, byte[] stretchedEncKey, byte[] stretchedMacKey)
    {
        ArgumentNullException.ThrowIfNull(cipherString);
        _CheckSymmetricKeys(stretchedEncKey, stretchedMacKey);

        var parts = cipherString.Split('.', 2);

        if (parts.Length != 2 || !Enum.TryParse<BitWardenCipherType>(parts[0], out var cipherType) || cipherType != BitWardenCipherType.AesCbc256HmacSha256B64)
            throw new InvalidDataException("Unexpected cipher type.");

        var innerParts = parts[1].Split('|');
        if (innerParts.Length != 3)
            throw new InvalidDataException("Invalid cipher string format.");

        var iv = _FromBase64(innerParts[0]);
        var ciphertext = _FromBase64(innerParts[1]);
        var mac = _ParseMac(innerParts[2]);

        if (iv.Length != IvSize || mac.Length != MacSize || ciphertext.Length == 0 || ciphertext.Length % 16 != 0)
            throw new InvalidDataException("Invalid cipher string format.");

        using (var hmac = IncrementalHash.CreateHMAC(HashAlgorithmName.SHA256, stretchedMacKey))
        {
            hmac.AppendData(iv);
            hmac.AppendData(ciphertext);

            // Constant time comparison to avoid leaking MAC information through timing.
            if (!CryptographicOperations.FixedTimeEquals(hmac.GetHashAndReset(), mac))
                throw new CryptographicException("MAC mismatch - invalid password or data.");
        }

        using var aes = Aes.Create();
        aes.Key = stretchedEncKey;
        return aes.DecryptCbc(ciphertext, iv, PaddingMode.PKCS7);
    }

    public static string DecryptStringWithAesCbc256HmacSha256Base64(string cipherString, byte[] stretchedEncKey, byte[] stretchedMacKey)
    {
        return Encoding.UTF8.GetString(DecryptWithAesCbc256HmacSha256Base64(cipherString, stretchedEncKey, stretchedMacKey));
    }

    public static string EncryptStringWithAesCbc256HmacSha256Base64(string plainText, byte[] stretchedEncKey, byte[] stretchedMacKey)
    {
        return EncryptWithAesCbc256HmacSha256Base64(Encoding.UTF8.GetBytes(plainText), stretchedEncKey, stretchedMacKey);
    }

    public static string EncryptWithAesCbc256HmacSha256Base64(byte[] plainBytes, byte[] stretchedEncKey, byte[] stretchedMacKey)
    {
        ArgumentNullException.ThrowIfNull(plainBytes);
        _CheckSymmetricKeys(stretchedEncKey, stretchedMacKey);

        byte[] iv = RandomNumberGenerator.GetBytes(IvSize);

        using var aes = Aes.Create();
        aes.Key = stretchedEncKey;
        byte[] ciphertext = aes.EncryptCbc(plainBytes, iv, PaddingMode.PKCS7);

        // Encrypt-then-MAC: HMAC over IV + ciphertext.
        byte[] mac;
        using (var hmac = IncrementalHash.CreateHMAC(HashAlgorithmName.SHA256, stretchedMacKey))
        {
            hmac.AppendData(iv);
            hmac.AppendData(ciphertext);
            mac = hmac.GetHashAndReset();
        }

        return $"{(int)BitWardenCipherType.AesCbc256HmacSha256B64}.{Convert.ToBase64String(iv)}|{Convert.ToBase64String(ciphertext)}|{Convert.ToBase64String(mac)}";
    }

    public static byte[] DecryptWithRsa2048OaepSha1Base64(string cipherString, byte[] privateKeyDer)
    {
        ArgumentNullException.ThrowIfNull(cipherString);
        ArgumentNullException.ThrowIfNull(privateKeyDer);

        string[] parts = cipherString.Split('.', 2);

        if (parts.Length != 2 || !Enum.TryParse<BitWardenCipherType>(parts[0], out var cipherType) || cipherType != BitWardenCipherType.Rsa2048OaepSha1B64)
            throw new InvalidDataException("Unexpected cipher type.");

        string[] innerParts = parts[1].Split('|');
        if (innerParts.Length != 1)
            throw new InvalidDataException("Expected a single component for RSA encrypted string.");

        byte[] ciphertext = _FromBase64(innerParts[0]);

        AsymmetricKeyParameter privateKey = PrivateKeyFactory.CreateKey(privateKeyDer);
        IAsymmetricBlockCipher engine = new OaepEncoding(new RsaEngine());

        engine.Init(false, privateKey);
        return engine.ProcessBlock(ciphertext, 0, ciphertext.Length);
    }

    public static string DecryptStringWithRsa2048OaepSha1Base64(string cipherString, byte[] privateKeyDer)
    {
        return Encoding.UTF8.GetString(DecryptWithRsa2048OaepSha1Base64(cipherString, privateKeyDer));
    }
    
    public static string EncryptStringWithRsa2048OaepSha1Base64(string plainText, byte[] publicKeyDer)
    {
        return EncryptWithRsa2048OaepSha1Base64(Encoding.UTF8.GetBytes(plainText), publicKeyDer);
    }

    public static string EncryptWithRsa2048OaepSha1Base64(byte[] plainBytes, byte[] publicKeyDer)
    {
        ArgumentNullException.ThrowIfNull(plainBytes);
        ArgumentNullException.ThrowIfNull(publicKeyDer);

        AsymmetricKeyParameter publicKey = PublicKeyFactory.CreateKey(publicKeyDer);
        IAsymmetricBlockCipher engine = new OaepEncoding(new RsaEngine());

        engine.Init(true, publicKey);
        return $"{(int)BitWardenCipherType.Rsa2048OaepSha1B64}.{Convert.ToBase64String(engine.ProcessBlock(plainBytes, 0, plainBytes.Length))}";
    }
}
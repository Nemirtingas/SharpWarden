// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
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
            await _inner.WriteAsync(buffer.AsMemory(offset, count), cancellationToken);
        }

        // Not supported
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => _inner.SetLength(value);
    }

    public enum BitWardenCipherType
    {
        Unknown = -1,
        // Symmetric encryption types
        AesCbc256_B64 = 0,
        // Type 1 was the unused and removed AesCbc128_HmacSha256_B64
        AesCbc256_HmacSha256_B64 = 2,
        // Cose is the encoding for the key used, but contained can be:
        // - XChaCha20Poly1305
        CoseEncrypt0 = 7,

        // Asymmetric encryption types. These never occur in the same places that the symmetric ones would
        // and can be split out into a separate enum.
        Rsa2048_OaepSha256_B64 = 3,
        Rsa2048_OaepSha1_B64 = 4,
        Rsa2048_OaepSha256_HmacSha256_B64 = 5,
        Rsa2048_OaepSha1_HmacSha256_B64 = 6,
    }

    public static byte[] ComputeMasterKey(byte[] userMail, byte[] userPassword, int kdfIterations)
    {
        return PBKDF2_SHA256(userPassword, userMail, kdfIterations, 32);
    }

    public static byte[] ComputeUserPasswordHash(byte[] userMasterKey, byte[] userPassword)
    {
        return PBKDF2_SHA256(userMasterKey, userPassword, 1, 32);
    }

    public static byte[] PBKDF2_SHA256(byte[] passwordBytes, byte[] salt, int iterations, int length)
    {
        using var deriveBytes = new Rfc2898DeriveBytes(passwordBytes, salt, iterations, HashAlgorithmName.SHA256);
        return deriveBytes.GetBytes(length);
    }

    public static byte[] HKDF_SHA256(byte[] inputKey, byte[] info, int length)
    {
        using var hmac = new HMACSHA256(inputKey);
        byte[] prk = hmac.ComputeHash(Array.Empty<byte>());

        byte[] okm = new byte[length];
        byte[] previous = new byte[0];
        int bytesFilled = 0;
        byte counter = 1;

        while (bytesFilled < length)
        {
            hmac.Initialize();
            hmac.Key = inputKey;

            hmac.TransformBlock(previous, 0, previous.Length, null, 0);
            hmac.TransformBlock(info, 0, info.Length, null, 0);
            hmac.TransformFinalBlock(new byte[] { counter }, 0, 1);

            previous = hmac.Hash;
            int toCopy = Math.Min(previous.Length, length - bytesFilled);
            Array.Copy(previous, 0, okm, bytesFilled, toCopy);
            bytesFilled += toCopy;
            counter++;
        }

        return okm;
    }

    public static async Task DecryptWithAesCbc256HmacSha256Base64ByteStreamAsync(Stream attachment, Stream decryptedAttachment, byte[] attachmentKey)
    {
        var keyEnc = attachmentKey[..32];
        var keyMac = attachmentKey[32..];

        // Lire l’en-tête
        var type = (BitWardenCipherType)attachment.ReadByte();
        if (type != BitWardenCipherType.AesCbc256_HmacSha256_B64)
            throw new InvalidDataException("Unexpected cipher type.");

        var ciphertextLength = attachment.Length - 1 - 16 - 32;
        if (ciphertextLength <= 0)
            throw new InvalidDataException("Attachment too small.");

        var iv = new byte[16];
        attachment.ReadExactly(iv);

        var macExpected = new byte[32];
        attachment.ReadExactly(macExpected);

        using var hmac = new HMACSHA256(keyMac);
        hmac.TransformBlock(iv, 0, iv.Length, null, 0); // MAC = HMAC(iv + ciphertext)

        using var aes = Aes.Create();
        aes.Key = keyEnc;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        using var cryptoStream = new CryptoStream(decryptedAttachment, decryptor, CryptoStreamMode.Write, leaveOpen: true);

        const int bufferSize = 8192;
        byte[] buffer = new byte[bufferSize];
        int bytesRead;

        while ((bytesRead = await attachment.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            hmac.TransformBlock(buffer, 0, bytesRead, null, 0);
            await cryptoStream.WriteAsync(buffer, 0, bytesRead);
        }

        hmac.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

        if (!hmac.Hash.SequenceEqual(macExpected))
            throw new CryptographicException("MAC mismatch – fichier corrompu ou clé invalide.");

        await cryptoStream.FlushAsync();
    }

    public static async Task EncryptWithAesCbc256HmacSha256Base64ByteStreamAsync(Stream input, Stream output, byte[] attachmentKey)
    {
        var keyEnc = attachmentKey[..32];
        var keyMac = attachmentKey[32..];

        byte[] iv = RandomNumberGenerator.GetBytes(16);

        output.WriteByte((byte)BitWardenCipherType.AesCbc256_HmacSha256_B64);

        await output.WriteAsync(iv, 0, iv.Length);

        long macPosition = output.Position;
        await output.WriteAsync(new byte[32]); // Placeholder

        using var hmac = new HMACSHA256(keyMac);
        hmac.TransformBlock(iv, 0, iv.Length, null, 0);

        using var aes = Aes.Create();
        aes.Key = keyEnc;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        using (var cryptoStream = new CryptoStream(new HmacAppendingStream(output, hmac), encryptor, CryptoStreamMode.Write, leaveOpen: true))
        {
            const int bufferSize = 8192;
            byte[] buffer = new byte[bufferSize];
            int bytesRead;
            while ((bytesRead = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await cryptoStream.WriteAsync(buffer, 0, bytesRead);
            }
        }

        hmac.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

        output.Position = macPosition;
        await output.WriteAsync(hmac.Hash, 0, 32);
    }

    public static byte[] DecryptWithAesCbc256HmacSha256Base64(string cipherString, byte[] stretchedEncKey, byte[] stretchedMacKey)
    {
        var parts = cipherString.Split('.', 2);

        if (parts.Length != 2 || !Enum.TryParse<BitWardenCipherType>(parts[0], out var cipherType) || cipherType != BitWardenCipherType.AesCbc256_HmacSha256_B64)
            throw new InvalidDataException("Unexpected cipher type.");

        var innerParts = parts[1].Split('|');
        var iv = Convert.FromBase64String(innerParts[0]);
        var ciphertext = Convert.FromBase64String(innerParts[1]);
        var mac = default(byte[]);
        try
        {
            mac = Convert.FromBase64String(innerParts[2]);
        }
        catch
        {
            var pp = innerParts[2].Split('/');
            pp[0] = pp[0] + "/";
            mac = Convert.FromBase64String(string.Join('/', pp));
        }

        using (var hmac = new HMACSHA256(stretchedMacKey))
        {
            hmac.TransformBlock(iv, 0, iv.Length, null, 0);
            hmac.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
            var computedMac = hmac.Hash;

            if (mac.Length != computedMac.Length || !mac.SequenceEqual(computedMac))
                throw new CryptographicException("MAC mismatch - invalid password or data.");
        }

        using var aes = Aes.Create();
        aes.Key = stretchedEncKey;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
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
        using var aes = Aes.Create();
        aes.Key = stretchedEncKey;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV(); // Génère un IV aléatoire

        byte[] iv = aes.IV;

        using var encryptor = aes.CreateEncryptor();
        byte[] ciphertext = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // HMAC pour l'intégrité (sur IV + ciphertext)
        byte[] mac;
        using (var hmac = new HMACSHA256(stretchedMacKey))
        {
            hmac.TransformBlock(iv, 0, iv.Length, null, 0);
            hmac.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
            mac = hmac.Hash;
        }

        return $"{(int)BitWardenCipherType.AesCbc256_HmacSha256_B64}.{Convert.ToBase64String(iv)}|{Convert.ToBase64String(ciphertext)}|{Convert.ToBase64String(mac)}";
    }

    public static byte[] DecryptWithRsa2048OaepSha1Base64(string cipherString, byte[] privateKeyDer)
    {
        string[] parts = cipherString.Split('.', 2);

        if (parts.Length != 2 || !Enum.TryParse<BitWardenCipherType>(parts[0], out var cipherType) || cipherType != BitWardenCipherType.Rsa2048_OaepSha1_B64)
            throw new InvalidDataException("Unexpected cipher type.");

        string[] innerParts = parts[1].Split('|');
        if (innerParts.Length != 1)
            throw new ArgumentException("Expected a single component for RSA encrypted string.");

        byte[] ciphertext = Convert.FromBase64String(innerParts[0]);

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
        AsymmetricKeyParameter publicKey = PublicKeyFactory.CreateKey(publicKeyDer);
        IAsymmetricBlockCipher engine = new OaepEncoding(new RsaEngine());

        engine.Init(true, publicKey);
        return $"{(int)BitWardenCipherType.Rsa2048_OaepSha1_B64}.{Convert.ToBase64String(engine.ProcessBlock(plainBytes, 0, plainBytes.Length))}";
    }
}
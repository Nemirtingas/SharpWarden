// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using SharpWarden.BitWardenDatabaseSession.Services;
using SharpWarden.Models;
using SharpWarden.NotificationClient.Services;
using SharpWarden.WebClient.Services;
using Org.BouncyCastle.OpenSsl;
using System.Text;
using System.Security.Cryptography;

namespace SharpWarden;

public static class BitWardenHelper
{
    private static IServiceScope _CreateSessionScope(string hostBase, string notificationUri, Guid? webClientId)
    {
        var services = new ServiceCollection();

        services.AddScoped<ISessionJsonConverterService, DefaultSessionJsonConverterService>();
        services.AddScoped<IKeyProviderService, DefaultKeyProviderService>();
        services.AddScoped<ICryptoService, DefaultCryptoService>();
        services.AddScoped<IUserCryptoService, UserCryptoService>();
        services.AddScoped<IVaultService, DefaultVaultService>();
        services.AddScoped<IOrganizationCryptoFactoryService, DefaultOrganizationCryptoFactoryService>();
        services.AddScoped<IWebClientService, DefaultWebClientService>((serviceProvider) =>
        {
            return new DefaultWebClientService(serviceProvider.GetRequiredService<ISessionJsonConverterService>(), hostBase, null, webClientId);
        });
        services.AddScoped<INotificationClientService, DefaultNotificationClientService>((serviceProvider) =>
        {
            return new DefaultNotificationClientService(serviceProvider.GetRequiredService<IWebClientService>(), notificationUri);
        });

        return services.BuildServiceProvider().CreateScope();
    }

    public static IServiceScope CreateSessionScope(string hostBase, string notificationUri)
        => _CreateSessionScope(hostBase, notificationUri, null);

    public static IServiceScope CreateSessionScope(string hostBase, string notificationUri, Guid webClientId)
        => _CreateSessionScope(hostBase, notificationUri, webClientId);

    public static SSHKeyModel GenerateRsaKey(int keySize = 4096, string comment = "")
    {
        var rsaKeyGen = new RsaKeyPairGenerator();
        rsaKeyGen.Init(new KeyGenerationParameters(new SecureRandom(), keySize));
        AsymmetricCipherKeyPair keyPair = rsaKeyGen.GenerateKeyPair();

        var privateKeyPem = ExportPrivateKeyToPem(keyPair.Private);
        var pubKey = (RsaKeyParameters)keyPair.Public;
        var sshPublicKey = ConvertRsaPublicKeyToSsh(pubKey, comment);
        var fingerprint = GetFingerprintFromSshKey(sshPublicKey);

        return new SSHKeyModel
        {
            KeyType = SSHKeyType.RSA,
            PrivateKey = privateKeyPem,
            PublicKey = sshPublicKey,
            Fingerprint = fingerprint
        };
    }

    public static SSHKeyModel GenerateEd25519Key(string comment = "")
    {
        var keyGen = new Ed25519KeyPairGenerator();
        keyGen.Init(new Ed25519KeyGenerationParameters(new SecureRandom()));
        AsymmetricCipherKeyPair keyPair = keyGen.GenerateKeyPair();

        var privateKey = (Ed25519PrivateKeyParameters)keyPair.Private;
        var publicKey = (Ed25519PublicKeyParameters)keyPair.Public;

        var privateKeyOpenSsh = ExportOpenSshEd25519PrivateKey(privateKey, publicKey);
        var sshPublicKey = ConvertEd25519PublicKeyToSsh(publicKey, comment);
        var fingerprint = GetFingerprintFromSshKey(sshPublicKey);

        return new SSHKeyModel
        {
            KeyType = SSHKeyType.ED25519,
            PrivateKey = privateKeyOpenSsh,
            PublicKey = sshPublicKey,
            Fingerprint = fingerprint
        };
    }

    private static string ExportOpenSshEd25519PrivateKey(Ed25519PrivateKeyParameters privateKey, Ed25519PublicKeyParameters publicKey)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms, Encoding.UTF8, true);

        // Header
        writer.Write("openssh-key-v1\0"u8.ToArray());
        WriteSshString(writer, "none");  // ciphername
        WriteSshString(writer, "none");  // kdfname
        WriteSshString(writer, new byte[0]); // kdfoptions
        writer.Write(ToBigEndian(1)); // number of keys

        // Public key block
        using (var pubMs = new MemoryStream())
        using (var pubWriter = new BinaryWriter(pubMs, Encoding.UTF8, true))
        {
            WriteSshString(pubWriter, "ssh-ed25519");
            WriteSshString(pubWriter, publicKey.GetEncoded());
            WriteSshString(writer, pubMs.ToArray());
        }

        // Private key block
        using (var privMs = new MemoryStream())
        using (var privWriter = new BinaryWriter(privMs, Encoding.UTF8, true))
        {
            var rng = new SecureRandom();
            uint checkInt = (uint)rng.NextInt();
            privWriter.Write(ToBigEndian((int)checkInt));
            privWriter.Write(ToBigEndian((int)checkInt));

            WriteSshString(privWriter, "ssh-ed25519");
            WriteSshString(privWriter, publicKey.GetEncoded());

            // private key = 64 bytes: 32 priv + 32 pub concatenated
            byte[] privBytes = new byte[64];
            privateKey.Encode(privBytes, 0);
            Buffer.BlockCopy(publicKey.GetEncoded(), 0, privBytes, 32, 32);
            WriteSshString(privWriter, privBytes);

            WriteSshString(privWriter, Encoding.UTF8.GetBytes(""));

            // Padding to multiple of 8 bytes
            int paddingLen = (int)(8 - (privMs.Length % 8));
            if (paddingLen == 8) paddingLen = 0;
            for (int i = 1; i <= paddingLen; i++)
                privWriter.Write((byte)i);

            WriteSshString(writer, privMs.ToArray());
        }

        var base64 = Convert.ToBase64String(ms.ToArray());
        var sb = new StringBuilder();
        sb.AppendLine("-----BEGIN OPENSSH PRIVATE KEY-----");
        for (int i = 0; i < base64.Length; i += 70)
            sb.AppendLine(base64.Substring(i, Math.Min(70, base64.Length - i)));
        sb.AppendLine("-----END OPENSSH PRIVATE KEY-----");
        return sb.ToString();
    }

    private static string ExportPrivateKeyToPem(AsymmetricKeyParameter privateKey)
    {
        using var sw = new StringWriter();
        var pemWriter = new PemWriter(sw);
        pemWriter.WriteObject(privateKey);
        pemWriter.Writer.Flush();
        return sw.ToString();
    }

    private static string ConvertRsaPublicKeyToSsh(RsaKeyParameters publicKey, string comment)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        WriteSshString(writer, "ssh-rsa");
        WriteSshMpint(writer, publicKey.Exponent.ToByteArrayUnsigned());
        WriteSshMpint(writer, publicKey.Modulus.ToByteArrayUnsigned());

        string base64Key = Convert.ToBase64String(ms.ToArray());
        if (string.IsNullOrWhiteSpace(comment))
            return $"ssh-rsa {base64Key}";
        else
            return $"ssh-rsa {base64Key} {comment}";
    }

    private static string ConvertEd25519PublicKeyToSsh(Ed25519PublicKeyParameters publicKey, string comment)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        WriteSshString(writer, "ssh-ed25519");
        WriteSshString(writer, publicKey.GetEncoded());

        string base64Key = Convert.ToBase64String(ms.ToArray());
        if (string.IsNullOrWhiteSpace(comment))
            return $"ssh-ed25519 {base64Key}";
        else
            return $"ssh-ed25519 {base64Key} {comment}";
    }

    private static void WriteSshString(BinaryWriter writer, string value) => WriteSshString(writer, Encoding.UTF8.GetBytes(value));

    private static void WriteSshString(BinaryWriter writer, byte[] bytes)
    {
        writer.Write(ToBigEndian(bytes.Length));
        writer.Write(bytes);
    }

    private static void WriteSshMpint(BinaryWriter writer, byte[] data)
    {
        if (data.Length > 0 && (data[0] & 0x80) != 0)
        {
            var prefixed = new byte[data.Length + 1];
            Buffer.BlockCopy(data, 0, prefixed, 1, data.Length);
            data = prefixed;
        }
        writer.Write(ToBigEndian(data.Length));
        writer.Write(data);
    }

    private static byte[] ToBigEndian(int value)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        return bytes;
    }

    private static string GetFingerprintFromSshKey(string sshKey)
    {
        var base64 = sshKey.Split(' ')[1];
        var keyBytes = Convert.FromBase64String(base64);
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(keyBytes);
        var fingerprint = Convert.ToBase64String(hash).TrimEnd('=');
        return $"SHA256:{fingerprint}";
    }
}

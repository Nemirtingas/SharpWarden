using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Security;

namespace SharpWarden;

static class BitWardenCipherService
{
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

    public static byte[] DecryptWithMasterKey(string cipherString, byte[] stretchedEncKey, byte[] stretchedMacKey)
    {
        var parts = cipherString.Split('.', 2);

        if (parts.Length != 2 || parts[0] != "2")
            throw new InvalidDataException("Unexpected cipher string type.");

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
            byte[] computedMac = hmac.Hash;

            for (int i = 0; i < mac.Length; i++)
                if (mac[i] != computedMac[i])
                    throw new CryptographicException("MAC mismatch - invalid password or data.");
        }

        // Decryption
        using var aes = Aes.Create();
        aes.Key = stretchedEncKey;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
    }

    public static string DecryptStringWithMasterKey(string cipherString, byte[] stretchedEncKey, byte[] stretchedMacKey)
    {
        return Encoding.UTF8.GetString(DecryptWithMasterKey(cipherString, stretchedEncKey, stretchedMacKey));
    }

    public static string EncryptStringWithMasterKey(string plainText, byte[] stretchedEncKey, byte[] stretchedMacKey)
    {
        return EncryptWithMasterKey(Encoding.UTF8.GetBytes(plainText), stretchedEncKey, stretchedMacKey);
    }

    public static string EncryptWithMasterKey(byte[] plainBytes, byte[] stretchedEncKey, byte[] stretchedMacKey)
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

        return $"2.{Convert.ToBase64String(iv)}|{Convert.ToBase64String(ciphertext)}|{Convert.ToBase64String(mac)}";
    }

    public static byte[] DecryptWithRSA(string cipherString, byte[] privateKeyDer)
    {
        string[] parts = cipherString.Split('.', 2);

        if (parts.Length != 2 || parts[0] != "4")
            throw new InvalidDataException("Unexpected cipher string type.");

        string[] innerParts = parts[1].Split('|');
        if (innerParts.Length != 1)
            throw new ArgumentException("Expected a single component for RSA encrypted string.");

        byte[] ciphertext = Convert.FromBase64String(innerParts[0]);

        AsymmetricKeyParameter privateKey = PrivateKeyFactory.CreateKey(privateKeyDer);
        IAsymmetricBlockCipher engine = new OaepEncoding(new RsaEngine());

        engine.Init(false, privateKey);
        return engine.ProcessBlock(ciphertext, 0, ciphertext.Length);
    }

    public static string DecryptStringWithRSA(string cipherString, byte[] privateKeyDer)
    {
        return Encoding.UTF8.GetString(DecryptWithRSA(cipherString, privateKeyDer));
    }
    
    public static string EncryptStringWithRSA(string plainText, byte[] publicKeyDer)
    {
        return EncryptWithRSA(Encoding.UTF8.GetBytes(plainText), publicKeyDer);
    }

    public static string EncryptWithRSA(byte[] plainBytes, byte[] publicKeyDer)
    {
        AsymmetricKeyParameter publicKey = PublicKeyFactory.CreateKey(publicKeyDer);
        IAsymmetricBlockCipher engine = new OaepEncoding(new RsaEngine());

        engine.Init(true, publicKey);
        return $"4.{Convert.ToBase64String(engine.ProcessBlock(plainBytes, 0, plainBytes.Length))}";
    }
}
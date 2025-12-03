using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Santa.Core.Security
{
    // Lightweight secure storage helper.
    // Mobile-first: uses AES encryption with a device-bound key and stores bytes under Application.persistentDataPath.
    // On Windows, uses DPAPI (ProtectedData) when available for additional protection.
    public static class SecureStorage
    {
        private const string FolderName = "secure";

        // NOTE: For real secrets, consider platform-native keychain/keystore integrations.
        // This helper aims to be safer than plain PlayerPrefs while remaining portable.

        public static void SetString(string key, string value)
        {
            var path = GetPathForKey(key);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var plaintext = Encoding.UTF8.GetBytes(value ?? string.Empty);
            var cipher = Protect(plaintext);
            File.WriteAllBytes(path, cipher);
        }

        public static bool TryGetString(string key, out string value)
        {
            var path = GetPathForKey(key);
            value = null;
            if (!File.Exists(path)) return false;
            try
            {
                var cipher = File.ReadAllBytes(path);
                var plaintext = Unprotect(cipher);
                value = Encoding.UTF8.GetString(plaintext);
                return true;
            }
            catch (CryptographicException e)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning($"SecureStorage: Failed to decrypt '{key}': {e.Message}. Deleting corrupted entry.");
#endif
                // Corrupted or incompatible data; delete to self-heal
                Delete(key);
                return false;
            }
            catch (Exception e)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning($"SecureStorage: Failed to read '{key}': {e.Message}");
#endif
                return false;
            }
        }

        public static void Delete(string key)
        {
            var path = GetPathForKey(key);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private static string GetPathForKey(string key)
        {
            var safeKey = SanitizeKey(key);
            var baseDir = Path.Combine(Application.persistentDataPath, FolderName);
            return Path.Combine(baseDir, safeKey + ".dat");
        }

        private static string SanitizeKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return "key";
            var invalid = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(key.Length);
            foreach (var ch in key)
            {
                sb.Append(Array.IndexOf(invalid, ch) >= 0 ? '_' : ch);
            }
            return sb.ToString();
        }

        private static byte[] Protect(byte[] data)
        {
            // Cross-platform AES encryption (mobile-first).
            using (var aes = Aes.Create())
            {
                aes.Key = DeriveKey(32);
                // Generate a random IV for each encryption
                aes.GenerateIV();
                var iv = aes.IV;

                using (var ms = new MemoryStream())
                {
                    // Prepend the IV to the stream
                    ms.Write(iv, 0, iv.Length);
                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
                        return ms.ToArray();
                    }
                }
            }
        }

        private static byte[] Unprotect(byte[] data)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = DeriveKey(32);

                // Read the IV from the beginning of the data
                var iv = new byte[aes.BlockSize / 8];
                if (data.Length < iv.Length) throw new CryptographicException("Invalid ciphertext length.");
                Buffer.BlockCopy(data, 0, iv, 0, iv.Length);
                aes.IV = iv;

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        // Decrypt the data that comes after the IV
                        cs.Write(data, iv.Length, data.Length - iv.Length);
                        cs.FlushFinalBlock();
                        return ms.ToArray();
                    }
                }
            }
        }

        private static byte[] DeriveKey(int length)
        {
            // Derive per-device key material from deviceUniqueIdentifier plus an obfuscated app-scoped salt.
            var deviceId = SystemInfo.deviceUniqueIdentifier ?? "unknown-device";
            var salt = SecurityConstants.GetStorageSalt();

            // Use PBKDF2 for stronger key derivation. 100,000 iterations meets modern security standards.
            using (var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(deviceId, salt, 100000, System.Security.Cryptography.HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(length);
            }
        }
    }
}

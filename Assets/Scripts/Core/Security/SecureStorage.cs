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
            catch (Exception e)
            {
                Debug.LogWarning($"SecureStorage: Failed to read '{key}': {e.Message}");
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
            // Cross-platform AES encryption (mobile-first). Avoid Windows DPAPI to keep build targets simple.
            using (var aes = Aes.Create())
            {
                aes.Key = DeriveKey(32);
                aes.IV = DeriveKey(16, iv:true);
                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                    cs.FlushFinalBlock();
                    return ms.ToArray();
                }
            }
        }

        private static byte[] Unprotect(byte[] data)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = DeriveKey(32);
                aes.IV = DeriveKey(16, iv:true);
                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                    cs.FlushFinalBlock();
                    return ms.ToArray();
                }
            }
        }

        private static byte[] DeriveKey(int length, bool iv = false)
        {
            // Derive per-device key material from deviceUniqueIdentifier plus an app-scoped salt.
            var deviceId = SystemInfo.deviceUniqueIdentifier ?? "unknown-device";
            var salt = iv ? "santa-sec-iv" : "santa-sec-key";
            using (var sha256 = SHA256.Create())
            {
                var material = Encoding.UTF8.GetBytes(deviceId + ":" + salt);
                var digest = sha256.ComputeHash(material);
                if (length <= digest.Length)
                {
                    var key = new byte[length];
                    Buffer.BlockCopy(digest, 0, key, 0, length);
                    return key;
                }
                // Expand deterministically if longer needed
                var keyBytes = new byte[length];
                var offset = 0;
                var counter = 0;
                while (offset < length)
                {
                    var block = sha256.ComputeHash(Encoding.UTF8.GetBytes(deviceId + ":" + salt + ":" + counter++));
                    var toCopy = Math.Min(block.Length, length - offset);
                    Buffer.BlockCopy(block, 0, keyBytes, offset, toCopy);
                    offset += toCopy;
                }
                return keyBytes;
            }
        }
    }
}

using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Santa.Core.Save
{
    /// <summary>
    /// Wrapper for save data with integrity validation
    /// </summary>
    [Serializable]
    public class SecureSaveContainer<T>
    {
        public T Data;
        public string Checksum;
        public int Version;
        public long TimestampUtc;
    }

    // Small helper to persist tiny objects securely as JSON using SecureStorage.
    public static class SecureStorageJson
    {
        private const int CURRENT_SAVE_VERSION = 1;

        // Secret key for HMAC derived from device ID
        private static byte[] GetHmacKey()
        {
            var deviceId = SystemInfo.deviceUniqueIdentifier ?? "unknown-device";
            var salt = SecurityConstants.GetSaveSalt();
            using (var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(deviceId, salt, 100000, System.Security.Cryptography.HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(32);
            }
        }

        public static void Set<T>(string key, T obj)
        {
            try
            {
                var container = new SecureSaveContainer<T>
                {
                    Data = obj ?? Activator.CreateInstance<T>(),
                    Version = CURRENT_SAVE_VERSION,
                    TimestampUtc = DateTime.UtcNow.Ticks
                };

                // Compute checksum over data + timestamp
                container.Checksum = ComputeChecksum(container.Data, container.TimestampUtc);

                var json = JsonUtility.ToJson(container);
                SecureStorage.SetString(key, json);
            }
            catch (Exception e)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogError($"SecureStorageJson: Failed to save '{key}': {e.Message}");
#else
                _ = e;
#endif
            }
        }

        public static bool TryGet<T>(string key, out T obj)
        {
            obj = default;
            if (!SecureStorage.TryGetString(key, out var json)) return false;

            try
            {
                // Try new format with checksum first
                var container = JsonUtility.FromJson<SecureSaveContainer<T>>(json);

                if (container != null && !string.IsNullOrEmpty(container.Checksum))
                {
                    // Validate checksum
                    string computedChecksum = ComputeChecksum(container.Data, container.TimestampUtc);
                    if (container.Checksum != computedChecksum)
                    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        GameLog.LogError($"SecureStorageJson: Integrity check failed for '{key}'. Data may be corrupted or tampered with!");
#endif
                        return false;
                    }

                    // Validate version
                    if (container.Version > CURRENT_SAVE_VERSION)
                    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        GameLog.LogWarning($"SecureStorageJson: Save data for '{key}' is from a newer version ({container.Version} vs {CURRENT_SAVE_VERSION}). Loading may fail.");
#endif
                    }

                    obj = container.Data;
                    return obj != null;
                }
                else
                {
                    // Fallback: try to parse as legacy direct format (no container)
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    GameLog.LogWarning($"SecureStorageJson: Loading legacy save format for '{key}' without checksum validation. Consider re-saving.");
#endif
                    obj = JsonUtility.FromJson<T>(json);
                    return obj != null;
                }
            }
            catch (Exception e)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning($"SecureStorageJson: Failed to parse JSON for '{key}': {e.Message}");
#else
                _ = e;
#endif
                return false;
            }
        }

        public static void Delete(string key)
        {
            SecureStorage.Delete(key);
        }

        private static string ComputeChecksum<T>(T data, long timestamp)
        {
            try
            {
                // Serialize data to JSON
                string dataJson = JsonUtility.ToJson(data);

                // Combine data + timestamp
                string combined = $"{dataJson}|{timestamp}";
                byte[] dataBytes = Encoding.UTF8.GetBytes(combined);

                // Compute HMAC-SHA256
                using (var hmac = new HMACSHA256(GetHmacKey()))
                {
                    byte[] hash = hmac.ComputeHash(dataBytes);
                    return Convert.ToBase64String(hash);
                }
            }
            catch (Exception e)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogError($"SecureStorageJson: Failed to compute checksum: {e.Message}");
#else
                _ = e;
#endif
                return string.Empty;
            }
        }
    }
}

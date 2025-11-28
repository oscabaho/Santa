using System;
using UnityEngine;

namespace Santa.Core.Security
{
    // Small helper to persist tiny objects securely as JSON using SecureStorage.
    public static class SecureStorageJson
    {
        public static void Set<T>(string key, T obj)
        {
            var json = JsonUtility.ToJson(obj ?? Activator.CreateInstance<T>());
            SecureStorage.SetString(key, json);
        }

        public static bool TryGet<T>(string key, out T obj)
        {
            obj = default;
            if (!SecureStorage.TryGetString(key, out var json)) return false;
            try
            {
                obj = JsonUtility.FromJson<T>(json);
                return obj != null;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"SecureStorageJson: Failed to parse JSON for '{key}': {e.Message}");
                return false;
            }
        }

        public static void Delete(string key)
        {
            SecureStorage.Delete(key);
        }
    }
}

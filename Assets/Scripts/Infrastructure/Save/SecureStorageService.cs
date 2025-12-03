using System;

namespace Santa.Core.Save
{
    /// <summary>
    /// Interface for secure storage operations, allowing for dependency injection and mocking.
    /// </summary>
    public interface ISecureStorageService
    {
        void Save<T>(string key, T data);
        bool TryLoad<T>(string key, out T data);
        void Delete(string key);
    }

    /// <summary>
    /// Concrete implementation of ISecureStorageService that uses the static SecureStorageJson helper.
    /// </summary>
    public class SecureStorageService : ISecureStorageService
    {
        public void Save<T>(string key, T data)
        {
            SecureStorageJson.Set(key, data);
        }

        public bool TryLoad<T>(string key, out T data)
        {
            return SecureStorageJson.TryGet(key, out data);
        }

        public void Delete(string key)
        {
            SecureStorageJson.Delete(key);
        }
    }
}

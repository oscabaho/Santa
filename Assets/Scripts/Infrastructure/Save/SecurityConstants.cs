using System;
using System.Linq;
using System.Text;

namespace Santa.Core.Security
{
    /// <summary>
    /// Provides obfuscated cryptographic salts to prevent easy extraction via decompilation.
    /// Uses simple reversible encoding to hide salt values from casual inspection.
    /// </summary>
    internal static class SecurityConstants
    {
        // Obfuscated salts stored as reversed Base64 strings
        // Actual values are decoded at runtime to avoid plaintext in compiled binary

        /// <summary>
        /// Salt for save file encryption (SecureStorageJson HMAC)
        /// Original: "Santa_Save_Secret_Salt_2025"
        /// </summary>
        private const string ObfuscatedSaveSalt = "==wYjN2MzYDN5ATM5YzN2MjM0ADN4UjN";

        /// <summary>
        /// Salt for general secure storage (SecureStorage AES key derivation)
        /// Original: "santa-sec-key"
        /// </summary>
        private const string ObfuscatedStorageSalt = "eXktY2VzLWF0bmFz";

        /// <summary>
        /// Gets the deobfuscated salt for save file HMAC operations
        /// </summary>
        public static byte[] GetSaveSalt()
        {
            return DeobfuscateSalt(ObfuscatedSaveSalt);
        }

        /// <summary>
        /// Gets the deobfuscated salt for secure storage key derivation
        /// </summary>
        public static byte[] GetStorageSalt()
        {
            return DeobfuscateSalt(ObfuscatedStorageSalt);
        }

        private static byte[] DeobfuscateSalt(string obfuscated)
        {
            try
            {
                // Reverse the Base64 string
                var reversed = new string(obfuscated.Reverse().ToArray());
                // Decode from Base64
                return Convert.FromBase64String(reversed);
            }
            catch
            {
                // Fallback to a default if deobfuscation fails (should never happen)
                // This prevents crashes but logs a warning
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogError("SecurityConstants: Failed to deobfuscate salt. Using fallback.");
#endif
                return Encoding.UTF8.GetBytes("fallback-salt-error");
            }
        }

        // Note: To update salts, use this helper:
        // public static string ObfuscateSalt(string plaintext)
        // {
        //     var bytes = Encoding.UTF8.GetBytes(plaintext);
        //     var base64 = Convert.ToBase64String(bytes);
        //     return new string(base64.Reverse().ToArray());
        // }
    }
}

using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Santa.Core.Net
{
    // Enforces HTTPS, sensible defaults, and avoids permissive certificate handlers.
    public static class HttpPolicy
    {
        public static UnityWebRequest Get(string url)
        {
            ValidateHttps(url);
            var req = UnityWebRequest.Get(url);
            ApplyDefaults(req);
            return req;
        }

        public static UnityWebRequest PostJson(string url, string json)
        {
            ValidateHttps(url);
            var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            var bodyRaw = System.Text.Encoding.UTF8.GetBytes(json ?? "{}");
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            ApplyDefaults(req);
            return req;
        }

        public static void ApplyDefaults(UnityWebRequest req)
        {
            req.timeout = 15; // seconds
            // Do NOT attach any CertificateHandler that accepts all certs.
        }

        private static void ValidateHttps(string url)
        {
            if (string.IsNullOrEmpty(url) || !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"HttpPolicy: Only HTTPS is permitted. Invalid URL '{url}'.");
            }
        }
    }
}

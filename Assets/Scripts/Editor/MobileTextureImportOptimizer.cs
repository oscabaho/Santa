#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Santa.EditorTools
{
    public static class MobileTextureImportOptimizer
    {
        private const int MaxSizeMobile = 1024;
        private const int CompressionQuality = 50; // 0-100

        [MenuItem("Tools/Mobile/Optimize Texture Import Settings (Android + iPhone)")]
        public static void OptimizeAllTextures()
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" });
            int processed = 0;
            try
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    EditorUtility.DisplayProgressBar("Optimizing Textures for Mobile", path, (float)i / guids.Length);

                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null) continue;

                    // Heuristic sRGB off for non-color maps
                    string fileName = Path.GetFileName(path).ToLowerInvariant();
                    if (fileName.Contains("normal") || fileName.Contains("roughness") || fileName.Contains("metallic") || fileName.Contains("ao"))
                    {
                        importer.sRGBTexture = false;
                    }

                    // Android settings (ETC2 automatic + Crunch)
                    var android = new TextureImporterPlatformSettings
                    {
                        name = "Android",
                        maxTextureSize = MaxSizeMobile,
                        overridden = true,
                        textureCompression = TextureImporterCompression.Compressed,
                        compressionQuality = CompressionQuality,
                        crunchedCompression = true,
                        format = TextureImporterFormat.Automatic
                    };
                    importer.SetPlatformTextureSettings(android);

                    // iPhone settings (ASTC automatic, no Crunch)
                    var ios = new TextureImporterPlatformSettings
                    {
                        name = "iPhone",
                        maxTextureSize = MaxSizeMobile,
                        overridden = true,
                        textureCompression = TextureImporterCompression.Compressed,
                        compressionQuality = CompressionQuality,
                        crunchedCompression = false,
                        format = TextureImporterFormat.Automatic
                    };
                    importer.SetPlatformTextureSettings(ios);

                    // Write & reimport
                    AssetDatabase.WriteImportSettingsIfDirty(path);
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    processed++;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            Debug.Log($"[MobileTextureImportOptimizer] Optimized {processed} textures for mobile platforms.");
        }
    }
}
#endif

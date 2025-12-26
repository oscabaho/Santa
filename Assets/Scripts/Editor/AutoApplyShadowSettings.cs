#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;

namespace Santa.Editor
{
    /// <summary>
    /// Aplica automáticamente configuraciones optimizadas para sombras en dispositivos móviles
    /// </summary>
    public static class AutoApplyShadowSettings
    {
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            // Descomenta la siguiente línea si quieres que se aplique automáticamente al abrir Unity
            // ApplyOptimalSettingsToAllAssets();
        }

        [MenuItem("Santa/Auto-Apply Optimal Shadow Settings")]
        public static void ApplyOptimalSettingsToAllAssets()
        {
            string[] guids = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
            int count = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);

                if (asset != null)
                {
                    string assetName = asset.name.ToLower();

                    // Aplicar configuraciones basadas en el tier de calidad
                    if (assetName.Contains("low"))
                    {
                        ApplyLowQualitySettings(asset);
                    }
                    else if (assetName.Contains("medium"))
                    {
                        ApplyMediumQualitySettings(asset);
                    }
                    else if (assetName.Contains("high") || assetName.Contains("ultra") || assetName.Contains("very high"))
                    {
                        ApplyHighQualitySettings(asset);
                    }
                    else
                    {
                        // Configuración por defecto (medium)
                        ApplyMediumQualitySettings(asset);
                    }

                    count++;
                    Debug.Log($"[AutoApplyShadowSettings] Configuración aplicada a: {asset.name}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Shadow Settings Applied",
                $"Configuración óptima de sombras aplicada a {count} URP Assets.\n\n" +
                "Los warnings de sombras deberían desaparecer.",
                "OK");

            Debug.Log($"[AutoApplyShadowSettings] Configuración aplicada a {count} assets");
        }

        private static void ApplyLowQualitySettings(UniversalRenderPipelineAsset asset)
        {
            SerializedObject so = new SerializedObject(asset);

            // Configuración para móviles de gama baja
            SetProperty(so, "m_ShadowAtlasResolution", 1024);
            SetProperty(so, "m_AdditionalLightShadowsSupported", true); // Cambiar a true
            SetProperty(so, "m_AdditionalLightsShadowmapResolution", 256);
            SetProperty(so, "m_AdditionalLightsShadowResolutionTierLow", 128);
            SetProperty(so, "m_AdditionalLightsShadowResolutionTierMedium", 128);
            SetProperty(so, "m_AdditionalLightsShadowResolutionTierHigh", 256);
            SetProperty(so, "m_ShadowDistance", 20f);
            SetProperty(so, "m_SoftShadowsSupported", false);
            SetProperty(so, "m_AdditionalLightsPerObjectLimit", 2);
            SetProperty(so, "m_MainLightShadowsSupported", false); // Desactivar para low

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
        }

        private static void ApplyMediumQualitySettings(UniversalRenderPipelineAsset asset)
        {
            SerializedObject so = new SerializedObject(asset);

            // Configuración para móviles de gama media
            SetProperty(so, "m_ShadowAtlasResolution", 2048);
            SetProperty(so, "m_AdditionalLightShadowsSupported", true);
            SetProperty(so, "m_AdditionalLightsShadowmapResolution", 512);
            SetProperty(so, "m_AdditionalLightsShadowResolutionTierLow", 128);
            SetProperty(so, "m_AdditionalLightsShadowResolutionTierMedium", 256);
            SetProperty(so, "m_AdditionalLightsShadowResolutionTierHigh", 512);
            SetProperty(so, "m_ShadowDistance", 30f);
            SetProperty(so, "m_SoftShadowsSupported", false);
            SetProperty(so, "m_AdditionalLightsPerObjectLimit", 4);
            SetProperty(so, "m_MainLightShadowsSupported", true);
            SetProperty(so, "m_MainLightShadowmapResolution", 2048);

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
        }

        private static void ApplyHighQualitySettings(UniversalRenderPipelineAsset asset)
        {
            SerializedObject so = new SerializedObject(asset);

            // Configuración para móviles de gama alta o testing
            SetProperty(so, "m_ShadowAtlasResolution", 4096);
            SetProperty(so, "m_AdditionalLightShadowsSupported", true);
            SetProperty(so, "m_AdditionalLightsShadowmapResolution", 1024);
            SetProperty(so, "m_AdditionalLightsShadowResolutionTierLow", 256);
            SetProperty(so, "m_AdditionalLightsShadowResolutionTierMedium", 512);
            SetProperty(so, "m_AdditionalLightsShadowResolutionTierHigh", 1024);
            SetProperty(so, "m_ShadowDistance", 50f);
            SetProperty(so, "m_SoftShadowsSupported", false); // Mantener desactivado para móviles
            SetProperty(so, "m_AdditionalLightsPerObjectLimit", 8);
            SetProperty(so, "m_MainLightShadowsSupported", true);
            SetProperty(so, "m_MainLightShadowmapResolution", 4096);

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
        }

        private static void SetProperty(SerializedObject obj, string propertyName, object value)
        {
            SerializedProperty property = obj.FindProperty(propertyName);
            if (property != null)
            {
                switch (property.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        property.intValue = (int)value;
                        break;
                    case SerializedPropertyType.Float:
                        property.floatValue = (float)value;
                        break;
                    case SerializedPropertyType.Boolean:
                        property.boolValue = (bool)value;
                        break;
                }
            }
        }
    }
}
#endif

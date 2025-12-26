#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Reflection;
using System.Collections.Generic;

namespace Santa.Editor
{
    /// <summary>
    /// Editor tool para optimizar configuraciones de sombras en URP Assets para móviles
    /// </summary>
    public class URPShadowOptimizerEditor : EditorWindow
    {
        private bool applyToAllQualityLevels = true;
        private int shadowAtlasSize = 2048;
        private int additionalLightsShadowResolution = 512;
        private int shadowResolutionTierLow = 128;
        private int shadowResolutionTierMedium = 256;
        private int shadowResolutionTierHigh = 512;
        private float shadowDistance = 30f;
        private bool enableSoftShadows = false;
        private bool enableAdditionalLightsShadows = true;
        private int maxAdditionalLightsPerObject = 4;

        [MenuItem("Santa/Optimize URP Shadows for Mobile")]
        public static void ShowWindow()
        {
            var window = GetWindow<URPShadowOptimizerEditor>("URP Shadow Optimizer");
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("URP Shadow Optimizer for Mobile", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Esta herramienta optimiza automáticamente las configuraciones de sombras en tus URP Assets para dispositivos móviles.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Configuraciones
            applyToAllQualityLevels = EditorGUILayout.Toggle("Aplicar a todos los niveles de calidad", applyToAllQualityLevels);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Shadow Atlas", EditorStyles.boldLabel);
            shadowAtlasSize = EditorGUILayout.IntSlider("Atlas Size", shadowAtlasSize, 256, 4096);
            EditorGUILayout.HelpBox($"Tamaño recomendado: 2048x2048 para móviles de gama media/alta", MessageType.None);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Additional Lights Shadow Resolutions", EditorStyles.boldLabel);
            additionalLightsShadowResolution = EditorGUILayout.IntSlider("Base Resolution", additionalLightsShadowResolution, 128, 2048);
            shadowResolutionTierLow = EditorGUILayout.IntSlider("Tier Low", shadowResolutionTierLow, 128, 512);
            shadowResolutionTierMedium = EditorGUILayout.IntSlider("Tier Medium", shadowResolutionTierMedium, 128, 1024);
            shadowResolutionTierHigh = EditorGUILayout.IntSlider("Tier High", shadowResolutionTierHigh, 128, 2048);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Shadow Settings", EditorStyles.boldLabel);
            shadowDistance = EditorGUILayout.Slider("Shadow Distance", shadowDistance, 10f, 100f);
            enableSoftShadows = EditorGUILayout.Toggle("Enable Soft Shadows", enableSoftShadows);
            EditorGUILayout.HelpBox("Soft Shadows pueden reducir el rendimiento en móviles. Recomendado: desactivado", 
                enableSoftShadows ? MessageType.Warning : MessageType.Info);

            enableAdditionalLightsShadows = EditorGUILayout.Toggle("Enable Additional Lights Shadows", enableAdditionalLightsShadows);
            maxAdditionalLightsPerObject = EditorGUILayout.IntSlider("Max Additional Lights Per Object", maxAdditionalLightsPerObject, 1, 8);

            EditorGUILayout.Space(10);

            // Botones de acción
            if (GUILayout.Button("Apply to Current URP Asset", GUILayout.Height(30)))
            {
                ApplyToCurrentURPAsset();
            }

            if (GUILayout.Button("Apply to All URP Assets in Project", GUILayout.Height(30)))
            {
                ApplyToAllURPAssets();
            }

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Analyze Current Scene Lights", GUILayout.Height(25)))
            {
                AnalyzeSceneLights();
            }

            if (GUILayout.Button("Convert Point Lights to Spot Lights in Scene", GUILayout.Height(25)))
            {
                ConvertPointLightsInScene();
            }

            if (GUILayout.Button("Disable Soft Shadows in Scene", GUILayout.Height(25)))
            {
                DisableSoftShadowsInScene();
            }
        }

        private void ApplyToCurrentURPAsset()
        {
            var pipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (pipeline == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontró un URP Asset activo en Graphics Settings", "OK");
                return;
            }

            ApplySettingsToAsset(pipeline);
            EditorUtility.DisplayDialog("Success", "Configuración aplicada al URP Asset actual", "OK");
        }

        private void ApplyToAllURPAssets()
        {
            string[] guids = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
            int count = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
                
                if (asset != null)
                {
                    ApplySettingsToAsset(asset);
                    count++;
                }
            }

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Success", $"Configuración aplicada a {count} URP Assets", "OK");
        }

        private void ApplySettingsToAsset(UniversalRenderPipelineAsset asset)
        {
            SerializedObject serializedObject = new SerializedObject(asset);

            // Shadow Atlas
            SetProperty(serializedObject, "m_ShadowAtlasResolution", shadowAtlasSize);

            // Additional Lights Shadows
            SetProperty(serializedObject, "m_AdditionalLightShadowsSupported", enableAdditionalLightsShadows);
            SetProperty(serializedObject, "m_AdditionalLightsShadowmapResolution", additionalLightsShadowResolution);
            SetProperty(serializedObject, "m_AdditionalLightsShadowResolutionTierLow", shadowResolutionTierLow);
            SetProperty(serializedObject, "m_AdditionalLightsShadowResolutionTierMedium", shadowResolutionTierMedium);
            SetProperty(serializedObject, "m_AdditionalLightsShadowResolutionTierHigh", shadowResolutionTierHigh);

            // Shadow Distance
            SetProperty(serializedObject, "m_ShadowDistance", shadowDistance);

            // Soft Shadows
            SetProperty(serializedObject, "m_SoftShadowsSupported", enableSoftShadows);

            // Additional Lights Per Object
            SetProperty(serializedObject, "m_AdditionalLightsPerObjectLimit", maxAdditionalLightsPerObject);

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);

            Debug.Log($"[URPShadowOptimizer] Configuración aplicada a: {asset.name}");
        }

        private void SetProperty(SerializedObject obj, string propertyName, object value)
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
            else
            {
                Debug.LogWarning($"Property {propertyName} not found");
            }
        }

        private void AnalyzeSceneLights()
        {
            Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            int totalAdditionalLights = 0;
            int shadowCastingLights = 0;
            int pointLights = 0;
            int spotLights = 0;
            int softShadows = 0;
            int estimatedShadowMaps = 0;

            foreach (var light in allLights)
            {
                if (light.type == LightType.Directional) continue;

                totalAdditionalLights++;

                if (light.shadows != LightShadows.None)
                {
                    shadowCastingLights++;

                    if (light.shadows == LightShadows.Soft)
                        softShadows++;

                    if (light.type == LightType.Point)
                    {
                        pointLights++;
                        estimatedShadowMaps += 6;
                    }
                    else if (light.type == LightType.Spot)
                    {
                        spotLights++;
                        estimatedShadowMaps += 1;
                    }
                }
            }

            string message = $"Análisis de Luces en la Escena:\n\n" +
                           $"Total luces adicionales: {totalAdditionalLights}\n" +
                           $"Luces con sombras: {shadowCastingLights}\n" +
                           $"  • Point Lights: {pointLights} (usan {pointLights * 6} shadow maps)\n" +
                           $"  • Spot Lights: {spotLights} (usan {spotLights} shadow maps)\n" +
                           $"  • Soft Shadows: {softShadows}\n\n" +
                           $"Shadow Maps totales estimados: {estimatedShadowMaps}\n" +
                           $"Tamaño atlas configurado: {shadowAtlasSize}x{shadowAtlasSize}\n\n";

            if (estimatedShadowMaps > 32)
            {
                message += $"⚠️ ADVERTENCIA: Tienes demasiados shadow maps ({estimatedShadowMaps}).\n" +
                          $"Esto causará los logs que estás viendo.\n\n" +
                          $"Recomendaciones:\n" +
                          $"1. Convierte Point Lights a Spot Lights (ahorra ~83% de shadow maps)\n" +
                          $"2. Reduce el número de luces con sombras activas\n" +
                          $"3. Aumenta el atlas de sombras a 2048 o 4096\n" +
                          $"4. Desactiva Soft Shadows";
            }
            else
            {
                message += "✓ El número de shadow maps es aceptable.";
            }

            EditorUtility.DisplayDialog("Scene Light Analysis", message, "OK");
            Debug.Log($"[URPShadowOptimizer] {message}");
        }

        private void ConvertPointLightsInScene()
        {
            Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            int converted = 0;
            List<GameObject> modifiedObjects = new List<GameObject>();

            foreach (var light in allLights)
            {
                if (light.type == LightType.Point && light.shadows != LightShadows.None)
                {
                    Undo.RecordObject(light, "Convert Point Light to Spot Light");
                    light.type = LightType.Spot;
                    light.spotAngle = 120f;
                    converted++;
                    modifiedObjects.Add(light.gameObject);
                }
            }

            if (converted > 0)
            {
                foreach (var obj in modifiedObjects)
                {
                    EditorUtility.SetDirty(obj);
                }

                int savedShadowMaps = converted * 5; // Cada Point Light convertida ahorra 5 shadow maps (6-1)
                EditorUtility.DisplayDialog("Conversion Complete", 
                    $"Convertidas {converted} Point Lights a Spot Lights.\n" +
                    $"Shadow maps ahorrados: {savedShadowMaps}\n" +
                    $"Esto reducirá significativamente los warnings.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("No Changes", "No se encontraron Point Lights con sombras para convertir.", "OK");
            }
        }

        private void DisableSoftShadowsInScene()
        {
            Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            int changed = 0;

            foreach (var light in allLights)
            {
                if (light.shadows == LightShadows.Soft)
                {
                    Undo.RecordObject(light, "Disable Soft Shadows");
                    light.shadows = LightShadows.Hard;
                    changed++;
                    EditorUtility.SetDirty(light.gameObject);
                }
            }

            if (changed > 0)
            {
                EditorUtility.DisplayDialog("Soft Shadows Disabled", 
                    $"Cambiadas {changed} luces de Soft a Hard Shadows.\n" +
                    $"Esto mejorará el rendimiento en dispositivos móviles.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("No Changes", "No se encontraron luces con Soft Shadows.", "OK");
            }
        }
    }
}
#endif

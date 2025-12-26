#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

namespace Santa.Editor
{
    /// <summary>
    /// Optimiza automáticamente las luces en la escena para reducir shadow maps
    /// </summary>
    public class SceneLightOptimizer : EditorWindow
    {
        private int maxShadowedLights = 12;
        private bool convertAllPointToSpot = true;
        private bool disableAllSoftShadows = true;
        private bool limitShadowsByDistance = true;
        private float maxShadowDistance = 30f;
        private bool limitShadowsByImportance = true;

        [MenuItem("Santa/Optimize Scene Lights (Fix Shadow Warnings)")]
        public static void ShowWindow()
        {
            var window = GetWindow<SceneLightOptimizer>("Scene Light Optimizer");
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Scene Light Optimizer", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Esta herramienta optimiza automáticamente TODAS las luces en la escena actual " +
                "para eliminar los warnings de shadow maps.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Análisis actual
            if (GUILayout.Button("📊 Analyze Current Scene", GUILayout.Height(30)))
            {
                AnalyzeAndShowReport();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Optimizations to Apply:", EditorStyles.boldLabel);

            convertAllPointToSpot = EditorGUILayout.Toggle("Convert Point → Spot Lights", convertAllPointToSpot);
            EditorGUILayout.HelpBox("Esto reduce shadow maps de 6 a 1 por luz (83% reducción)", MessageType.None);

            disableAllSoftShadows = EditorGUILayout.Toggle("Disable Soft Shadows", disableAllSoftShadows);
            EditorGUILayout.HelpBox("Mejora rendimiento y permite mayor resolución", MessageType.None);

            EditorGUILayout.Space(5);
            limitShadowsByImportance = EditorGUILayout.Toggle("Limit Shadows by Importance", limitShadowsByImportance);
            if (limitShadowsByImportance)
            {
                EditorGUI.indentLevel++;
                maxShadowedLights = EditorGUILayout.IntSlider("Max Shadowed Lights", maxShadowedLights, 4, 32);
                EditorGUILayout.HelpBox($"Solo las {maxShadowedLights} luces más importantes tendrán sombras", MessageType.None);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(5);
            limitShadowsByDistance = EditorGUILayout.Toggle("Disable Far Lights", limitShadowsByDistance);
            if (limitShadowsByDistance)
            {
                EditorGUI.indentLevel++;
                maxShadowDistance = EditorGUILayout.Slider("Max Distance", maxShadowDistance, 10f, 100f);
                EditorGUILayout.HelpBox("Luces más lejas de la cámara principal no tendrán sombras", MessageType.None);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(15);

            // Botón principal
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("✨ APPLY ALL OPTIMIZATIONS", GUILayout.Height(40)))
            {
                ApplyAllOptimizations();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);

            // Botones individuales
            EditorGUILayout.LabelField("Individual Actions:", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Convert Point Lights Only"))
            {
                ConvertPointLightsToSpot();
            }

            if (GUILayout.Button("Disable Soft Shadows Only"))
            {
                DisableSoftShadows();
            }

            if (GUILayout.Button("Disable All Additional Light Shadows"))
            {
                DisableAllAdditionalLightShadows();
            }
        }

        private void AnalyzeAndShowReport()
        {
            var lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            Camera mainCam = Camera.main;

            int totalLights = 0;
            int shadowedLights = 0;
            int pointLights = 0;
            int spotLights = 0;
            int softShadows = 0;
            int hardShadows = 0;
            int estimatedShadowMaps = 0;

            var lightsByDistance = new List<LightInfo>();

            foreach (var light in lights)
            {
                if (light == null || !light.isActiveAndEnabled) continue;
                if (light.type == LightType.Directional) continue;

                totalLights++;

                float distance = mainCam != null ? Vector3.Distance(mainCam.transform.position, light.transform.position) : 0;

                if (light.shadows != LightShadows.None)
                {
                    shadowedLights++;

                    if (light.shadows == LightShadows.Soft)
                        softShadows++;
                    else
                        hardShadows++;

                    int shadowMaps = 0;
                    if (light.type == LightType.Point)
                    {
                        pointLights++;
                        shadowMaps = 6;
                    }
                    else if (light.type == LightType.Spot)
                    {
                        spotLights++;
                        shadowMaps = 1;
                    }

                    estimatedShadowMaps += shadowMaps;

                    lightsByDistance.Add(new LightInfo
                    {
                        light = light,
                        distance = distance,
                        shadowMaps = shadowMaps,
                        intensity = light.intensity
                    });
                }
            }

            // Ordenar por importancia
            lightsByDistance = lightsByDistance.OrderByDescending(l => l.intensity / (l.distance + 1)).ToList();

            string report = $"=== SCENE LIGHT ANALYSIS ===\n\n" +
                          $"Total Additional Lights: {totalLights}\n" +
                          $"Lights with Shadows: {shadowedLights}\n\n" +
                          $"Shadow Types:\n" +
                          $"  • Point Lights: {pointLights} ({pointLights * 6} shadow maps)\n" +
                          $"  • Spot Lights: {spotLights} ({spotLights} shadow maps)\n" +
                          $"  • Soft Shadows: {softShadows}\n" +
                          $"  • Hard Shadows: {hardShadows}\n\n" +
                          $"TOTAL SHADOW MAPS: {estimatedShadowMaps}\n" +
                          $"ATLAS SIZE: 4096x4096\n\n";

            if (estimatedShadowMaps > 32)
            {
                report += $"❌ PROBLEMA DETECTADO!\n" +
                         $"Tienes {estimatedShadowMaps} shadow maps, pero el límite es ~32-40\n\n" +
                         $"RECOMENDACIONES:\n";

                if (pointLights > 0)
                {
                    int savedMaps = pointLights * 5;
                    report += $"1. Convertir {pointLights} Point Lights → Spot: AHORRA {savedMaps} shadow maps\n";
                }

                if (softShadows > 0)
                {
                    report += $"2. Desactivar Soft Shadows en {softShadows} luces: MEJORA rendimiento\n";
                }

                int excess = shadowedLights - maxShadowedLights;
                if (excess > 0)
                {
                    report += $"3. Desactivar sombras en las {excess} luces menos importantes\n";
                }

                report += $"\n✨ Haz clic en 'APPLY ALL OPTIMIZATIONS' para arreglar automáticamente";
            }
            else
            {
                report += $"✅ Shadow maps dentro del límite aceptable\n" +
                         $"Pero aún puedes optimizar para mejor rendimiento.";
            }

            EditorUtility.DisplayDialog("Scene Analysis", report, "OK");
            Debug.Log($"[SceneLightOptimizer]\n{report}");

            // Mostrar top luces por importancia
            if (lightsByDistance.Count > 0)
            {
                Debug.Log($"\n=== TOP 10 MOST IMPORTANT LIGHTS ===");
                for (int i = 0; i < Mathf.Min(10, lightsByDistance.Count); i++)
                {
                    var info = lightsByDistance[i];
                    Debug.Log($"{i + 1}. {info.light.gameObject.name} - Type: {info.light.type}, " +
                             $"Distance: {info.distance:F1}m, Intensity: {info.intensity:F2}, Shadow Maps: {info.shadowMaps}");
                }
            }
        }

        private void ApplyAllOptimizations()
        {
            if (!EditorUtility.DisplayDialog("Confirm Optimization",
                "This will modify lights in your scene. This action can be undone with Ctrl+Z.\n\n" +
                "Continue?", "Yes", "Cancel"))
            {
                return;
            }

            Undo.RecordObject(SceneManager.GetActiveScene().GetRootGameObjects()[0], "Optimize Scene Lights");

            int totalChanges = 0;

            if (convertAllPointToSpot)
            {
                totalChanges += ConvertPointLightsToSpot();
            }

            if (disableAllSoftShadows)
            {
                totalChanges += DisableSoftShadows();
            }

            if (limitShadowsByImportance)
            {
                totalChanges += LimitShadowsByImportance();
            }

            if (limitShadowsByDistance)
            {
                totalChanges += DisableFarLights();
            }

            EditorUtility.DisplayDialog("Optimization Complete",
                $"Applied {totalChanges} optimizations to scene lights.\n\n" +
                "The shadow warnings should now be resolved!", "OK");

            EditorUtility.SetDirty(SceneManager.GetActiveScene().GetRootGameObjects()[0]);
        }

        private int ConvertPointLightsToSpot()
        {
            var lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            int converted = 0;

            foreach (var light in lights)
            {
                if (light == null || !light.isActiveAndEnabled) continue;
                if (light.type == LightType.Point && light.shadows != LightShadows.None)
                {
                    Undo.RecordObject(light, "Convert Point to Spot");
                    light.type = LightType.Spot;
                    light.spotAngle = 120f; // Ángulo amplio para simular point light
                    EditorUtility.SetDirty(light.gameObject);
                    converted++;
                }
            }

            Debug.Log($"[SceneLightOptimizer] Converted {converted} Point Lights to Spot Lights (saved {converted * 5} shadow maps)");
            return converted;
        }

        private int DisableSoftShadows()
        {
            var lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            int changed = 0;

            foreach (var light in lights)
            {
                if (light == null || !light.isActiveAndEnabled) continue;
                if (light.shadows == LightShadows.Soft)
                {
                    Undo.RecordObject(light, "Disable Soft Shadows");
                    light.shadows = LightShadows.Hard;
                    EditorUtility.SetDirty(light.gameObject);
                    changed++;
                }
            }

            Debug.Log($"[SceneLightOptimizer] Disabled soft shadows on {changed} lights");
            return changed;
        }

        private int DisableAllAdditionalLightShadows()
        {
            if (!EditorUtility.DisplayDialog("Warning",
                "This will DISABLE shadows on ALL additional lights (not directional).\n\n" +
                "Are you sure?", "Yes", "Cancel"))
            {
                return 0;
            }

            var lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            int changed = 0;

            foreach (var light in lights)
            {
                if (light == null || !light.isActiveAndEnabled) continue;
                if (light.type != LightType.Directional && light.shadows != LightShadows.None)
                {
                    Undo.RecordObject(light, "Disable Shadows");
                    light.shadows = LightShadows.None;
                    EditorUtility.SetDirty(light.gameObject);
                    changed++;
                }
            }

            Debug.Log($"[SceneLightOptimizer] Disabled shadows on {changed} additional lights");
            return changed;
        }

        private int LimitShadowsByImportance()
        {
            var lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            Camera mainCam = Camera.main;

            var lightInfos = new List<LightInfo>();

            foreach (var light in lights)
            {
                if (light == null || !light.isActiveAndEnabled) continue;
                if (light.type == LightType.Directional) continue;
                if (light.shadows == LightShadows.None) continue;

                float distance = mainCam != null ? Vector3.Distance(mainCam.transform.position, light.transform.position) : 0;
                float importance = light.intensity / (distance + 1);

                lightInfos.Add(new LightInfo
                {
                    light = light,
                    distance = distance,
                    intensity = light.intensity,
                    importance = importance
                });
            }

            // Ordenar por importancia
            lightInfos = lightInfos.OrderByDescending(l => l.importance).ToList();

            int changed = 0;
            for (int i = 0; i < lightInfos.Count; i++)
            {
                var info = lightInfos[i];
                if (i >= maxShadowedLights)
                {
                    // Desactivar sombras en luces menos importantes
                    Undo.RecordObject(info.light, "Disable Low Priority Shadows");
                    info.light.shadows = LightShadows.None;
                    EditorUtility.SetDirty(info.light.gameObject);
                    changed++;
                }
            }

            Debug.Log($"[SceneLightOptimizer] Disabled shadows on {changed} low-priority lights (kept top {maxShadowedLights})");
            return changed;
        }

        private int DisableFarLights()
        {
            var lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            Camera mainCam = Camera.main;

            if (mainCam == null)
            {
                Debug.LogWarning("[SceneLightOptimizer] No main camera found, skipping distance-based optimization");
                return 0;
            }

            int changed = 0;
            foreach (var light in lights)
            {
                if (light == null || !light.isActiveAndEnabled) continue;
                if (light.type == LightType.Directional) continue;
                if (light.shadows == LightShadows.None) continue;

                float distance = Vector3.Distance(mainCam.transform.position, light.transform.position);
                if (distance > maxShadowDistance)
                {
                    Undo.RecordObject(light, "Disable Far Light Shadows");
                    light.shadows = LightShadows.None;
                    EditorUtility.SetDirty(light.gameObject);
                    changed++;
                }
            }

            Debug.Log($"[SceneLightOptimizer] Disabled shadows on {changed} far lights (>{maxShadowDistance}m from camera)");
            return changed;
        }

        private class LightInfo
        {
            public Light light;
            public float distance;
            public float intensity;
            public float importance;
            public int shadowMaps;
        }
    }
}
#endif

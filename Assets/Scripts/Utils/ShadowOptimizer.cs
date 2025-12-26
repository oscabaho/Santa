using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Santa.Utils
{
    /// <summary>
    /// Optimiza automáticamente las sombras de luces adicionales para dispositivos móviles
    /// </summary>
    public class ShadowOptimizer : MonoBehaviour
    {
        [Header("Shadow Atlas Settings")]
        [Tooltip("Tamaño del atlas de sombras (recomendado: 2048 para móviles de gama media/alta)")]
        [SerializeField] private int shadowAtlasSize = 2048;

        [Header("Shadow Distance Optimization")]
        [Tooltip("Distancia máxima para renderizar sombras adicionales")]
        [SerializeField] private float maxShadowDistance = 30f;

        [Header("Light Culling")]
        [Tooltip("Máximo número de luces con sombras activas simultáneamente")]
        [SerializeField] private int maxShadowedLights = 8;

        [Tooltip("Priorizar luces por intensidad")]
        [SerializeField] private bool prioritizeByIntensity = true;

        [Header("Shadow Quality")]
        [Tooltip("Desactivar soft shadows (mejor rendimiento en móviles)")]
        [SerializeField] private bool disableSoftShadows = true;

        [Header("Resolution Limits")]
        [Tooltip("Resolución máxima para sombras adicionales en móviles")]
        [SerializeField] private int maxShadowResolution = 512;

        private Camera mainCamera;
        private Light[] allLights;
        private Transform cameraTransform;

        private void Start()
        {
            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
            }

            // Aplicar configuraciones iniciales de URP
            ApplyURPSettings();

            // Cachear todas las luces en la escena
            RefreshLightCache();
        }

        private void ApplyURPSettings()
        {
            // Nota: Estos cambios se deben hacer en el URP Asset directamente en el editor
            // Este método solo documenta los valores recomendados
            Debug.Log($"[ShadowOptimizer] Configuración recomendada para URP Asset:\n" +
                     $"- Shadow Atlas Resolution: {shadowAtlasSize}x{shadowAtlasSize}\n" +
                     $"- Additional Lights Shadow Resolution: {maxShadowResolution}\n" +
                     $"- Shadow Distance: {maxShadowDistance}\n" +
                     $"- Soft Shadows: {!disableSoftShadows}");
        }

        private void RefreshLightCache()
        {
            allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            Debug.Log($"[ShadowOptimizer] Encontradas {allLights.Length} luces en la escena");
        }

        private void LateUpdate()
        {
            if (cameraTransform == null) return;

            OptimizeShadowCasters();
        }

        private void OptimizeShadowCasters()
        {
            if (allLights == null || allLights.Length == 0)
            {
                RefreshLightCache();
                return;
            }

            // Estructura para almacenar luces con prioridad
            var lightPriorities = new System.Collections.Generic.List<LightPriority>();

            Vector3 cameraPos = cameraTransform.position;

            // Evaluar todas las luces
            foreach (var light in allLights)
            {
                if (light == null || !light.isActiveAndEnabled) continue;
                
                // Saltar la luz principal (directional)
                if (light.type == LightType.Directional) continue;

                float distance = Vector3.Distance(cameraPos, light.transform.position);
                
                // Si la luz está más lejos que la distancia de sombras, desactivar sombras
                if (distance > maxShadowDistance)
                {
                    if (light.shadows != LightShadows.None)
                    {
                        light.shadows = LightShadows.None;
                    }
                    continue;
                }

                // Calcular prioridad basada en distancia e intensidad
                float priority = CalculateLightPriority(light, distance);

                lightPriorities.Add(new LightPriority
                {
                    light = light,
                    priority = priority,
                    originalShadowType = light.shadows
                });
            }

            // Ordenar por prioridad (mayor a menor)
            lightPriorities.Sort((a, b) => b.priority.CompareTo(a.priority));

            // Activar sombras solo para las luces de mayor prioridad
            int shadowedLightsCount = 0;
            foreach (var lightPriority in lightPriorities)
            {
                var light = lightPriority.light;

                if (shadowedLightsCount < maxShadowedLights)
                {
                    // Activar sombras
                    if (light.shadows == LightShadows.None && lightPriority.originalShadowType != LightShadows.None)
                    {
                        // Usar hard shadows si está configurado
                        light.shadows = disableSoftShadows ? LightShadows.Hard : LightShadows.Soft;
                    }
                    
                    // Limitar la resolución de sombras
                    if (light.shadowCustomResolution > maxShadowResolution)
                    {
                        light.shadowCustomResolution = maxShadowResolution;
                    }

                    shadowedLightsCount++;
                }
                else
                {
                    // Desactivar sombras para luces de menor prioridad
                    if (light.shadows != LightShadows.None)
                    {
                        light.shadows = LightShadows.None;
                    }
                }
            }
        }

        private float CalculateLightPriority(Light light, float distance)
        {
            float priority = 0f;

            // Factor de distancia (más cerca = mayor prioridad)
            float distanceFactor = Mathf.Clamp01(1f - (distance / maxShadowDistance));
            priority += distanceFactor * 50f;

            // Factor de intensidad
            if (prioritizeByIntensity)
            {
                float intensityFactor = Mathf.Clamp01(light.intensity / 10f);
                priority += intensityFactor * 30f;
            }

            // Penalizar Point Lights (usan 6 shadow maps vs 1 de Spot)
            if (light.type == LightType.Point)
            {
                priority -= 20f;
            }

            // Bonus para luces que están en la vista de la cámara
            Vector3 directionToLight = (light.transform.position - cameraTransform.position).normalized;
            float dotProduct = Vector3.Dot(cameraTransform.forward, directionToLight);
            if (dotProduct > 0)
            {
                priority += dotProduct * 20f;
            }

            return priority;
        }

        private struct LightPriority
        {
            public Light light;
            public float priority;
            public LightShadows originalShadowType;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Validar valores
            shadowAtlasSize = Mathf.ClosestPowerOfTwo(Mathf.Clamp(shadowAtlasSize, 256, 4096));
            maxShadowDistance = Mathf.Max(0, maxShadowDistance);
            maxShadowedLights = Mathf.Clamp(maxShadowedLights, 1, 32);
            maxShadowResolution = Mathf.ClosestPowerOfTwo(Mathf.Clamp(maxShadowResolution, 128, 2048));
        }

        [ContextMenu("Apply Settings to URP Asset")]
        private void ApplySettingsToURPAsset()
        {
            var pipeline = GraphicsSettings.currentRenderPipeline;
            if (pipeline is UniversalRenderPipelineAsset urpAsset)
            {
                Debug.LogWarning("[ShadowOptimizer] Para modificar el URP Asset, debes hacerlo manualmente en el editor:\n" +
                                "1. Selecciona el URP Asset en Project Settings > Graphics\n" +
                                "2. En 'Shadows' ajusta:\n" +
                                $"   - Shadow Atlas Resolution: {shadowAtlasSize}\n" +
                                $"   - Additional Lights Shadow Resolution: {maxShadowResolution}\n" +
                                $"   - Shadow Distance: {maxShadowDistance}\n" +
                                $"   - Soft Shadows: {!disableSoftShadows}");
            }
        }

        [ContextMenu("Convert Point Lights to Spot Lights")]
        private void ConvertPointLightsToSpotLights()
        {
            RefreshLightCache();
            int converted = 0;
            foreach (var light in allLights)
            {
                if (light != null && light.type == LightType.Point && light.shadows != LightShadows.None)
                {
                    light.type = LightType.Spot;
                    light.spotAngle = 120f; // Ángulo amplio para simular punto
                    converted++;
                    UnityEditor.EditorUtility.SetDirty(light.gameObject);
                }
            }
            Debug.Log($"[ShadowOptimizer] Convertidas {converted} Point Lights a Spot Lights");
        }

        [ContextMenu("Disable All Soft Shadows")]
        private void DisableAllSoftShadows()
        {
            RefreshLightCache();
            int changed = 0;
            foreach (var light in allLights)
            {
                if (light != null && light.shadows == LightShadows.Soft)
                {
                    light.shadows = LightShadows.Hard;
                    changed++;
                    UnityEditor.EditorUtility.SetDirty(light.gameObject);
                }
            }
            Debug.Log($"[ShadowOptimizer] Cambiadas {changed} luces a Hard Shadows");
        }

        [ContextMenu("Show Shadow Statistics")]
        private void ShowShadowStatistics()
        {
            RefreshLightCache();
            int totalLights = 0;
            int shadowCastingLights = 0;
            int pointLights = 0;
            int spotLights = 0;
            int softShadows = 0;
            int estimatedShadowMaps = 0;

            foreach (var light in allLights)
            {
                if (light == null || !light.isActiveAndEnabled) continue;
                if (light.type == LightType.Directional) continue;

                totalLights++;

                if (light.shadows != LightShadows.None)
                {
                    shadowCastingLights++;

                    if (light.shadows == LightShadows.Soft)
                        softShadows++;

                    if (light.type == LightType.Point)
                    {
                        pointLights++;
                        estimatedShadowMaps += 6; // Point lights usan 6 shadow maps
                    }
                    else if (light.type == LightType.Spot)
                    {
                        spotLights++;
                        estimatedShadowMaps += 1; // Spot lights usan 1 shadow map
                    }
                }
            }

            Debug.Log($"[ShadowOptimizer] Estadísticas de Sombras:\n" +
                     $"Total luces adicionales: {totalLights}\n" +
                     $"Luces con sombras: {shadowCastingLights}\n" +
                     $"  - Point Lights: {pointLights} (usan {pointLights * 6} shadow maps)\n" +
                     $"  - Spot Lights: {spotLights} (usan {spotLights} shadow maps)\n" +
                     $"  - Soft Shadows: {softShadows}\n" +
                     $"Shadow Maps totales estimados: {estimatedShadowMaps}\n" +
                     $"Tamaño atlas configurado: {shadowAtlasSize}x{shadowAtlasSize}\n" +
                     $"Máximo de sombras activas: {maxShadowedLights}");
        }
#endif
    }
}

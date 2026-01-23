using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Editor
{
    public class ConfigureSpotLights : EditorWindow
    {
        [MenuItem("Tools/Configure All Spot Lights")]
        public static void ShowWindow()
        {
            GetWindow<ConfigureSpotLights>("Configure Spot Lights");
        }

        private Color lightColor = new(1f, 0.78f, 0.35f); // Sodium Vapor (Warm Orange)
        private float intensity = 6f; // Tuned for Baked: 40 is too bright, 6 is balanced
        private float range = 20f; // Wider range for street lighting
        private float spotAngle = 120f;
        private float innerSpotAngle = 60f;
        private float indirectMultiplier = 2f; // Higher bounce for better ambient fill
        private LightShadows shadowType = LightShadows.Soft;
        private LightmapBakeType lightMode = LightmapBakeType.Baked;

        private void OnGUI()
        {
            GUILayout.Label("Spot Light Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            lightColor = EditorGUILayout.ColorField("Color", lightColor);
            intensity = EditorGUILayout.FloatField("Intensity", intensity);
            range = EditorGUILayout.FloatField("Range", range);
            spotAngle = EditorGUILayout.FloatField("Outer Spot Angle", spotAngle);
            innerSpotAngle = EditorGUILayout.FloatField("Inner Spot Angle", innerSpotAngle);
            indirectMultiplier = EditorGUILayout.FloatField("Indirect Multiplier", indirectMultiplier);
            shadowType = (LightShadows)EditorGUILayout.EnumPopup("Shadow Type", shadowType);
            lightMode = (LightmapBakeType)EditorGUILayout.EnumPopup("Light Mode", lightMode);

            EditorGUILayout.Space();

            if (GUILayout.Button("Apply to All Spot Lights in Scene"))
            {
                ApplyConfigurationToAllSpotLights();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "This will configure all Spot lights in the current scene with the settings above.\n\n" +
                "Settings applied:\n" +
                "- Mode: " + lightMode + "\n" +
                "- Shadow Type: " + shadowType + "\n" +
                "- Culling Mask: Everything\n" +
                "- And the custom values above",
                MessageType.Info);
        }

        private void ApplyConfigurationToAllSpotLights()
        {
            Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            int spotLightsConfigured = 0;

            Undo.RecordObjects(allLights, "Configure Spot Lights");

            foreach (Light light in allLights)
            {
                if (light.type == LightType.Spot)
                {
                    // Basic light settings
                    light.color = lightColor;
                    light.intensity = intensity;
                    light.range = range;
                    light.spotAngle = spotAngle;
                    light.innerSpotAngle = innerSpotAngle;
                    light.bounceIntensity = indirectMultiplier;

                    // Mode and shadows
                    light.lightmapBakeType = lightMode;
                    light.shadows = shadowType;

                    // Culling mask
                    light.cullingMask = -1; // Everything

                    // Cookie (none)
                    light.cookie = null;

                    // Mark as dirty for saving
                    EditorUtility.SetDirty(light.gameObject);

                    spotLightsConfigured++;
                }
            }

            Debug.Log($"Configured {spotLightsConfigured} Spot lights in the scene.");
            EditorUtility.DisplayDialog("Success",
                $"Successfully configured {spotLightsConfigured} Spot lights.", "OK");
        }
    }
}

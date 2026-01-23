using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Editor
{
    public class LightingDebugger : EditorWindow
    {
        [MenuItem("Tools/Debug Lighting Values")]
        public static void DebugLighting()
        {
            int issuesFound = 0;

            // 1. Check Transforms (Scale 0 is a common cause of NaNs)
            Transform[] transforms = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            foreach (var t in transforms)
            {
                if (t.localScale.x == 0 || t.localScale.y == 0 || t.localScale.z == 0)
                {
                    // Ignore particle systems as they often have 0 scale start
                    if (t.GetComponent<ParticleSystem>() == null)
                    {
                        Debug.LogWarning($"⚠️ Zero Scale detected: {t.name} (Parent: {(t.parent != null ? t.parent.name : "root")}). This can cause bake errors.", t.gameObject);
                        issuesFound++;
                    }
                }
            }

            // 2. Check Lights for extreme values (not just Infinity)
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (var l in lights)
            {
                if (l.intensity > 10000f && l.type != LightType.Directional)
                {
                    Debug.LogWarning($"⚠️ Extremely High Intensity Light: {l.name} ({l.intensity}). Verify if this is intended.", l.gameObject);
                    issuesFound++;
                }

                if (float.IsNaN(l.intensity) || float.IsInfinity(l.intensity) ||
                    float.IsNaN(l.range) || float.IsInfinity(l.range) ||
                    float.IsNaN(l.bounceIntensity) || float.IsInfinity(l.bounceIntensity) ||
                    float.IsNaN(l.color.r) || float.IsNaN(l.color.g) || float.IsNaN(l.color.b))
                {
                    Debug.LogError($"🚨 Invalid Light Value: {l.name}", l.gameObject);
                    issuesFound++;
                }
            }

            // 3. Check Renderers
            Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            foreach (var r in renderers)
            {
                if (float.IsNaN(r.bounds.center.x) || float.IsInfinity(r.bounds.center.x) ||
                    r.bounds.size == Vector3.zero)
                {
                    // Zero bounds on a visible renderer can be an issue
                    if (r.enabled && r.gameObject.activeInHierarchy)
                    {
                        // Skip simple particle systems again
                        if (r is not ParticleSystemRenderer)
                        {
                            Debug.LogWarning($"⚠️ Suspicious Renderer Bounds (NaN or Zero): {r.name}", r.gameObject);
                            issuesFound++;
                        }
                    }
                }

                // Check material emission
                foreach (var mat in r.sharedMaterials)
                {
                    if (mat != null && mat.HasProperty("_EmissionColor"))
                    {
                        Color c = mat.GetColor("_EmissionColor");
                        if (c.maxColorComponent > 100f)
                        {
                            Debug.LogWarning($"⚠️ Very High Emission Material: {r.name} (Mat: {mat.name}, Intensity: {c.maxColorComponent})", r.gameObject);
                            issuesFound++;
                        }
                    }
                }
            }

            if (issuesFound == 0)
            {
                EditorUtility.DisplayDialog("Lighting Debugger", "✅ No Zero-Scale objects, Extreme Lights, or Invalid values found.\n\nNext Step: Go to Window > Rendering > Lighting, and try reducing 'Lightmap Resolution' or switching 'Directional Mode' to 'Non-Directional' to isolate the issue.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Lighting Debugger", $"⚠️ Found {issuesFound} potential issues.\n\nCheck the Console (Window > General > Console) for the list of yellow warnings/red errors.", "OK");
            }
        }

        private static bool IsInvalidColor(Color c)
        {
            return float.IsNaN(c.r) || float.IsInfinity(c.r) ||
                   float.IsNaN(c.g) || float.IsInfinity(c.g) ||
                   float.IsNaN(c.b) || float.IsInfinity(c.b) ||
                   float.IsNaN(c.a) || float.IsInfinity(c.a);
        }
    }
}

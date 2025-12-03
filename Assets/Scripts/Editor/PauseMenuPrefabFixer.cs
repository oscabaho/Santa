#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Santa.UI;
using Santa.Presentation.UI;
using Santa.Presentation.Menus;

namespace Santa.Editor
{
    /// <summary>
    /// Adds required components to PauseMenu prefab for UIManager compatibility.
    /// Menu: "Santa/Fix PauseMenu Prefab"
    /// </summary>
    public static class PauseMenuPrefabFixer
    {
        [MenuItem("Santa/Fix PauseMenu Prefab")]
        public static void FixPauseMenuPrefab()
        {
            // Find PauseMenu prefab by searching for PauseMenuUI component
            string[] guids = AssetDatabase.FindAssets("t:Prefab PauseMenu");
            if (guids.Length == 0)
            {
                Debug.LogWarning("PauseMenuPrefabFixer: Could not find PauseMenu prefab. Search manually in Assets/Prefabs/UI/");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
            {
                Debug.LogError($"PauseMenuPrefabFixer: Failed to load prefab at {path}");
                return;
            }

            // Check if PauseMenuUI exists on root
            var pauseMenuUI = prefab.GetComponent<PauseMenuUI>();
            if (pauseMenuUI == null)
            {
                Debug.LogError($"PauseMenuPrefabFixer: Prefab '{path}' does not have PauseMenuUI component on root.");
                return;
            }

            // Add components if missing
            bool modified = false;

            if (prefab.GetComponent<UIPanel>() == null)
            {
                Undo.AddComponent<UIPanel>(prefab);
                Debug.Log($"PauseMenuPrefabFixer: Added UIPanel to '{prefab.name}'.");
                modified = true;
            }
            else
            {
                Debug.Log($"PauseMenuPrefabFixer: UIPanel already exists on '{prefab.name}'.");
            }

            if (prefab.GetComponent<CanvasGroup>() == null)
            {
                Undo.AddComponent<CanvasGroup>(prefab);
                Debug.Log($"PauseMenuPrefabFixer: Added CanvasGroup to '{prefab.name}'.");
                modified = true;
            }
            else
            {
                Debug.Log($"PauseMenuPrefabFixer: CanvasGroup already exists on '{prefab.name}'.");
            }

            var animator = prefab.GetComponent<PauseMenuAnimator>();
            if (animator == null)
            {
                animator = Undo.AddComponent<PauseMenuAnimator>(prefab);
                Debug.Log($"PauseMenuPrefabFixer: Added PauseMenuAnimator to '{prefab.name}'.");
                modified = true;
            }
            else
            {
                Debug.Log($"PauseMenuPrefabFixer: PauseMenuAnimator already exists on '{prefab.name}'.");
            }

            // Auto-assign CanvasGroup reference in animator
            var canvasGroup = prefab.GetComponent<CanvasGroup>();
            if (canvasGroup != null && animator != null)
            {
                var so = new SerializedObject(animator);
                var canvasGroupProp = so.FindProperty("canvasGroup");
                if (canvasGroupProp != null && canvasGroupProp.objectReferenceValue == null)
                {
                    canvasGroupProp.objectReferenceValue = canvasGroup;
                    so.ApplyModifiedProperties();
                    Debug.Log($"PauseMenuPrefabFixer: Auto-assigned CanvasGroup reference in PauseMenuAnimator.");
                    modified = true;
                }
            }

            if (modified)
            {
                EditorUtility.SetDirty(prefab);
                AssetDatabase.SaveAssets();
                Debug.Log($"<color=green>PauseMenuPrefabFixer: Successfully updated '{path}'. Prefab is ready for UIManager.</color>");
            }
            else
            {
                Debug.Log($"PauseMenuPrefabFixer: No changes needed. '{path}' is already configured correctly.");
            }
        }
    }
}
#endif

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;

namespace Santa.Editor
{
    /// <summary>
    /// Adds missing essential components to the active scene.
    /// </summary>
    public class SceneMissingComponentsFixer : EditorWindow
    {
        [MenuItem("Santa/Fix Missing Scene Components")]
        public static void ShowWindow()
        {
            GetWindow<SceneMissingComponentsFixer>("Scene Components Fixer");
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "This tool will add missing essential components to your active scene:\n\n" +
                "• PauseMenuController (required for pause functionality)\n" +
                "• Other essential managers as needed\n\n" +
                "Make sure the scene is open before running this.",
                MessageType.Info
            );

            EditorGUILayout.Space();

            if (GUILayout.Button("Add Missing PauseMenuController", GUILayout.Height(40)))
            {
                AddPauseMenuController();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Verify Scene Setup", GUILayout.Height(30)))
            {
                VerifySceneSetup();
            }
        }

        private static void AddPauseMenuController()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogError("No valid scene is currently open.");
                return;
            }

            // Check if PauseMenuController already exists
            var existingController = FindFirstObjByType<Santa.UI.PauseMenuController>();
            if (existingController != null)
            {
                Debug.Log($"PauseMenuController already exists on GameObject '{existingController.gameObject.name}'.");
                EditorGUIUtility.PingObject(existingController.gameObject);
                return;
            }

            // Find or create Services root
            GameObject servicesRoot = null;
            foreach (var rootObj in scene.GetRootGameObjects())
            {
                if (rootObj.name == "Services")
                {
                    servicesRoot = rootObj;
                    break;
                }
            }

            if (servicesRoot == null)
            {
                servicesRoot = new GameObject("Services");
                Undo.RegisterCreatedObjectUndo(servicesRoot, "Create Services Root");
                Debug.Log("Created 'Services' root GameObject.");
            }

            // Create PauseMenuController GameObject
            var pauseMenuController = new GameObject("PauseMenuController");
            pauseMenuController.transform.SetParent(servicesRoot.transform);
            pauseMenuController.AddComponent<Santa.UI.PauseMenuController>();

            Undo.RegisterCreatedObjectUndo(pauseMenuController, "Create PauseMenuController");

            EditorGUIUtility.PingObject(pauseMenuController);
            Debug.Log("<color=green>✓ PauseMenuController created successfully under Services/</color>");

            EditorSceneManager.MarkSceneDirty(scene);
        }

        private static void VerifySceneSetup()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogError("No valid scene is currently open.");
                return;
            }

            Debug.Log($"<b>Verifying Scene: {scene.name}</b>");

            // Check for essential components
            var pauseController = FindFirstObjByType<Santa.UI.PauseMenuController>();
            // UIManager: avoid type reference; check by name to prevent compile issues if not present
            var uiManagerByName = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(m => m != null && m.GetType().Name == "UIManager");
            // LevelManager exists in global namespace
            var levelManager = FindFirstObjByType<LevelManager>();

            Debug.Log($"PauseMenuController: {(pauseController != null ? "✓ Found" : "✗ MISSING")}");
            Debug.Log($"UIManager: {(uiManagerByName != null ? "✓ Found (by name)" : "✗ MISSING")}");
            Debug.Log($"LevelManager: {(levelManager != null ? "✓ Found" : "✗ MISSING")}");

            if (pauseController == null)
            {
                Debug.LogWarning("⚠ PauseMenuController is missing. Click 'Add Missing PauseMenuController' to fix.");
            }
            else
            {
                Debug.Log($"<color=green>Scene setup verified! All essential components present.</color>");
            }
        }

        // Helper to find first object by type without hiding Unity's API name
        private static T FindFirstObjByType<T>() where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<T>();
#else
            return Object.FindObjectOfType<T>();
#endif
        }
    }
}

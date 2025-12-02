#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Santa.Editor
{
    /// <summary>
    /// Editor utility to organize scene hierarchy according to recommended structure.
    /// Right-click in Hierarchy: "Santa/Organize Scene Hierarchy"
    /// </summary>
    public static class SceneHierarchyOrganizer
    {
        private static readonly string[] RequiredRoots = { "Managers", "UI", "Cameras", "Actors", "Environment", "Pools" };
        private static readonly string[] OptionalRoots = { "Services" };
        private static readonly Dictionary<string, string[]> RequiredChildren = new()
        {
            { "UI", new[] { "StaticCanvas", "DynamicPanels" } },
            { "Cameras", new[] { "CombatCameras" } },
            { "Environment", new[] { "Terrain", "Markers", "Props_Static", "Props_Dynamic", "State" } }
        };

        [MenuItem("GameObject/Santa/Organize Scene Hierarchy", false, 0)]
        public static void OrganizeCurrentScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogError("SceneOrganizer: No active scene.");
                return;
            }

            Undo.SetCurrentGroupName("Organize Scene Hierarchy");
            int undoGroup = Undo.GetCurrentGroup();

            Debug.Log($"SceneOrganizer: Organizing '{scene.name}'...");

            // 1. Create missing root folders
            var rootObjects = new Dictionary<string, GameObject>();
            foreach (var rootName in RequiredRoots)
            {
                var root = FindOrCreateRoot(rootName);
                rootObjects[rootName] = root;
                Undo.RegisterCreatedObjectUndo(root, "Create Root");
            }

            // 2. Create required children
            foreach (var kvp in RequiredChildren)
            {
                if (rootObjects.TryGetValue(kvp.Key, out var parent))
                {
                    foreach (var childName in kvp.Value)
                    {
                        FindOrCreateChild(parent.transform, childName);
                    }
                }
            }

            // 3. Organize existing objects by type
            OrganizeExistingObjects(rootObjects);

            Undo.CollapseUndoOperations(undoGroup);
            EditorUtility.SetDirty(scene.GetRootGameObjects()[0]);
            Debug.Log($"SceneOrganizer: Hierarchy organized successfully. Use Undo (Ctrl+Z) to revert.");
        }

        private static GameObject FindOrCreateRoot(string name)
        {
            var existing = GameObject.Find(name);
            if (existing != null && existing.transform.parent == null)
            {
                Debug.Log($"SceneOrganizer: Root '{name}' already exists.");
                return existing;
            }

            var root = new GameObject(name);
            Debug.Log($"SceneOrganizer: Created root '{name}'.");
            return root;
        }

        private static GameObject FindOrCreateChild(Transform parent, string name)
        {
            var existing = parent.Find(name);
            if (existing != null)
            {
                Debug.Log($"SceneOrganizer: Child '{parent.name}/{name}' already exists.");
                return existing.gameObject;
            }

            var child = new GameObject(name);
            Undo.SetTransformParent(child.transform, parent, "Create Child");
            Debug.Log($"SceneOrganizer: Created '{parent.name}/{name}'.");
            return child;
        }

        private static void OrganizeExistingObjects(Dictionary<string, GameObject> rootObjects)
        {
            var scene = SceneManager.GetActiveScene();
            var roots = scene.GetRootGameObjects();

            foreach (var obj in roots)
            {
                // Skip the root folders themselves
                if (rootObjects.ContainsValue(obj)) continue;
                // Skip GameLifetimeScope and EventSystem
                if (obj.name.Contains("LifetimeScope") || obj.name.Contains("EventSystem")) continue;

                string targetRoot = ClassifyObject(obj);
                if (targetRoot != null && rootObjects.TryGetValue(targetRoot, out var parent))
                {
                    // Special handling for UI children
                    if (targetRoot == "UI" && IsStaticUIPanel(obj))
                    {
                        var staticCanvas = parent.transform.Find("StaticCanvas");
                        if (staticCanvas != null)
                        {
                            Undo.SetTransformParent(obj.transform, staticCanvas, "Move to StaticCanvas");
                            Debug.Log($"SceneOrganizer: Moved '{obj.name}' to UI/StaticCanvas.");
                            continue;
                        }
                    }

                    // Ensure DynamicPanels is a child of StaticCanvas for proper rendering
                    var dynamicPanels = parent.transform.Find("DynamicPanels");
                    var staticCanvasTf = parent.transform.Find("StaticCanvas");
                    if (dynamicPanels != null && staticCanvasTf != null && dynamicPanels.parent != staticCanvasTf)
                    {
                        Undo.SetTransformParent(dynamicPanels, staticCanvasTf, "Move DynamicPanels under StaticCanvas");
                        Debug.Log("SceneOrganizer: Moved 'UI/DynamicPanels' under 'UI/StaticCanvas' to ensure canvased rendering.");
                    }

                    // Special handling for Environment state
                    if (targetRoot == "Environment" && obj.name.Contains("EnvironmentDecorState"))
                    {
                        var state = parent.transform.Find("State");
                        if (state != null)
                        {
                            Undo.SetTransformParent(obj.transform, state, "Move to Environment/State");
                            Debug.Log($"SceneOrganizer: Moved '{obj.name}' to Environment/State.");
                            continue;
                        }
                    }

                    Undo.SetTransformParent(obj.transform, parent.transform, $"Move to {targetRoot}");
                    Debug.Log($"SceneOrganizer: Moved '{obj.name}' to {targetRoot}.");
                }
            }
        }

        private static string ClassifyObject(GameObject obj)
        {
            // Check components to determine appropriate root
            if (obj.GetComponent<Camera>()) return "Cameras";
            if (obj.GetComponent<Light>()) return "Environment";
            if (obj.name.Contains("Player")) return "Actors";
            if (obj.name.Contains("Enemy")) return "Actors";
            if (obj.name.Contains("Pool")) return "Pools";

            // Manager types (check by component name to avoid assembly reference issues)
            var components = obj.GetComponents<Component>();
            foreach (var comp in components)
            {
                if (comp == null) continue;
                var typeName = comp.GetType().Name;

                // Managers
                if (typeName.Contains("Manager") || 
                    typeName == "SaveService" ||
                    typeName == "TurnBasedCombatManager")
                {
                    return "Managers";
                }

                // Services
                if (typeName == "PauseMenuController")
                {
                    return "Services";
                }

                // Environment
                if (typeName == "EnvironmentDecorState")
                {
                    return "Environment";
                }
            }

            // Environment types (terrain/props)
            if (obj.GetComponent<Terrain>() ||
                obj.name.Contains("Terrain") ||
                obj.name.Contains("Ground") ||
                obj.name.Contains("Prop"))
            {
                return "Environment";
            }

            return null; // Don't move unrecognized objects
        }

        private static bool IsStaticUIPanel(GameObject obj)
        {
            // GraphicsSettingsController or similar static UI
            return obj.name.Contains("Settings") || obj.name.Contains("Static");
        }
    }
}
#endif

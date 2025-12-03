#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Santa.Presentation.UI;
using Santa.Presentation.Menus;
using Santa.Infrastructure.Input;
using Santa.Infrastructure.Combat;
using Santa.Infrastructure.State;
using Santa.Infrastructure.Level;
using Santa.Infrastructure.Camera;
using Santa.Infrastructure;
using Santa.Presentation.Upgrades;
using Santa.Core.Pooling;

namespace Santa.Editor
{
    /// <summary>
    /// Editor utility to organize scene hierarchy according to recommended structure.
    /// Right-click in Hierarchy: "Santa/Organize Scene Hierarchy"
    /// </summary>
    public static class SceneHierarchyOrganizer
    {
        private static readonly string[] RequiredRoots = { "***Managers***", "***UI***", "***Cameras***", "***Actors***", "***Environment***", "***Pools***" };
        private static readonly string[] OptionalRoots = { "***Services***" };
        private static readonly Dictionary<string, string[]> RequiredChildren = new()
        {
            { "***UI***", new[] { "StaticCanvas" } },
            { "***Cameras***", new[] { "CombatCameras" } },
            { "***Environment***", new[] { "Terrain", "Markers", "Props_Static", "Props_Dynamic", "State" } }
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

            // Create optional roots if needed
            foreach (var rootName in OptionalRoots)
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

            // 3. Ensure UI infrastructure first so moves can target StaticCanvas
            EnsureUIInfrastructure(rootObjects);

            // 4. Organize existing objects by type
            OrganizeExistingObjects(rootObjects);
            ConsolidateLegacyRoots(rootObjects);
            EnsureManagersAndServices(rootObjects);
            EnsureLifetimeScopeAndWire(rootObjects);

            Undo.CollapseUndoOperations(undoGroup);
            EditorUtility.SetDirty(scene.GetRootGameObjects()[0]);
            Debug.Log($"SceneOrganizer: Hierarchy organized successfully. Use Undo (Ctrl+Z) to revert.");
        }

        private static GameObject FindOrCreateRoot(string name)
        {
            // Try find starred root first
            var existing = GameObject.Find(name);
            if (existing != null && existing.transform.parent == null)
            {
                Debug.Log($"SceneOrganizer: Root '{name}' already exists.");
                return existing;
            }
            // If starred version doesn't exist, check for plain version and rename/migrate
            var plainName = name.Replace("***", string.Empty);
            var plainExisting = GameObject.Find(plainName);
            if (plainExisting != null && plainExisting.transform.parent == null)
            {
                Undo.RecordObject(plainExisting, "Rename Root to Starred");
                plainExisting.name = name;
                Debug.Log($"SceneOrganizer: Renamed root '{plainName}' to '{name}'.");
                return plainExisting;
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
                // Normalize to starred naming convention
                if (targetRoot != null)
                    targetRoot = NormalizeRootName(targetRoot);

                if (targetRoot != null && rootObjects.TryGetValue(targetRoot, out var parent))
                {
                    // Special: DynamicPanels should go directly under StaticCanvas, not UI root
                    if (obj.name == "DynamicPanels")
                    {
                        var uiRoot = rootObjects["***UI***"];
                        var staticCanvas = uiRoot.transform.Find("StaticCanvas");
                        if (staticCanvas != null)
                        {
                            Undo.SetTransformParent(obj.transform, staticCanvas, "Move DynamicPanels to StaticCanvas");
                            Debug.Log("SceneOrganizer: Moved 'DynamicPanels' to UI/StaticCanvas.");
                            continue;
                        }
                    }

                    // Special handling for UI children
                    if (targetRoot == "***UI***" && IsStaticUIPanel(obj))
                    {
                        var staticCanvas = parent.transform.Find("StaticCanvas");
                        if (staticCanvas != null)
                        {
                            Undo.SetTransformParent(obj.transform, staticCanvas, "Move to StaticCanvas");
                            Debug.Log($"SceneOrganizer: Moved '{obj.name}' to UI/StaticCanvas.");
                            continue;
                        }
                    }

                    // Move GraphicsSettingsController under StaticCanvas
                    if (targetRoot == "***UI***" && obj.name.Contains("GraphicsSettingsController"))
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
                    if (targetRoot == "***Environment***" && obj.name.Contains("EnvironmentDecorState"))
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

        // Maps plain root names to starred convention
        private static string NormalizeRootName(string plain)
        {
            if (string.IsNullOrEmpty(plain)) return null;
            switch (plain)
            {
                case "Managers": return "***Managers***";
                case "UI": return "***UI***";
                case "Cameras": return "***Cameras***";
                case "Actors": return "***Actors***";
                case "Environment": return "***Environment***";
                case "Pools": return "***Pools***";
                case "Services": return "***Services***";
                default: return plain; // already starred or unknown
            }
        }

        private static void EnsureUIInfrastructure(Dictionary<string, GameObject> roots)
        {
            if (!roots.TryGetValue("***UI***", out var uiRoot)) return;

            // Ensure StaticCanvas has required components
            var staticCanvasTf = uiRoot.transform.Find("StaticCanvas");
            if (staticCanvasTf == null)
            {
                staticCanvasTf = FindOrCreateChild(uiRoot.transform, "StaticCanvas").transform;
            }

            var staticCanvasGo = staticCanvasTf.gameObject;
            var canvas = staticCanvasGo.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = Undo.AddComponent<Canvas>(staticCanvasGo);
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.pixelPerfect = true;
                Debug.Log("SceneOrganizer: Added Canvas to UI/StaticCanvas.");
            }

            var scaler = staticCanvasGo.GetComponent<UnityEngine.UI.CanvasScaler>();
            if (scaler == null)
            {
                scaler = Undo.AddComponent<UnityEngine.UI.CanvasScaler>(staticCanvasGo);
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                Debug.Log("SceneOrganizer: Added CanvasScaler to UI/StaticCanvas.");
            }

            var raycaster = staticCanvasGo.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (raycaster == null)
            {
                raycaster = Undo.AddComponent<UnityEngine.UI.GraphicRaycaster>(staticCanvasGo);
                Debug.Log("SceneOrganizer: Added GraphicRaycaster to UI/StaticCanvas.");
            }

            // Ensure DynamicPanels exists and is under StaticCanvas
            var dynamicPanels = staticCanvasTf.Find("DynamicPanels");
            if (dynamicPanels == null)
            {
                // Search anywhere in scene for a root/stray DynamicPanels and move it under StaticCanvas
                var allRoots = SceneManager.GetActiveScene().GetRootGameObjects();
                Transform found = uiRoot.transform.Find("DynamicPanels");
                if (found == null)
                {
                    foreach (var r in allRoots)
                    {
                        if (r.name == "DynamicPanels") { found = r.transform; break; }
                        var t = r.transform.Find("DynamicPanels");
                        if (t != null) { found = t; break; }
                    }
                }
                if (found != null)
                {
                    Undo.SetTransformParent(found, staticCanvasTf, "Move DynamicPanels under StaticCanvas");
                    Debug.Log("SceneOrganizer: Moved existing 'DynamicPanels' under 'UI/StaticCanvas'.");
                }
                else
                {
                    dynamicPanels = FindOrCreateChild(staticCanvasTf, "DynamicPanels").transform;
                }
            }

            // Ensure GraphicsSettingsController exists under StaticCanvas
            var gscTf = staticCanvasTf.Find("GraphicsSettingsController");
            GameObject gscGo;
            if (gscTf == null)
            {
                gscGo = new GameObject("GraphicsSettingsController");
                Undo.RegisterCreatedObjectUndo(gscGo, "Create GraphicsSettingsController");
                Undo.SetTransformParent(gscGo.transform, staticCanvasTf, "Move GraphicsSettingsController under StaticCanvas");
                Debug.Log("SceneOrganizer: Created 'UI/StaticCanvas/GraphicsSettingsController'.");
            }
            else gscGo = gscTf.gameObject;

            if (gscGo.GetComponent<GraphicsSettingsController>() == null)
            {
                Undo.AddComponent<GraphicsSettingsController>(gscGo);
                Debug.Log("SceneOrganizer: Added GraphicsSettingsController component to UI/StaticCanvas/GraphicsSettingsController.");
            }

            // Ensure EventSystem object with configurator
            var eventSystem = uiRoot.transform.Find("EventSystem");
            GameObject esGo;
            if (eventSystem == null)
            {
                esGo = new GameObject("EventSystem");
                Undo.RegisterCreatedObjectUndo(esGo, "Create EventSystem");
                Undo.SetTransformParent(esGo.transform, uiRoot.transform, "Create EventSystem");
                Debug.Log("SceneOrganizer: Created UI/EventSystem.");
            }
            else esGo = eventSystem.gameObject;

            if (esGo.GetComponent<UIEventSystemConfigurator>() == null)
            {
                Undo.AddComponent<UIEventSystemConfigurator>(esGo);
                Debug.Log("SceneOrganizer: Added UIEventSystemConfigurator to UI/EventSystem.");
            }
        }

        private static void EnsureManagersAndServices(Dictionary<string, GameObject> roots)
        {
            // Managers
            if (roots.TryGetValue("***Managers***", out var managersRoot))
            {
                EnsureManagerComponent<UIManager>(managersRoot, "UIManager");
                
                // TurnBasedCombatManager with required children
                var tbcm = EnsureManagerComponent<TurnBasedCombatManager>(managersRoot, "TurnBasedCombatManager");
                EnsureManagerComponent<ActionExecutor>(tbcm, "ActionExecutor");
                EnsureManagerComponent<AIManager>(tbcm, "AIManager");
                
                EnsureManagerComponent<GraphicsSettingsManager>(managersRoot, "GraphicsSettingsManager");
                EnsureManagerComponent<GameStateManager>(managersRoot, "GameStateManager");
                EnsureManagerComponent<GameplayUIManager>(managersRoot, "GameplayUIManager");
                EnsureManagerComponent<LevelManager>(managersRoot, "LevelManager");
                EnsureManagerComponent<CombatTransitionManager>(managersRoot, "CombatTransitionManager");
                EnsureManagerComponent<CombatEncounterManager>(managersRoot, "CombatEncounterManager");
                // Camera manager used by combat transitions
                EnsureManagerComponent<CombatCameraManager>(managersRoot, "CombatCameraManager");
                EnsureManagerComponent<Santa.Infrastructure.SaveService>(managersRoot, "SaveService");
                EnsureManagerComponent<UpgradeManager>(managersRoot, "UpgradeManager");
                EnsureManagerComponent<GameInitializer>(managersRoot, "GameInitializer");
                // Scene pool used by encounters
                EnsureManagerComponent<CombatScenePool>(managersRoot, "CombatScenePool");
            }

            // Services
            if (roots.TryGetValue("***Services***", out var servicesRoot))
            {
                EnsureManagerComponent<Santa.UI.PauseMenuController>(servicesRoot, "PauseMenuController");
            }

            // Environment State holder
            if (roots.TryGetValue("***Environment***", out var envRoot))
            {
                var stateTf = envRoot.transform.Find("State");
                if (stateTf == null) stateTf = FindOrCreateChild(envRoot.transform, "State").transform;
                EnsureManagerComponent<Santa.Core.Save.EnvironmentDecorState>(stateTf.gameObject, "EnvironmentDecorState");
            }

            // Actors - ensure Player with ExplorationPlayerIdentifier
            if (roots.TryGetValue("***Actors***", out var actorsRoot))
            {
                var playerTf = actorsRoot.transform.Find("Player");
                GameObject playerGo;
                if (playerTf == null)
                {
                    playerGo = new GameObject("Player");
                    Undo.RegisterCreatedObjectUndo(playerGo, "Create Player");
                    Undo.SetTransformParent(playerGo.transform, actorsRoot.transform, "Move Player to Actors");
                    Debug.Log("SceneOrganizer: Created 'Actors/Player'.");
                }
                else playerGo = playerTf.gameObject;

                if (playerGo.GetComponent<ExplorationPlayerIdentifier>() == null)
                {
                    Undo.AddComponent<ExplorationPlayerIdentifier>(playerGo);
                    Debug.Log("SceneOrganizer: Added ExplorationPlayerIdentifier to 'Actors/Player'.");
                }
                
                if (playerGo.GetComponent<Santa.Core.Player.PlayerReference>() == null)
                {
                    Undo.AddComponent<Santa.Core.Player.PlayerReference>(playerGo);
                    Debug.Log("SceneOrganizer: Added PlayerReference to 'Actors/Player'.");
                }
            }
        }

        private static void EnsureLifetimeScopeAndWire(Dictionary<string, GameObject> roots)
        {
            // Ensure a GameObject with GameLifetimeScope at root
            var gls = Object.FindFirstObjectByType<GameLifetimeScope>(FindObjectsInactive.Include);
            if (gls == null)
            {
                var go = new GameObject("GameLifetimeScope");
                Undo.RegisterCreatedObjectUndo(go, "Create GameLifetimeScope");
                gls = Undo.AddComponent<GameLifetimeScope>(go);
                Debug.Log("SceneOrganizer: Created root 'GameLifetimeScope' with component.");
            }

            // Try to wire known references if they are null
            var so = new SerializedObject(gls);
            TryWireAssetRef<InputReader>(so, "inputReaderAsset", "Assets/ScriptableObjects/Input/InputReader.asset");
            TryWireRef<UIManager>(so, "uiManagerInstance");
            TryWireRef<TurnBasedCombatManager>(so, "turnBasedCombatManagerInstance");
            TryWireRef<CombatTransitionManager>(so, "combatTransitionManagerInstance");
            TryWireRef<CombatEncounterManager>(so, "combatEncounterManagerInstance");
            TryWireRef<UpgradeManager>(so, "upgradeManagerInstance");
            TryWireRef<GameStateManager>(so, "gameStateManagerInstance");
            TryWireRef<GameplayUIManager>(so, "gameplayUIManagerInstance");
            TryWireRef<LevelManager>(so, "levelManagerInstance");
            TryWireRef<CombatCameraManager>(so, "combatCameraManagerInstance");
            TryWireRef<UpgradeManager>(so, "upgradeManagerInstance");
            so.ApplyModifiedPropertiesWithoutUndo();
            
            // Wire InputActions asset to UIEventSystemConfigurator
            var uiEventConfig = Object.FindFirstObjectByType<UIEventSystemConfigurator>(FindObjectsInactive.Include);
            if (uiEventConfig != null)
            {
                var configSo = new SerializedObject(uiEventConfig);
                TryWireAssetRef<UnityEngine.InputSystem.InputActionAsset>(configSo, "actionsAsset", "Assets/Input/ActionMap.inputactions");
                configSo.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        // Consolidate any legacy plain roots into the new starred roots
        private static void ConsolidateLegacyRoots(Dictionary<string, GameObject> rootObjects)
        {
            // Handle Environment specifically: migrate children from plain 'Environment' into '***Environment***'
            var starredEnv = GameObject.Find("***Environment***");
            var plainEnv = GameObject.Find("Environment");
            if (starredEnv != null && plainEnv != null && plainEnv.transform.parent == null)
            {
                var children = new List<Transform>();
                foreach (Transform child in plainEnv.transform) children.Add(child);
                foreach (var child in children)
                {
                    Undo.SetTransformParent(child, starredEnv.transform, "Migrate Environment child to starred root");
                }
                Undo.RecordObject(plainEnv, "Mark legacy Environment obsolete");
                plainEnv.name = "Environment_OBSOLETE";
                Debug.Log("SceneOrganizer: Migrated children from 'Environment' to '***Environment***' and marked old root as obsolete.");
            }
        }

        private static void TryWireRef<T>(SerializedObject so, string fieldName) where T : Component
        {
            var prop = so.FindProperty(fieldName);
            if (prop == null) return;
            if (prop.objectReferenceValue != null) return;
            var inst = Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
            if (inst != null)
            {
                prop.objectReferenceValue = inst;
                Debug.Log($"SceneOrganizer: Wired '{fieldName}' to '{inst.gameObject.name}'.");
            }
        }

        private static void TryWireAssetRef<T>(SerializedObject so, string fieldName, string assetPath) where T : Object
        {
            var prop = so.FindProperty(fieldName);
            if (prop == null) return;
            if (prop.objectReferenceValue != null) return;
            var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                prop.objectReferenceValue = asset;
                Debug.Log($"SceneOrganizer: Wired '{fieldName}' to asset '{assetPath}'.");
            }
            else
            {
                Debug.LogWarning($"SceneOrganizer: Asset '{assetPath}' not found for field '{fieldName}'.");
            }
        }

        private static GameObject EnsureManagerComponent<T>(GameObject parent, string childName) where T : Component
        {
            var tf = parent.transform.Find(childName);
            GameObject go;
            if (tf == null)
            {
                go = new GameObject(childName);
                Undo.RegisterCreatedObjectUndo(go, $"Create {childName}");
                Undo.SetTransformParent(go.transform, parent.transform, $"Move {childName}");
                Debug.Log($"SceneOrganizer: Created '{parent.name}/{childName}'.");
            }
            else go = tf.gameObject;

            if (go.GetComponent<T>() == null)
            {
                Undo.AddComponent<T>(go);
                Debug.Log($"SceneOrganizer: Added component {typeof(T).Name} to '{childName}'.");
            }
            
            return go;
        }

        private static GameObject EnsureManagerComponent<T>(Transform parent, string childName) where T : Component
            => EnsureManagerComponent<T>(parent.gameObject, childName);

        private static string ClassifyObject(GameObject obj)
        {
            // Check components to determine appropriate root (starred where applicable)
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
                    typeName == "GameInitializer" ||
                    typeName == "TurnBasedCombatManager")
                {
                    return "Managers";
                }

                // Services
                if (typeName == "PauseMenuController")
                {
                    return "Services";
                }

                // UI Controllers
                if (typeName == "GraphicsSettingsController")
                {
                    return "UI";
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

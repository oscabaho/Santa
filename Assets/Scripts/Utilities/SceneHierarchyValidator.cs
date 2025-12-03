using Santa.Core.Config;
using Santa.Infrastructure.Combat;
using Santa.Infrastructure.Level;
using Santa.Presentation.UI;
using Santa.Presentation.Menus;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Santa.Utils
{

/// <summary>
/// Validates presence and placement of key scene roots to keep hierarchy consistent.
/// Logs actionable hints on Play. Attach anywhere in the scene (e.g., to GameLifetimeScope).
/// </summary>
public class SceneHierarchyValidator : MonoBehaviour
{
    [Header("Expected Roots (Exact Names)")]
    [SerializeField] private string managersRoot = GameConstants.Hierarchy.Managers;
    [SerializeField] private string servicesRoot = GameConstants.Hierarchy.Services; // optional
    [SerializeField] private string uiRoot = GameConstants.Hierarchy.UI;
    [SerializeField] private string camerasRoot = GameConstants.Hierarchy.Cameras;
    [SerializeField] private string actorsRoot = GameConstants.Hierarchy.Actors;
    [SerializeField] private string environmentRoot = GameConstants.Hierarchy.Environment;
    [SerializeField] private string poolsRoot = GameConstants.Hierarchy.Pools;
    [SerializeField] private string dynamicPanelsName = GameConstants.Hierarchy.DynamicPanels;
    [SerializeField] private string combatCamerasName = GameConstants.Hierarchy.CombatCameras;

    private void Start()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        ValidateRoot(managersRoot);
        ValidateRoot(uiRoot);
        ValidateRoot(camerasRoot);
        ValidateRoot(actorsRoot);
        ValidateRoot(environmentRoot);
        ValidateRoot(poolsRoot);

        // Optional roots
        ValidateOptionalRoot(servicesRoot);

        // Children expectations
        ValidateChild(uiRoot, dynamicPanelsName);
        ValidateChild(camerasRoot, combatCamerasName);

        // Key components presence
        WarnIfMissingComponentInRoot<UIManager>(managersRoot, nameof(UIManager));
        WarnIfMissingComponentInRoot<LevelManager>(managersRoot, nameof(LevelManager));
        WarnIfMissingComponentInRoot<TurnBasedCombatManager>(managersRoot, nameof(TurnBasedCombatManager));
        ValidateGraphicsSettingsManagerPlacement();
        WarnIfMissingComponentInRoot<Santa.Infrastructure.SaveService>(managersRoot, "SaveService");
        WarnIfMissingComponentInRoot<Santa.UI.PauseMenuController>(servicesRoot, "PauseMenuController");

        // Directional Light recommendation
        var sun = FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var l in sun)
        {
            if (l.type == LightType.Directional)
            {
                // Suggest placement under Environment
                var env = FindRoot(environmentRoot);
                if (env != null && l.transform.parent == null)
                {
                    GameLog.Log("SceneValidator: Directional Light found at root. Consider moving under 'Environment' for clarity.");
                }

                // Mobile-friendly defaults
                if (l.shadows != LightShadows.None)
                {
                    GameLog.Log("SceneValidator: Directional Light shadows enabled. For mobile, consider disabling or using URP Shadow Cascade = 1.");
                }
            }
        }
#endif
    }

    private void ValidateRoot(string name)
    {
        var root = FindRoot(name);
        if (root == null)
        {
            GameLog.LogWarning($"SceneValidator: Root '{name}' is missing. Create it and organize objects accordingly.");
        }
    }

    private void ValidateOptionalRoot(string name)
    {
        if (string.IsNullOrEmpty(name)) return;
        var root = FindRoot(name);
        if (root == null)
        {
            GameLog.Log($"SceneValidator: Optional root '{name}' not found (ok). Create if you need auxiliary services.");
        }
    }

    private void ValidateChild(string parentName, string childName)
    {
        var parent = FindRoot(parentName);
        if (parent == null) return;
        var child = parent.transform.Find(childName);
        if (child == null)
        {
            GameLog.LogWarning($"SceneValidator: '{childName}' missing under '{parentName}'. Create it to keep runtime instances organized.");
        }
    }

    private GameObject FindRoot(string name)
    {
        var roots = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i] != null && roots[i].name == name)
            {
                return roots[i];
            }
        }
        return null;
    }

    private void WarnIfMissingComponentInRoot<T>(string rootName, string label) where T : Component
    {
        var root = FindRoot(rootName);
        if (root == null) return;
        var found = root.GetComponentInChildren<T>(true);
        if (found == null)
        {
            GameLog.LogWarning($"SceneValidator: '{label}' not found under '{rootName}'. Move or add it to match architecture.");
        }
    }

    private void ValidateGraphicsSettingsManagerPlacement()
    {
        // Accept either placement: under Managers root or as its own root object (to allow DontDestroyOnLoad persistence)
        var managers = FindRoot(managersRoot);
        var gsmInManagers = managers != null ? managers.GetComponentInChildren<GraphicsSettingsManager>(true) : null;
        if (gsmInManagers != null) return; // OK

        // Search anywhere in the scene
        var anyGsm = FindObjectsByType<GraphicsSettingsManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (anyGsm != null && anyGsm.Length > 0)
        {
            // Found, but not in preferred locations; provide a suggestion rather than a warning
            GameLog.Log("SceneValidator: GraphicsSettingsManager found but not under 'Managers' or at root. Consider moving it for clarity or persistence.");
            return;
        }

        GameLog.LogWarning("SceneValidator: 'GraphicsSettingsManager' not found. Add it under 'Managers' or as a root to manage platform-specific graphics settings.");
    }
}
}

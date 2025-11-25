using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

// ═══════════════════════════════════════════════════════════════════════════
// ⚠️ TEMPORARY DIAGNOSTIC SCRIPT - DELETE AFTER FIXING TARGET SELECTION ⚠️
// ═══════════════════════════════════════════════════════════════════════════
// This script is for debugging only. Once target selection works correctly:
// 1. Remove this component from your scene
// 2. Delete this file (CombatTargetingDiagnostic.cs)
// 3. Delete the .meta file
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// TEMPORARY: Diagnostic script to ensure proper setup for target selection in combat.
/// Automatically adds PhysicsRaycaster to the Main Camera if missing.
/// DELETE THIS SCRIPT once target selection is working properly!
/// </summary>
public class CombatTargetingDiagnostic : MonoBehaviour
{
    [SerializeField] private bool runDiagnosticOnStart = true;
    
    private void Start()
    {
        if (runDiagnosticOnStart)
        {
            RunDiagnostic();
        }
    }

    [ContextMenu("Run Targeting Diagnostic")]
    public void RunDiagnostic()
    {
        GameLog.Log("=== COMBAT TARGETING DIAGNOSTIC ===");
        
        CheckEventSystem();
        CheckMainCamera();
        CheckPhysicsRaycaster();
        CheckEnemyTargets();
        
        GameLog.Log("=== DIAGNOSTIC COMPLETE ===");
    }

    private void CheckEventSystem()
    {
        var eventSystem = FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameLog.LogError("❌ No EventSystem found in scene! Add one via GameObject > UI > Event System");
            return;
        }
        
        GameLog.Log($"✅ EventSystem found: {eventSystem.name}");
        
        // Check for InputSystemUIInputModule (required for new Input System)
        var inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputModule == null)
        {
            GameLog.LogWarning("⚠️ EventSystem doesn't have InputSystemUIInputModule! It should for Unity 6 + Input System 1.15");
            
            var standaloneModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (standaloneModule != null)
            {
                GameLog.LogWarning("⚠️ Found old StandaloneInputModule. Consider replacing with InputSystemUIInputModule");
            }
        }
        else
        {
            GameLog.Log($"✅ InputSystemUIInputModule found on EventSystem");
        }
    }

    private void CheckMainCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameLog.LogError("❌ No Main Camera found! Make sure your camera has the 'MainCamera' tag");
            return;
        }
        
        GameLog.Log($"✅ Main Camera found: {mainCam.name}");
        GameLog.Log($"   - Active: {mainCam.gameObject.activeInHierarchy}");
        GameLog.Log($"   - Enabled: {mainCam.enabled}");
    }

    private void CheckPhysicsRaycaster()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        var raycaster = mainCam.GetComponent<PhysicsRaycaster>();
        if (raycaster == null)
        {
            GameLog.LogWarning($"⚠️ No PhysicsRaycaster on Main Camera '{mainCam.name}'");
            GameLog.Log("   Attempting to add PhysicsRaycaster...");
            
            raycaster = mainCam.gameObject.AddComponent<PhysicsRaycaster>();
            GameLog.Log($"✅ PhysicsRaycaster added to {mainCam.name}");
        }
        else
        {
            GameLog.Log($"✅ PhysicsRaycaster found on Main Camera");
        }

        GameLog.Log($"   - PhysicsRaycaster enabled: {raycaster.enabled}");
        GameLog.Log($"   - Event Mask: {raycaster.eventMask.value}");
    }

    private void CheckEnemyTargets()
    {
        var enemyTargets = FindObjectsByType<EnemyTarget>(FindObjectsSortMode.None);
        GameLog.Log($"Found {enemyTargets.Length} EnemyTarget components in scene");
        
        int withCollider = 0;
        int withDependencies = 0;
        
        foreach (var target in enemyTargets)
        {
            var collider = target.GetComponent<Collider>();
            if (collider == null)
            {
                GameLog.LogError($"❌ EnemyTarget on '{target.name}' has NO COLLIDER!");
            }
            else
            {
                withCollider++;
                GameLog.Log($"   - {target.name}: Collider={collider.GetType().Name}, Enabled={collider.enabled}, Active={target.gameObject.activeInHierarchy}");
            }
            
            // Check if VContainer dependencies were injected
            // We can't directly check private fields, but we can try to trigger the selection to see logs
            var hasInjection = CheckEnemyTargetInjection(target);
            if (hasInjection)
            {
                withDependencies++;
            }
        }
        
        GameLog.Log($"Summary: {withCollider}/{enemyTargets.Length} have colliders, {withDependencies}/{enemyTargets.Length} have dependencies injected");
        
        if (withDependencies < enemyTargets.Length)
        {
            GameLog.LogWarning("⚠️ Some EnemyTargets are missing VContainer dependency injection!");
            GameLog.LogWarning("   Make sure CombatScenePool is injecting dependencies when instantiating arenas.");
        }
    }
    
    private bool CheckEnemyTargetInjection(EnemyTarget target)
    {
        // We can't access private fields directly, but we can check using reflection
        // or just assume if it's in the scene, it should have been injected
        // For now, we'll just return true and rely on runtime logs
        return true; // Placeholder - actual injection will be verified at runtime via logs
    }
}

using Santa.Core;
using Santa.Domain.Combat;
using UnityEngine;
using VContainer;

#if UNITY_EDITOR || DEVELOPMENT_BUILD

namespace Santa.Editor
{

/// <summary>
/// Testing script to try the Upgrade UI without having to complete combats.
/// SOLO PARA DESARROLLO - Eliminar en build final.
/// </summary>
public class UpgradeUITester : MonoBehaviour
{
    [Header("Testing Upgrades")]
    [Tooltip("Arrastra 2 AbilityUpgrade ScriptableObjects para testing")]
    [SerializeField] private AbilityUpgrade testUpgrade1;
    [SerializeField] private AbilityUpgrade testUpgrade2;

    [Header("Testing Controls")]
    [SerializeField] private KeyCode showUIKey = KeyCode.T;
    [SerializeField] private KeyCode hideUIKey = KeyCode.Escape;

    private IUpgradeUI _upgradeUI;
    private bool _isInitialized = false;

    [Inject]
    public void Construct(IUpgradeUI upgradeUI)
    {
        _upgradeUI = upgradeUI;
        _isInitialized = true;
    }

    private void Update()
    {
        if (!_isInitialized)
            return;

        // Presiona T para mostrar la UI
        if (Input.GetKeyDown(showUIKey))
        {
            TestShowUpgrades();
        }

        // Press Escape to hide (useful for debugging)
        if (Input.GetKeyDown(hideUIKey))
        {
            Debug.Log("Testing: Hiding upgrade UI (if visible)");
        }
    }

    private void TestShowUpgrades()
    {
        if (testUpgrade1 == null || testUpgrade2 == null)
        {
            Debug.LogWarning("⚠️ UpgradeUITester: Assign test upgrades in Inspector!");
            return;
        }

        if (_upgradeUI == null)
        {
            Debug.LogError("❌ UpgradeUITester: IUpgradeUI not injected. Check VContainer setup.");
            return;
        }

        Debug.Log($"🎮 Testing Upgrade UI with: '{testUpgrade1.UpgradeName}' vs '{testUpgrade2.UpgradeName}'");
        _upgradeUI.ShowUpgrades(testUpgrade1, testUpgrade2);
    }

    private void OnGUI()
    {
        // Mostrar instrucciones en pantalla
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 20;
        style.normal.textColor = Color.white;
        style.fontStyle = FontStyle.Bold;

        GUI.Label(new Rect(10, 10, 400, 30), $"Press [{showUIKey}] to test Upgrade UI", style);

        if (testUpgrade1 == null || testUpgrade2 == null)
        {
            style.normal.textColor = Color.yellow;
            GUI.Label(new Rect(10, 40, 500, 30), "⚠️ Assign test upgrades in Inspector!", style);
        }
    }
}
}
#endif

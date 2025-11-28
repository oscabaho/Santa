using UnityEngine;
using VContainer;

#if UNITY_EDITOR || DEVELOPMENT_BUILD

/// <summary>
/// Example of manually preloading the UpgradeUI at the start of a combat level.
/// This avoids a delay when the UI is shown after winning.
/// 
/// INSTRUCTIONS:
/// 1. Add this script to a GameObject at the start of your combat level
/// 2. Or call it from your combat start script
/// </summary>
public class PreloadUpgradeUIOnCombatStart : MonoBehaviour
{
    private UpgradeUILoader _upgradeUILoader;

    [Inject]
    public void Construct(UpgradeUILoader upgradeUILoader)
    {
        _upgradeUILoader = upgradeUILoader;
    }

    private async void Start()
    {
        // Preload the UI in background at combat start
        GameLog.Log("PreloadUpgradeUIOnCombatStart: Preloading UpgradeUI...");
        await _upgradeUILoader.PreloadAsync();
        GameLog.Log("PreloadUpgradeUIOnCombatStart: Preload completed.");
    }
}

#endif

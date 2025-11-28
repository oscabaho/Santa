using UnityEngine;
using VContainer;

#if UNITY_EDITOR || DEVELOPMENT_BUILD

/// <summary>
/// Example showing how to release UpgradeUI resources when returning to the main menu.
/// This releases memory not needed in the menu.
/// 
/// INSTRUCCIONES:
/// 1. Add this script to a GameObject in your main menu
/// 2. Or call it from your menu transition script
/// </summary>
public class ReleaseUpgradeUIOnMenuReturn : MonoBehaviour
{
    private UpgradeUILoader _upgradeUILoader;

    [Inject]
    public void Construct(UpgradeUILoader upgradeUILoader)
    {
        _upgradeUILoader = upgradeUILoader;
    }

    private void Start()
    {
        // Release resources when returning to the menu
        GameLog.Log("ReleaseUpgradeUIOnMenuReturn: Releasing UpgradeUI resources...");
        _upgradeUILoader.Release();
        GameLog.Log("ReleaseUpgradeUIOnMenuReturn: Resources released.");
    }
}

#endif

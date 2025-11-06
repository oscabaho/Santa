using UnityEngine;
using VContainer;

#if UNITY_EDITOR || DEVELOPMENT_BUILD

/// <summary>
/// Ejemplo de cómo precargar manualmente la UpgradeUI al inicio de un nivel de combate.
/// Esto evita delay cuando se muestre la UI después de ganar.
/// 
/// INSTRUCCIONES:
/// 1. Añade este script a un GameObject al inicio de tu nivel de combate
/// 2. O llámalo desde tu script de inicio de combate
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
        // Precargar la UI en background al inicio del combate
        GameLog.Log("PreloadUpgradeUIOnCombatStart: Preloading UpgradeUI...");
        await _upgradeUILoader.PreloadAsync();
        GameLog.Log("PreloadUpgradeUIOnCombatStart: Preload completed.");
    }
}

#endif

using UnityEngine;
using VContainer;

#if UNITY_EDITOR || DEVELOPMENT_BUILD

/// <summary>
/// Ejemplo de cómo liberar los recursos de UpgradeUI al volver al menú principal.
/// Esto libera memoria que no se necesita en el menú.
/// 
/// INSTRUCCIONES:
/// 1. Añade este script a un GameObject en tu menú principal
/// 2. O llámalo desde tu script de transición al menú
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
        // Liberar recursos al volver al menú
        GameLog.Log("ReleaseUpgradeUIOnMenuReturn: Releasing UpgradeUI resources...");
        _upgradeUILoader.Release();
        GameLog.Log("ReleaseUpgradeUIOnMenuReturn: Resources released.");
    }
}

#endif

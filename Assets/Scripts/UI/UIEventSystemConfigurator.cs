using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// Ensures the EventSystem is correctly configured for the new Input System UI.
/// - Adds EventSystem if missing
/// - Adds InputSystemUIInputModule if missing
/// - Assigns the provided InputActionAsset (expects a map named "UI" with Point/Click/etc.)
/// - Disables StandaloneInputModule (old) when using the new system
/// </summary>
public class UIEventSystemConfigurator : MonoBehaviour
{
#if ENABLE_INPUT_SYSTEM
    [Header("Assign your Input Actions asset (ActionMap.inputactions)")]
    [SerializeField] private InputActionAsset actionsAsset;
#endif

    private void Awake()
    {
        var es = EventSystem.current;
        if (es == null)
        {
            es = gameObject.GetComponent<EventSystem>();
            if (es == null)
            {
                es = gameObject.AddComponent<EventSystem>();
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.Log("UIEventSystemConfigurator: Added EventSystem.", this);
                #endif
            }
        }

#if ENABLE_INPUT_SYSTEM
        var go = es.gameObject;
        var inputModule = go.GetComponent<InputSystemUIInputModule>();
        if (inputModule == null)
        {
            inputModule = go.AddComponent<InputSystemUIInputModule>();
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log("UIEventSystemConfigurator: Added InputSystemUIInputModule.", this);
            #endif
        }

        if (actionsAsset != null)
        {
            inputModule.actionsAsset = actionsAsset;
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"UIEventSystemConfigurator: Assigned actions asset '{actionsAsset.name}' to InputSystemUIInputModule.", this);
            #endif
        }
        else
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning("UIEventSystemConfigurator: No actions asset assigned. Drag 'ActionMap.inputactions' into the component.", this);
            #endif
        }

        var standalone = go.GetComponent<StandaloneInputModule>();
        if (standalone != null && standalone.enabled)
        {
            standalone.enabled = false; // Old input module not needed with New input system
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log("UIEventSystemConfigurator: Disabled StandaloneInputModule (using New Input System).", this);
            #endif
        }
#else
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.LogWarning("UIEventSystemConfigurator: New Input System not enabled in this build. Consider switching Active Input Handling to 'Input System Package (New)'.", this);
#endif
#endif
    }
}

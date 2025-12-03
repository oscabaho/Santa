using UnityEngine;
using UnityEngine.InputSystem;

namespace Santa.Infrastructure.Input
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Santa/Input Reader")]
public class InputReader : ScriptableObject, @ActionMap.IPlayerActions
{
    public event System.Action<Vector2> MoveEvent;
    public event System.Action InteractEvent;
    public event System.Action PauseEvent;

    private ActionMap _actionMap;

    private void OnEnable()
    {
        if (_actionMap == null)
        {
            _actionMap = new ActionMap();
            _actionMap.Player.SetCallbacks(this);
        }
        // Enable gameplay map (movement, look, interact)
        _actionMap.Player.Enable();
        // ALSO enable UI map so InputSystemUIInputModule receives Point/Click actions.
        // Without this, buttons and other UI won't dispatch events even if an actions asset is assigned.
        if (!_actionMap.UI.enabled)
        {
            _actionMap.UI.Enable();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"InputReader '{name}': Enabled UI action map for pointer/click processing.");
#endif
        }
    }

    private void OnDisable()
    {
        _actionMap.Player.Disable();
        _actionMap.UI.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnLook(InputAction.CallbackContext context) { }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            InteractEvent?.Invoke();
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            PauseEvent?.Invoke();
        }
    }

    /// <summary>
    /// Allows non-InputSystem UI (e.g., on-screen buttons) to trigger the same interaction flow.
    /// Safe to call from UI button onClick.
    /// </summary>
    public void RaiseInteract()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"InputReader '{name}': RaiseInteract invoked.");
#endif
        InteractEvent?.Invoke();
    }

    public void RaisePause()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"InputReader '{name}': RaisePause invoked.");
#endif
        PauseEvent?.Invoke();
    }
}
}

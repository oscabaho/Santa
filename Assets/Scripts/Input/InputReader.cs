using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "Santa/Input Reader")]
public class InputReader : ScriptableObject, ActionMap.IPlayerActions
{
    public event System.Action<Vector2> MoveEvent;
    public event System.Action JumpEvent;

    private ActionMap _actionMap;

    private void OnEnable()
    {
        if (_actionMap == null)
        {
            _actionMap = new ActionMap();
            _actionMap.Player.SetCallbacks(this);
        }
        _actionMap.Player.Enable();
    }

    private void OnDisable()
    {
        _actionMap.Player.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            JumpEvent?.Invoke();
        }
    }

    // The interface requires us to implement all methods, even if they are empty.
    public void OnLook(InputAction.CallbackContext context) { }
    public void OnAttack(InputAction.CallbackContext context) { }
    public void OnInteract(InputAction.CallbackContext context) { }
    public void OnCrouch(InputAction.CallbackContext context) { }
    public void OnPrevious(InputAction.CallbackContext context) { }
    public void OnNext(InputAction.CallbackContext context) { }
    public void OnSprint(InputAction.CallbackContext context) { }
}

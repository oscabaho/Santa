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
#if UNITY_EDITOR && GAME_LOGS_VERBOSE
                GameLog.LogVerbose($"InputReader '{name}': ActionMap created.", this);
#endif
            }

            // Enable gameplay map (movement, look, interact)
            try
            {
                _actionMap.Player.Enable();
#if UNITY_EDITOR && GAME_LOGS_VERBOSE
                GameLog.LogVerbose($"InputReader '{name}': Player action map enabled.", this);
#endif
            }
            catch (System.Exception ex)
            {
                GameLog.LogError($"InputReader: Failed to enable Player action map! {ex.Message}", this);
            }

            // ALSO enable UI map so InputSystemUIInputModule receives Point/Click actions.
            // Without this, buttons and other UI won't dispatch events even if an actions asset is assigned.
            if (!_actionMap.UI.enabled)
            {
                try
                {
                    _actionMap.UI.Enable();
#if UNITY_EDITOR && GAME_LOGS_VERBOSE
                    GameLog.LogVerbose($"InputReader '{name}': UI action map enabled for pointer/click processing.");
#endif
                }
                catch (System.Exception ex)
                {
                    GameLog.LogError($"InputReader: Failed to enable UI action map! {ex.Message}", this);
                }
            }

#if UNITY_EDITOR && GAME_LOGS_VERBOSE
            GameLog.LogVerbose($"InputReader '{name}': OnEnable complete.", this);
#endif
        }

        private void OnDisable()
        {
            try
            {
                _actionMap.Player.Disable();
                _actionMap.UI.Disable();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose($"InputReader '{name}': Action maps disabled.", this);
#endif
            }
            catch (System.Exception ex)
            {
                GameLog.LogError($"InputReader: Error disabling action maps! {ex.Message}", this);
            }
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
        /// 
        /// CRITICAL FOR MOBILE: This is how the action button triggers combat.
        /// If this is not called or the event is not received, combat won't start.
        /// </summary>
        public void RaiseInteract()
        {
            int subscriberCount = InteractEvent?.GetInvocationList()?.Length ?? 0;
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"InputReader '{name}': RaiseInteract invoked. Subscribers={subscriberCount}");
#else
            // CRITICAL: Log subscriber count even in Release for APK mobile debugging
            GameLog.Log($"InputReader: RaiseInteract -> Subscribers={subscriberCount}");
#endif
            
            if (subscriberCount == 0)
            {
                // This is why mobile might fail - PlayerInteraction not subscribed
                GameLog.LogError("InputReader: CRITICAL - No subscribers to InteractEvent! PlayerInteraction may not be active or InputReader was not initialized properly.", this);
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose($"InputReader '{name}': Invoking InteractEvent with {subscriberCount} subscriber(s).", this);
#else
                GameLog.Log($"InputReader: Invoking InteractEvent");
#endif
            }
            
            InteractEvent?.Invoke();
        }

        public void RaisePause()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"InputReader '{name}': RaisePause invoked.");
#endif
            PauseEvent?.Invoke();
        }

        public void DisableGameplayInput()
        {
            _actionMap.Player.Disable();
        }

        public void EnableGameplayInput()
        {
            _actionMap.Player.Enable();
        }
    }
}

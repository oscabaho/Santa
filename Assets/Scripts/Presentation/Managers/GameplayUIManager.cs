using System;
using Santa.Core;
using UnityEngine;

namespace Santa.Presentation.UI
{
    public class GameplayUIManager : MonoBehaviour, IGameplayUIService
    {
        // The actual button object, assigned at runtime.
        private GameObject actionButtonGameObject;
        // A "queued" or "buffered" state for the button, in case ShowActionButton is called before the button is registered.
        private bool? _queuedShowState = null;
        // Tracks the most recent desired visibility regardless of registration state.
        private bool? _lastRequestedShowState = null;
        private bool _isReady;

        public bool IsReady => _isReady;
        public event Action Ready;

        private void Start()
        {
            // The button will now register itself. We can add a check here later if needed.
        }

        public void RegisterActionButton(GameObject button)
        {
            if (button == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogError("An attempt was made to register a null Action Button.", this);
#endif
                return;
            }

            if (actionButtonGameObject == button)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.Log("GameplayUIManager: RegisterActionButton called with the existing instance; ignoring duplicate.", this);
#endif

                if (!_isReady)
                {
                    _isReady = true;
                    TryNotifyReady();
                }
                return;
            }

            actionButtonGameObject = button;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log("Action Button was registered successfully.", this);
#endif

            // Apply the last known desired state, with a preference for a recently queued state.
            // If no state has ever been requested, default to inactive.
            bool initialState = _queuedShowState ?? _lastRequestedShowState ?? false;
            actionButtonGameObject.SetActive(initialState);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"GameplayUIManager: Applied initial state ShowActionButton={initialState} on registration.", this);
#endif
            _queuedShowState = null; // Clear the queued state

            // Mark as ready and notify listeners
            if (!_isReady)
            {
                _isReady = true;
                TryNotifyReady();
            }
        }

        public void ShowActionButton(bool show)
        {
            _lastRequestedShowState = show; // Always record latest intent.
            if (actionButtonGameObject != null)
            {
                // If the button is ready, just set its state directly.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.Log($"GameplayUIManager: ShowActionButton({show}).", this);
#endif
                actionButtonGameObject.SetActive(show);
            }
            else
            {
                // If the button is not ready, queue the desired state.
                _queuedShowState = show;
                // Downgrade to info: callers should gate on IsReady/Ready; this is just a harmless queue.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.Log($"GameplayUIManager: ShowActionButton({show}) called before registration. Queued the desired state.", this);
#endif
            }
        }

        public void UnregisterActionButton(GameObject button)
        {
            if (button == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning("GameplayUIManager: Attempted to unregister a null Action Button.", this);
#endif
                return;
            }

            if (actionButtonGameObject != button)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning($"GameplayUIManager: Attempted to unregister an unknown Action Button '{button.name}'.", this);
#endif
                return;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log("GameplayUIManager: Action Button unregistered. UI will wait for a new registration.", this);
#endif
            actionButtonGameObject = null;
            _isReady = false;
        }

        private void TryNotifyReady()
        {
            try
            {
                Ready?.Invoke();
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogError($"GameplayUIManager.Ready event handler threw: {ex.Message}", this);
#else
            _ = ex;
#endif
            }
        }
    }
}

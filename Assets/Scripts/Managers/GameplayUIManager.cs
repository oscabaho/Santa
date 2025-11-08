using System;
using UnityEngine;

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
            GameLog.LogError("An attempt was made to register a null Action Button.", this);
            return;
        }

        if (actionButtonGameObject == button)
        {
            GameLog.Log("GameplayUIManager: RegisterActionButton called with the existing instance; ignoring duplicate.", this);

            if (!_isReady)
            {
                _isReady = true;
                TryNotifyReady();
            }
            return;
        }

        actionButtonGameObject = button;
        GameLog.Log("Action Button was registered successfully.", this);

        // Apply the last known desired state, with a preference for a recently queued state.
        // If no state has ever been requested, default to inactive.
        bool initialState = _queuedShowState ?? _lastRequestedShowState ?? false;
        actionButtonGameObject.SetActive(initialState);
        GameLog.Log($"GameplayUIManager: Applied initial state ShowActionButton={initialState} on registration.", this);
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
            GameLog.Log($"GameplayUIManager: ShowActionButton({show}).", this);
            actionButtonGameObject.SetActive(show);
        }
        else
        {
            // If the button is not ready, queue the desired state.
            _queuedShowState = show;
            // Downgrade to info: callers should gate on IsReady/Ready; this is just a harmless queue.
            GameLog.Log($"GameplayUIManager: ShowActionButton({show}) called before registration. Queued the desired state.", this);
        }
    }

    public void UnregisterActionButton(GameObject button)
    {
        if (button == null)
        {
            GameLog.LogWarning("GameplayUIManager: Attempted to unregister a null Action Button.", this);
            return;
        }

        if (actionButtonGameObject != button)
        {
            GameLog.LogWarning($"GameplayUIManager: Attempted to unregister an unknown Action Button '{button.name}'.", this);
            return;
        }

        GameLog.Log("GameplayUIManager: Action Button unregistered. UI will wait for a new registration.", this);
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
            GameLog.LogError($"GameplayUIManager.Ready event handler threw: {ex.Message}", this);
        }
    }
}

using UnityEngine;

public class GameplayUIManager : MonoBehaviour, IGameplayUIService
{
    // The actual button object, assigned at runtime.
    private GameObject actionButtonGameObject;
    // A "queued" or "buffered" state for the button, in case ShowActionButton is called before the button is registered.
    private bool? _queuedShowState = null;

    private void Start()
    {
        // The button will now register itself. We can add a check here later if needed.
    }

    public void RegisterActionButton(GameObject button)
    {
        if (button != null)
        {
            actionButtonGameObject = button;
            GameLog.Log("Action Button was registered successfully.", this);

            // If a state was queued before the button was ready, apply it now.
            if (_queuedShowState.HasValue)
            {
                actionButtonGameObject.SetActive(_queuedShowState.Value);
                _queuedShowState = null; // Clear the queued state
            }
            else
            {
                // If no state was queued, ensure it starts as inactive.
                actionButtonGameObject.SetActive(false);
            }
        }
        else
        {
            GameLog.LogError("An attempt was made to register a null Action Button.", this);
        }
    }

    public void ShowActionButton(bool show)
    {
        if (actionButtonGameObject != null)
        {
            // If the button is ready, just set its state directly.
            actionButtonGameObject.SetActive(show);
        }
        else
        {
            // If the button is not ready, queue the desired state.
            _queuedShowState = show;
            GameLog.LogWarning("ShowActionButton was called, but the button is not yet registered. The state has been queued.", this);
        }
    }
}

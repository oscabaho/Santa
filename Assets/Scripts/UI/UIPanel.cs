using UnityEngine;

/// <summary>
/// Base component for a UI Panel, using a CanvasGroup for efficient show/hide functionality.
/// This component requires a CanvasGroup to be attached to the same GameObject.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class UIPanel : MonoBehaviour
{
    private CanvasGroup _canvasGroup;

    /// <summary>
    /// Ensures the CanvasGroup component is cached.
    /// </summary>
    protected virtual void Awake()
    {
        Debug.Log($"UIPanel.Awake() called for {gameObject.name}", gameObject);
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            // This should not happen due to [RequireComponent]
            Debug.LogError($"UIPanel on {gameObject.name} requires a CanvasGroup component.", this);
        }
    }

    /// <summary>
    /// Makes the panel visible and interactive.
    /// Can be overridden for custom show animations (e.g., fading in).
    /// </summary>
    public virtual void Show()
    {
        Debug.Log($"UIPanel.Show() called for {gameObject.name}", gameObject);
        gameObject.SetActive(true);
        
        // Ensure canvas group is fetched, as Awake might not have been called if the object was inactive.
        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        if (_canvasGroup == null)
        {
            Debug.LogError($"UIPanel.Show() on {gameObject.name}: CanvasGroup is null and could not be found.", this);
            return;
        }
        
        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        Debug.Log($"UIPanel.Show() finished for {gameObject.name}. Is active: {gameObject.activeSelf}", gameObject);
    }

    /// <summary>
    /// Hides the panel and makes it non-interactive.
    /// Can be overridden for custom hide animations (e.g., fading out).
    /// </summary>
    public virtual void Hide()
     {
        Debug.Log($"UIPanel.Hide() called for {gameObject.name}", gameObject);
        
        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        if (_canvasGroup == null)
        {
            Debug.LogError($"UIPanel.Hide() on {gameObject.name}: CanvasGroup is null and could not be found.", this);
            return;
        }

        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
        Debug.Log($"UIPanel.Hide() finished for {gameObject.name}. Is active: {gameObject.activeSelf}", gameObject);
    }
}

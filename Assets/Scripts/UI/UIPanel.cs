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
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            // This should not happen due to [RequireComponent]
            Debug.LogError("UIPanel requires a CanvasGroup component.", this);
        }
    }

    /// <summary>
    /// Makes the panel visible and interactive.
    /// Can be overridden for custom show animations (e.g., fading in).
    /// </summary>
    public virtual void Show()
    {
        if (_canvasGroup == null) return;
        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// Hides the panel and makes it non-interactive.
    /// Can be overridden for custom hide animations (e.g., fading out).
    /// </summary>
    public virtual void Hide()
    {
                if (_canvasGroup == null) return;
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }
}
using UnityEngine;

/// <summary>
/// Base component for a UI Panel, using a CanvasGroup for efficient show/hide functionality.
/// This component requires a CanvasGroup to be attached to the same GameObject.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class UIPanel : MonoBehaviour
{
    protected CanvasGroup CanvasGroup { get; private set; }

    /// <summary>
    /// Ensures the CanvasGroup component is cached.
    /// </summary>
    protected virtual void Awake()
    {
        GameLog.Log($"UIPanel.Awake() called for {gameObject.name}", gameObject);
        CanvasGroup = GetComponent<CanvasGroup>();
        if (CanvasGroup == null)
        {
            // This should not happen due to [RequireComponent]
            GameLog.LogError($"UIPanel on {gameObject.name} requires a CanvasGroup component.", this);
        }
    }

    /// <summary>
    /// Makes the panel visible and interactive.
    /// Can be overridden for custom show animations (e.g., fading in).
    /// </summary>
    public virtual void Show()
    {
        GameLog.Log($"UIPanel.Show() called for {gameObject.name}", gameObject);

        // Ensure canvas group is fetched, as Awake might not have been called if the object was inactive.
        if (CanvasGroup == null)
        {
            CanvasGroup = GetComponent<CanvasGroup>();
        }

        if (CanvasGroup == null)
        {
            GameLog.LogError($"UIPanel.Show() on {gameObject.name}: CanvasGroup is null and could not be found.", this);
            return;
        }

        CanvasGroup.alpha = 1f;
        CanvasGroup.interactable = true;
        CanvasGroup.blocksRaycasts = true;

        gameObject.SetActive(true);
        GameLog.Log($"UIPanel.Show() finished for {gameObject.name}. Is active: {gameObject.activeSelf}", gameObject);
    }

    /// <summary>
    /// Hides the panel and makes it non-interactive.
    /// Can be overridden for custom hide animations (e.g., fading out).
    /// </summary>
    public virtual void Hide()
    {
        GameLog.Log($"UIPanel.Hide() called for {gameObject.name}", gameObject);

        if (CanvasGroup == null)
        {
            CanvasGroup = GetComponent<CanvasGroup>();
        }

        if (CanvasGroup == null)
        {
            GameLog.LogError($"UIPanel.Hide() on {gameObject.name}: CanvasGroup is null and could not be found.", this);
            return;
        }

        CanvasGroup.alpha = 0f;
        CanvasGroup.interactable = false;
        CanvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
        GameLog.Log($"UIPanel.Hide() finished for {gameObject.name}. Is active: {gameObject.activeSelf}", gameObject);
    }
}

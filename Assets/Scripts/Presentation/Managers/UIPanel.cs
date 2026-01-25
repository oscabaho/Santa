using UnityEngine;
using Cysharp.Threading.Tasks;
using Santa.UI;
using Santa.Presentation.Menus;

namespace Santa.Presentation.UI
{

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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"UIPanel.Awake() called for {gameObject.name}", gameObject);
#endif
        CanvasGroup = GetComponent<CanvasGroup>();
        if (CanvasGroup == null)
        {
            // This should not happen due to [RequireComponent]
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError($"UIPanel on {gameObject.name} requires a CanvasGroup component.", this);
#endif
        }
    }

    /// <summary>
    /// Makes the panel visible and interactive.
    /// Can be overridden for custom show animations (e.g., fading in).
    /// </summary>
    public virtual void Show()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"UIPanel.Show() called for {gameObject.name}", gameObject);
#endif

        // Ensure canvas group is fetched, as Awake might not have been called if the object was inactive.
        if (CanvasGroup == null)
        {
            CanvasGroup = GetComponent<CanvasGroup>();
        }

        if (CanvasGroup == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError($"UIPanel.Show() on {gameObject.name}: CanvasGroup is null and could not be found.", this);
#endif
            return;
        }

                var animator = GetComponent<PauseMenuAnimator>();
                if (animator != null)
                {
                        animator.FadeIn().Forget();
                }
                else
                {
                        CanvasGroup.alpha = 1f;
                        CanvasGroup.interactable = true;
                        CanvasGroup.blocksRaycasts = true;
                }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"UIPanel.Show() finished for {gameObject.name}.", gameObject);
#endif
    }

    /// <summary>
    /// Hides the panel and makes it non-interactive.
    /// Can be overridden for custom hide animations (e.g., fading out).
    /// </summary>
    public virtual void Hide()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"UIPanel.Hide() called for {gameObject.name}", gameObject);
#endif

        if (CanvasGroup == null)
        {
            CanvasGroup = GetComponent<CanvasGroup>();
        }

        if (CanvasGroup == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError($"UIPanel.Hide() on {gameObject.name}: CanvasGroup is null and could not be found.", this);
#endif
            return;
        }

                var animator = GetComponent<PauseMenuAnimator>();
                if (animator != null)
                {
                        animator.FadeOut().Forget();
                }
                else
                {
                        CanvasGroup.alpha = 0f;
                        CanvasGroup.interactable = false;
                        CanvasGroup.blocksRaycasts = false;
                }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"UIPanel.Hide() finished for {gameObject.name}.", gameObject);
#endif
    }
    /// <summary>
    /// Hides the panel immediately without animation (used for preloading).
    /// </summary>
    public virtual void HideImmediate()
    {
        if (CanvasGroup == null)
        {
            CanvasGroup = GetComponent<CanvasGroup>();
        }

        if (CanvasGroup != null)
        {
            CanvasGroup.alpha = 0f;
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
        }
    }
}
}

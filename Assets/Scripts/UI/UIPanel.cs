using UnityEngine;

/// <summary>
/// Component to identify a UI Panel, with basic show/hide functionality.
/// </summary>
public class UIPanel : MonoBehaviour
{
    [SerializeField] private string panelId;
    public string PanelId => panelId;

    /// <summary>
    /// Activates the panel. Can be overridden for custom show animations.
    /// </summary>
    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Deactivates the panel. Can be overridden for custom hide animations.
    /// </summary>
    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }
}

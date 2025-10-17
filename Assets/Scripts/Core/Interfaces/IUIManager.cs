using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

/// <summary>
/// Interface for a UI Manager that can show, hide, and switch between UI panels.
/// </summary>
public interface IUIManager
{
    /// <summary>
    /// Shows a specific panel without hiding others.
    /// </summary>
    /// <param name="panelReference">A direct reference to the panel prefab to show.</param>
    Task ShowPanel(AssetReferenceGameObject panelReference);

    /// <summary>
    /// Hides a specific panel.
    /// </summary>
    /// <param name="panelReference">A direct reference to the panel prefab to hide.</param>
    void HidePanel(AssetReferenceGameObject panelReference);

    /// <summary>
    /// Hides all other panels and shows the specified one.
    /// </summary>
    /// <param name="panelReference">A direct reference to the panel prefab to switch to.</param>
    Task SwitchToPanel(AssetReferenceGameObject panelReference);
}
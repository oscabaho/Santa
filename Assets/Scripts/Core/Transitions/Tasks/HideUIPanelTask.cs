using System.Collections;
using UnityEngine;

/// <summary>
/// A transition task that hides a specific UI panel via the UIManager.
/// </summary>
[CreateAssetMenu(fileName = "NewHideUIPanelTask", menuName = "Transitions/Tasks/Hide UI Panel")]
public class HideUIPanelTask : TransitionTask
{
    [SerializeField]
    private string panelAddress;

    public override IEnumerator Execute(TransitionContext context)
    {
        if (string.IsNullOrEmpty(panelAddress))
        {
            GameLog.LogError("HideUIPanelTask: Panel Address is not valid.");
            yield break;
        }

        var uiManager = context.GetFromContext<IUIManager>("UIManager");
        if (uiManager != null)
        {
            uiManager.HidePanel(panelAddress);
        }
        else
        {
            GameLog.LogError("HideUIPanelTask: IUIManager not found in TransitionContext.");
        }
        
        yield break;
    }
}
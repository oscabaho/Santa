using System.Collections;
using UnityEngine;

/// <summary>
/// A transition task that shows a specific UI panel via the UIManager.
/// </summary>
[CreateAssetMenu(fileName = "NewSwitchUIPanelTask", menuName = "Transitions/Tasks/Switch UI Panel")]
public class SwitchUIPanelTask : TransitionTask
{
    [SerializeField]
    private string panelAddress;

    public override IEnumerator Execute(TransitionContext context)
    {
        if (string.IsNullOrEmpty(panelAddress))
        {
            GameLog.LogError("SwitchUIPanelTask: Panel Address is not valid.");
            yield break;
        }

        var uiManager = context.GetFromContext<IUIManager>("UIManager");
        if (uiManager != null)
        {
            var task = uiManager.SwitchToPanel(panelAddress);
            if (task != null)
            {
                yield return new WaitUntil(() => task.IsCompleted);
            }
        }
        else
        {
            GameLog.LogError("SwitchUIPanelTask: IUIManager not found in TransitionContext.");
        }
    }
}

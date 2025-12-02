using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;

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
            yield return uiManager.SwitchToPanel(panelAddress).ToCoroutine();
        }
        else
        {
            GameLog.LogError("SwitchUIPanelTask: IUIManager not found in TransitionContext.");
        }
    }
}

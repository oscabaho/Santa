using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// A transition task that shows a specific UI panel via the UIManager.
/// </summary>
[CreateAssetMenu(fileName = "NewSwitchUIPanelTask", menuName = "Transitions/Tasks/Switch UI Panel")]
public class SwitchUIPanelTask : TransitionTask
{
    [SerializeField]
    private string panelAddress;

    public override async UniTask Execute(TransitionContext context)
    {
        if (string.IsNullOrEmpty(panelAddress))
        {
            GameLog.LogError("SwitchUIPanelTask: Panel Address is not valid.");
            return;
        }

        var uiManager = context.GetFromContext<IUIManager>("UIManager");
        if (uiManager != null)
        {
            await uiManager.SwitchToPanel(panelAddress);
        }
        else
        {
            GameLog.LogError("SwitchUIPanelTask: IUIManager not found in TransitionContext.");
        }
    }
}

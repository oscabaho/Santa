using System.Collections;
using UnityEngine;

/// <summary>
/// A transition task that shows a specific UI panel via the UIManager.
/// </summary>
[CreateAssetMenu(fileName = "NewSwitchUIPanelTask", menuName = "Transitions/Tasks/Switch UI Panel")]
public class SwitchUIPanelTask : TransitionTask
{
    [SerializeField]
    private string panelId;

    public override IEnumerator Execute(TransitionContext context)
    {
        ServiceLocator.Get<IUIManager>()?.SwitchToPanel(panelId);
        yield break;
    }
}

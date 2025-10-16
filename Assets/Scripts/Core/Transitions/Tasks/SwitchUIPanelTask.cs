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
        var task = ServiceLocator.Get<IUIManager>()?.SwitchToPanel(panelId);
        if (task != null)
        {
            yield return new WaitUntil(() => task.IsCompleted);
        }
    }
}

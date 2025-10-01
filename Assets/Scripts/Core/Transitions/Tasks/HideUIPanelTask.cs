using System.Collections;
using UnityEngine;

/// <summary>
/// A transition task that hides a specific UI panel via the UIManager.
/// </summary>
[CreateAssetMenu(fileName = "NewHideUIPanelTask", menuName = "Transitions/Tasks/Hide UI Panel")]
public class HideUIPanelTask : TransitionTask
{
    [SerializeField]
    private string panelId;

    public override IEnumerator Execute(TransitionContext context)
    {
        ServiceLocator.Get<IUIManager>()?.HidePanel(panelId);
        yield break;
    }
}
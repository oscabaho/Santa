using System.Collections;
using UnityEngine;

/// <summary>
/// A transition task that shows a specific UI panel via the UIManager without hiding others.
/// </summary>
[CreateAssetMenu(fileName = "NewShowUIPanelTask", menuName = "Transitions/Tasks/Show UI Panel")]
public class ShowUIPanelTask : TransitionTask
{
    [SerializeField]
    private string panelId;

    public override IEnumerator Execute(TransitionContext context)
    {
        ServiceLocator.Get<IUIManager>()?.ShowPanel(panelId);
        yield break;
    }
}
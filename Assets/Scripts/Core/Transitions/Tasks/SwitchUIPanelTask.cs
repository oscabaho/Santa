using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// A transition task that shows a specific UI panel via the UIManager.
/// </summary>
[CreateAssetMenu(fileName = "NewSwitchUIPanelTask", menuName = "Transitions/Tasks/Switch UI Panel")]
public class SwitchUIPanelTask : TransitionTask
{
    [SerializeField]
    private AssetReferenceGameObject panelReference;

    public override IEnumerator Execute(TransitionContext context)
    {
        if (panelReference == null || !panelReference.RuntimeKeyIsValid())
        {
            Debug.LogError("SwitchUIPanelTask: Panel Reference is not valid.");
            yield break;
        }

        var task = ServiceLocator.Get<IUIManager>()?.SwitchToPanel(panelReference);
        if (task != null)
        {
            yield return new WaitUntil(() => task.IsCompleted);
        }
    }
}

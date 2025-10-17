using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// A transition task that hides a specific UI panel via the UIManager.
/// </summary>
[CreateAssetMenu(fileName = "NewHideUIPanelTask", menuName = "Transitions/Tasks/Hide UI Panel")]
public class HideUIPanelTask : TransitionTask
{
    [SerializeField]
    private AssetReferenceGameObject panelReference;

    public override IEnumerator Execute(TransitionContext context)
    {
        if (panelReference == null || !panelReference.RuntimeKeyIsValid())
        {
            Debug.LogError("HideUIPanelTask: Panel Reference is not valid.");
            yield break;
        }

        ServiceLocator.Get<IUIManager>()?.HidePanel(panelReference);
        yield break;
    }
}
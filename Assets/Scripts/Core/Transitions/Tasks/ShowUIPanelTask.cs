using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// A transition task that shows a specific UI panel via the UIManager without hiding others.
/// </summary>
[CreateAssetMenu(fileName = "NewShowUIPanelTask", menuName = "Transitions/Tasks/Show UI Panel")]
public class ShowUIPanelTask : TransitionTask
{
    [SerializeField]
    private AssetReferenceGameObject panelReference;

    public override IEnumerator Execute(TransitionContext context)
    {
        if (panelReference == null || !panelReference.RuntimeKeyIsValid())
        {
            GameLog.LogError("ShowUIPanelTask: Panel Reference is not valid.");
            yield break;
        }

        // The RuntimeKey is the addressable address string.
        string panelAddress = panelReference.RuntimeKey.ToString();

        var uiManager = context.GetFromContext<IUIManager>("UIManager");
        if (uiManager != null)
        {
            var task = uiManager.ShowPanel(panelAddress);
            if (task != null)
            {
                yield return new WaitUntil(() => task.IsCompleted);
            }
        }
        else
        {
            GameLog.LogError("ShowUIPanelTask: IUIManager not found in TransitionContext.");
        }
    }
}
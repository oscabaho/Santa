using Cysharp.Threading.Tasks;
using Santa.Core;
using Santa.Core.Transitions;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Santa.UI
{

/// <summary>
/// A transition task that shows a specific UI panel via the UIManager without hiding others.
/// </summary>
[CreateAssetMenu(fileName = "NewShowUIPanelTask", menuName = "Transitions/Tasks/Show UI Panel")]
public class ShowUIPanelTask : TransitionTask
{
    [SerializeField]
    private AssetReferenceGameObject panelReference;

    public override async UniTask Execute(TransitionContext context)
    {
        if (panelReference == null || !panelReference.RuntimeKeyIsValid())
        {
            GameLog.LogError("ShowUIPanelTask: Panel Reference is not valid.");
            return;
        }

        // The RuntimeKey is the addressable address string.
        string panelAddress = panelReference.RuntimeKey.ToString();

        var uiManager = context.GetFromContext<IUIManager>("UIManager");
        if (uiManager != null)
        {
            await uiManager.ShowPanel(panelAddress);
        }
        else
        {
            GameLog.LogError("ShowUIPanelTask: IUIManager not found in TransitionContext.");
        }
    }
}
}
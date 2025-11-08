using System.Collections;
using UnityEngine;

/// <summary>
/// Shows or hides the loading text on the ScreenFade overlay.
/// Uses the ScreenFade instance provided in the TransitionContext under key "ScreenFade".
/// </summary>
[CreateAssetMenu(fileName = "NewSetLoadingTextTask", menuName = "Transitions/Tasks/Set Loading Text")]
public class SetLoadingTextTask : TransitionTask
{
    [SerializeField] private bool show = true;
    [SerializeField] [TextArea] private string message = "Cargando...";

    public override IEnumerator Execute(TransitionContext context)
    {
        var screenFade = context.GetFromContext<ScreenFade>("ScreenFade");
        if (screenFade == null)
        {
            GameLog.LogError("SetLoadingTextTask: ScreenFade not found in context.");
            yield break;
        }

        if (show)
        {
            screenFade.ShowLoading(string.IsNullOrWhiteSpace(message) ? null : message);
        }
        else
        {
            screenFade.HideLoading();
        }

        yield break;
    }
}

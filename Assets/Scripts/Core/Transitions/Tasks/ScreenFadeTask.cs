using System.Collections;
using UnityEngine;

public enum FadeDirection { In, Out }

/// <summary>
/// A transition task that fades the screen to or from a solid color.
/// </summary>
[CreateAssetMenu(fileName = "NewScreenFadeTask", menuName = "Transitions/Tasks/Screen Fade")]
public class ScreenFadeTask : TransitionTask
{
    [SerializeField]
    private FadeDirection direction;

    [SerializeField]
    private float duration = 0.25f;

    [SerializeField]
    private Color fadeColor = Color.black;

    public override IEnumerator Execute(TransitionContext context)
    {
        var screenFade = context.GetFromContext<ScreenFade>("ScreenFade");
        if (screenFade == null)
        {
            GameLog.LogError("ScreenFadeTask: ScreenFade not found in context.");
            yield break;
        }

        // Fade Out is to a color (transparent to solid)
        // Fade In is from a color (solid to transparent)
        float fromAlpha = (direction == FadeDirection.Out) ? 0f : 1f;
        float toAlpha = (direction == FadeDirection.Out) ? 1f : 0f;
        yield return screenFade.Fade(fromAlpha, toAlpha, duration, fadeColor);
    }
}

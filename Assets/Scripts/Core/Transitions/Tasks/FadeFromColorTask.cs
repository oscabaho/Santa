using System.Collections;
using UnityEngine;

/// <summary>
/// A transition task that fades the screen from a solid color to transparent.
/// </summary>
[CreateAssetMenu(fileName = "NewFadeFromColorTask", menuName = "Transitions/Tasks/Fade From Color")]
public class FadeFromColorTask : TransitionTask
{
    [SerializeField]
    private float duration = 0.25f;

    [SerializeField]
    private Color fadeColor = Color.black;

    public override IEnumerator Execute(TransitionContext context)
    {
        // Use the static ScreenFade helper to fade from 1 (solid) to 0 (transparent)
        yield return ScreenFade.Fade(1f, 0f, duration, fadeColor);
    }
}

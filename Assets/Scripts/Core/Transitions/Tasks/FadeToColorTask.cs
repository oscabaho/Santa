using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A transition task that fades the screen to a solid color.
/// </summary>
[CreateAssetMenu(fileName = "NewFadeToColorTask", menuName = "Transitions/Tasks/Fade To Color")]
public class FadeToColorTask : TransitionTask
{
    [SerializeField]
    private float duration = 0.25f;

    [SerializeField]
    private Color fadeColor = Color.black;

    public override IEnumerator Execute(TransitionContext context)
    {
        // Use the static ScreenFade helper to fade from 0 (transparent) to 1 (solid)
        yield return ScreenFade.Fade(0f, 1f, duration, fadeColor);
    }
}

/// <summary>
/// Helper static class to manage the fade overlay canvas.
/// </summary>
public static class ScreenFade
{
    private static GameObject _overlayRoot;
    private static Image _overlayImage;

    public static IEnumerator Fade(float fromAlpha, float toAlpha, float duration, Color color)
    {
        EnsureOverlayExists(color);
        
        // Set initial color, but with the starting alpha
        color.a = fromAlpha;
        _overlayImage.color = color;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            // Use unscaled time for transitions that should work even if Time.timeScale is 0
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float a = Mathf.Lerp(fromAlpha, toAlpha, t);
            color.a = a;
            _overlayImage.color = color;
            yield return null;
        }
        color.a = toAlpha;
        _overlayImage.color = color;
    }

    private static void EnsureOverlayExists(Color initialColor)
    {
        if (_overlayRoot != null) return;

        _overlayRoot = GameObject.Find("__TransitionOverlay");
        if (_overlayRoot == null)
        {
            _overlayRoot = new GameObject("__TransitionOverlay");
            Object.DontDestroyOnLoad(_overlayRoot);
            var canvas = _overlayRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10000; // High value to be on top of everything

            var scaler = _overlayRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            _overlayRoot.AddComponent<GraphicRaycaster>();

            var imageGO = new GameObject("OverlayImage");
            imageGO.transform.SetParent(_overlayRoot.transform, false);
            _overlayImage = imageGO.AddComponent<Image>();
            _overlayImage.rectTransform.anchorMin = Vector2.zero;
            _overlayImage.rectTransform.anchorMax = Vector2.one;
            _overlayImage.rectTransform.anchoredPosition = Vector2.zero;
            _overlayImage.rectTransform.sizeDelta = Vector2.zero;
            
            initialColor.a = 0;
            _overlayImage.color = initialColor;
        }
        else
        {
            _overlayImage = _overlayRoot.GetComponentInChildren<Image>();
        }
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full-screen fade using a runtime overlay Canvas + Image. This is reliable for fading non-UI and UI elements.
/// The overlay GameObject is created under a temporary parent and reused across calls.
/// </summary>
[CreateAssetMenu(fileName = "NewFullScreenFadeTask", menuName = "Transitions/Tasks/Full Screen Fade")]
public class FullScreenFadeTask : TransitionTask
{
    [SerializeField]
    private float duration = 0.5f;

    [SerializeField]
    private Color fadeColor = Color.black;

    // Singleton overlay used by all instances at runtime
    private static GameObject _overlayRoot;
    private static Image _overlayImage;

    public override IEnumerator Execute(TransitionContext context)
    {
        EnsureOverlayExists();

        // Fade from transparent to color (in) and then back to transparent (out)
        yield return Fade(0f, 1f, duration / 2f);
        yield return Fade(1f, 0f, duration / 2f);
    }

    private IEnumerator Fade(float from, float to, float dur)
    {
        if (_overlayImage == null) yield break;

        float elapsed = 0f;
        Color c = fadeColor;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dur);
            float a = Mathf.Lerp(from, to, t);
            c.a = a;
            _overlayImage.color = c;
            yield return null;
        }
        c.a = to;
        _overlayImage.color = c;
        yield break;
    }

    private void EnsureOverlayExists()
    {
        if (_overlayRoot != null && _overlayImage != null) return;

        // Try to find existing overlay in scene
        _overlayRoot = GameObject.Find("__TransitionOverlay");
        if (_overlayRoot == null)
        {
            _overlayRoot = new GameObject("__TransitionOverlay");
            var canvas = _overlayRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10000;
            _overlayRoot.AddComponent<CanvasScaler>();
            _overlayRoot.AddComponent<GraphicRaycaster>();

            var imageGO = new GameObject("OverlayImage");
            imageGO.transform.SetParent(_overlayRoot.transform, false);
            _overlayImage = imageGO.AddComponent<Image>();
            _overlayImage.rectTransform.anchorMin = Vector2.zero;
            _overlayImage.rectTransform.anchorMax = Vector2.one;
            _overlayImage.rectTransform.anchoredPosition = Vector2.zero;
            _overlayImage.rectTransform.sizeDelta = Vector2.zero;
            _overlayImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        }
        else
        {
            _overlayImage = _overlayRoot.GetComponentInChildren<Image>();
            if (_overlayImage == null)
            {
                var imageGO = new GameObject("OverlayImage");
                imageGO.transform.SetParent(_overlayRoot.transform, false);
                _overlayImage = imageGO.AddComponent<Image>();
                _overlayImage.rectTransform.anchorMin = Vector2.zero;
                _overlayImage.rectTransform.anchorMax = Vector2.one;
                _overlayImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            }
        }
    }
}

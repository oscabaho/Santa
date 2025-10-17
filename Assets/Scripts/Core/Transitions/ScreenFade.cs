using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper MonoBehaviour singleton to manage the fade overlay canvas.
/// </summary>
public class ScreenFade : MonoBehaviour
{
    private static ScreenFade _instance;
    private Image _overlayImage;

    public static ScreenFade Instance
    {
        get
        {
            if (_instance == null)
            {
                // Check if an instance exists in the scene
                _instance = FindFirstObjectByType<ScreenFade>();

                // If not, create a new one
                if (_instance == null)
                {
                    var go = new GameObject("__TransitionOverlay");
                    _instance = go.AddComponent<ScreenFade>();
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        // Singleton pattern
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Set up the canvas and image
        EnsureOverlayExists(Color.clear);
    }

    public IEnumerator Fade(float fromAlpha, float toAlpha, float duration, Color color)
    {
        // Ensure the overlay is set up
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

    private void EnsureOverlayExists(Color initialColor)
    {
        if (_overlayImage != null) return;

        // This GameObject should already have been set up in Awake
        // but we add components just in case it's being created on the fly.
        gameObject.name = "__TransitionOverlay";
        var canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10000; // High value to be on top of everything
        }

        var scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        }

        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        _overlayImage = GetComponentInChildren<Image>();
        if (_overlayImage == null)
        {
            var imageGO = new GameObject("OverlayImage");
            imageGO.transform.SetParent(transform, false);
            _overlayImage = imageGO.AddComponent<Image>();
            _overlayImage.rectTransform.anchorMin = Vector2.zero;
            _overlayImage.rectTransform.anchorMax = Vector2.one;
            _overlayImage.rectTransform.anchoredPosition = Vector2.zero;
            _overlayImage.rectTransform.sizeDelta = Vector2.zero;

            initialColor.a = 0;
            _overlayImage.color = initialColor;
        }
    }
}

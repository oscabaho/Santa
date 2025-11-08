using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Helper MonoBehaviour to manage the fade overlay canvas.
/// </summary>
public class ScreenFade : MonoBehaviour
{
    [Header("Behavior")]
    [Tooltip("If true, blocks all UI raycasts while the overlay is visible (alpha > 0).")]
    [SerializeField] private bool blockRaycastsWhileVisible = true;

    [Tooltip("If true, disables the Canvas component when the overlay becomes fully transparent (alpha == 0) to avoid any accidental interference with UI.")]
    [SerializeField] private bool disableCanvasWhenTransparent = true;

    private Image _overlayImage;
    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    private GraphicRaycaster _raycaster;
    private TextMeshProUGUI _loadingText;

    [Header("Loading Text")]
    [SerializeField] private string defaultLoadingMessage = "Cargando...";
    [SerializeField] private int loadingFontSize = 32;
    [SerializeField] private Color loadingTextColor = Color.white;
    [SerializeField] private Vector2 loadingTextAnchorMin = new Vector2(0.1f, 0.1f);
    [SerializeField] private Vector2 loadingTextAnchorMax = new Vector2(0.9f, 0.25f);
    [SerializeField] private TextAlignmentOptions loadingTextAlignment = TextAlignmentOptions.Center;

    [Header("Scene Start Fade")]
    [SerializeField] private bool fadeOnSceneStart = true;
    [SerializeField] private float sceneStartFadeDuration = 2f;
    [SerializeField] private Color sceneStartFadeColor = Color.black;
    [SerializeField] private bool showLoadingOnSceneStart = true;
    [SerializeField] private string sceneStartLoadingMessage = "Cargando...";

    private void Awake()
    {
        // Persist through scene loads
        DontDestroyOnLoad(gameObject);

        // Prepare overlay with appropriate initial alpha based on scene start fade setting
        Color initialColor = fadeOnSceneStart ? sceneStartFadeColor : Color.clear;
        if (fadeOnSceneStart)
        {
            initialColor.a = 1f; // Start opaque if we're fading on scene start
        }
        
        EnsureOverlayExists(initialColor);

        // Ensure canvas is enabled from the start if we need to show initial fade
        if (fadeOnSceneStart && _canvas != null)
        {
            _canvas.enabled = true;
        }

        // Optional initial fade to hide early scene construction
        if (fadeOnSceneStart)
        {
            // Enable blocking while visible
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = blockRaycastsWhileVisible;
            }
            
            if (showLoadingOnSceneStart)
            {
                ShowLoading(sceneStartLoadingMessage);
            }
            
            StartCoroutine(Fade(1f, 0f, sceneStartFadeDuration, sceneStartFadeColor));
        }
    }

    public IEnumerator Fade(float fromAlpha, float toAlpha, float duration, Color color)
    {
        // Ensure the overlay is set up
        EnsureOverlayExists(color);

        // Make sure the canvas is enabled during the fade
        if (_canvas != null) _canvas.enabled = true;
        if (_overlayImage != null && !_overlayImage.enabled) _overlayImage.enabled = true;

        // Set initial color, but with the starting alpha
        color.a = fromAlpha;
        _overlayImage.color = color;

        // Initialize loading text alpha to match
        if (_loadingText != null && _loadingText.enabled)
        {
            var tc = _loadingText.color; tc.a = fromAlpha; _loadingText.color = tc;
        }

        // While visible, optionally block raycasts so underlying UI can't be clicked
        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = false; // overlay itself isn't interactive
            _canvasGroup.blocksRaycasts = blockRaycastsWhileVisible && fromAlpha > 0f;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            // Use unscaled time for transitions that should work even if Time.timeScale is 0
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float a = Mathf.Lerp(fromAlpha, toAlpha, t);
            color.a = a;
            _overlayImage.color = color;

            // Update blocking while there's any visible alpha
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = blockRaycastsWhileVisible && a > 0f;
            }

            // Match loading text alpha
            if (_loadingText != null && _loadingText.enabled)
            {
                var tc = _loadingText.color; tc.a = a; _loadingText.color = tc;
            }
            yield return null;
        }
        color.a = toAlpha;
        _overlayImage.color = color;

        // At the end of the fade, ensure blocking reflects final visibility
        if (_canvasGroup != null)
        {
            _canvasGroup.blocksRaycasts = blockRaycastsWhileVisible && toAlpha > 0f;
        }

        // Optionally disable the canvas entirely when fully transparent to guarantee no obstruction
        if (disableCanvasWhenTransparent && toAlpha <= 0f && _canvas != null)
        {
            _canvas.enabled = false;
        }

        // Hide loading text when fully transparent
        if (toAlpha <= 0f && _loadingText != null)
        {
            _loadingText.enabled = false;
        }
    }

    private void EnsureOverlayExists(Color initialColor)
    {
        if (_overlayImage != null)
        {
            // Ensure image doesn't capture clicks by itself; we control blocking via CanvasGroup
            _overlayImage.raycastTarget = false;
            return;
        }

        // This GameObject should already have been set up in Awake
        // but we add components just in case it's being created on the fly.
        gameObject.name = "__TransitionOverlay";
        _canvas = GetComponent<Canvas>();
        if (_canvas == null)
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 10000; // High value to be on top of everything
        }

        var scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f; // Balance between width and height matching
        }

        // CanvasGroup controls whether this overlay blocks raycasts
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false; // default off; toggled during fades

        // Keep a GraphicRaycaster so this canvas can participate in raycasting when blocking is enabled
        _raycaster = GetComponent<GraphicRaycaster>();
        if (_raycaster == null)
        {
            _raycaster = gameObject.AddComponent<GraphicRaycaster>();
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

            // Respect the requested initial color (including alpha)
            _overlayImage.color = initialColor;
        }

        // The image itself should never be the thing that captures clicks
        _overlayImage.raycastTarget = false;

        // Create/loading text element (disabled by default)
        if (_loadingText == null)
        {
            var textGO = new GameObject("LoadingText");
            textGO.transform.SetParent(transform, false);
            _loadingText = textGO.AddComponent<TextMeshProUGUI>();
            _loadingText.raycastTarget = false;
            _loadingText.alignment = loadingTextAlignment;
            _loadingText.fontSize = loadingFontSize;
            _loadingText.color = new Color(loadingTextColor.r, loadingTextColor.g, loadingTextColor.b, 0f);
            _loadingText.text = defaultLoadingMessage;
            if (TMP_Settings.defaultFontAsset != null)
            {
                _loadingText.font = TMP_Settings.defaultFontAsset;
            }
            var rt = _loadingText.rectTransform;
            rt.anchorMin = loadingTextAnchorMin;
            rt.anchorMax = loadingTextAnchorMax;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
            _loadingText.enabled = false; // only shown via ShowLoading()
        }
    }

    /// <summary>
    /// Shows the loading text and (optionally) sets its message. The text will fade together with the overlay.
    /// </summary>
    public void ShowLoading(string message = null)
    {
        EnsureOverlayExists(Color.black);
        if (_loadingText != null)
        {
            if (!string.IsNullOrEmpty(message))
                _loadingText.text = message;
            _loadingText.enabled = true;
            var c = _loadingText.color; c.a = _overlayImage != null ? _overlayImage.color.a : 0f; _loadingText.color = c;
        }
        if (_canvas != null) _canvas.enabled = true;
    }

    /// <summary>
    /// Hides the loading text immediately (independent of the overlay alpha).
    /// </summary>
    public void HideLoading()
    {
        if (_loadingText != null)
        {
            _loadingText.enabled = false;
        }
    }
}

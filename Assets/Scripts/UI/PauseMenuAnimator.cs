using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Santa.Presentation.Menus
{
    /// <summary>
    /// Handles fade in/out animations for the pause menu using a CanvasGroup.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class PauseMenuAnimator : MonoBehaviour
    {
        [SerializeField] private float fadeDuration = 0.3f;

        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                GameLog.LogError($"PauseMenuAnimator on {gameObject.name} requires a CanvasGroup component.", this);
            }
        }

        /// <summary>
        /// Fades in the pause menu.
        /// </summary>
        public async UniTask FadeIn()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null) return;
            }

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = true;

            float elapsed = 0f;
            float startAlpha = _canvasGroup.alpha;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / fadeDuration);
                await UniTask.Yield();
            }

            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
        }

        /// <summary>
        /// Fades out the pause menu.
        /// </summary>
        public async UniTask FadeOut()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null) return;
            }

            _canvasGroup.interactable = false;

            float elapsed = 0f;
            float startAlpha = _canvasGroup.alpha;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
                await UniTask.Yield();
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
        }
    }
}

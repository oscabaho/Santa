using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Santa.UI
{
    /// <summary>
    /// Simple CanvasGroup-based fade animator for PauseMenu.
    /// Attach to the PauseMenu root and assign CanvasGroup.
    /// </summary>
    public class PauseMenuAnimator : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeDuration = 0.2f;

        private void Reset()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        public async UniTask FadeIn()
        {
            if (canvasGroup == null)
            {
                return;
            }
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            float t = 0f;
            float start = canvasGroup.alpha;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(start, 1f, t / fadeDuration);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
            canvasGroup.alpha = 1f;
        }

        public async UniTask FadeOut()
        {
            if (canvasGroup == null)
            {
                return;
            }
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            float t = 0f;
            float start = canvasGroup.alpha;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(start, 0f, t / fadeDuration);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
            canvasGroup.alpha = 0f;
        }
    }
}

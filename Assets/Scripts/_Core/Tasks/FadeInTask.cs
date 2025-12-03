using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Simple fade-in transition task. Looks for a CanvasGroup on the target and fades alpha from 0 to 1 over duration.
/// If no CanvasGroup exists, one is added to the target GameObject.
/// </summary>
[CreateAssetMenu(fileName = "NewFadeInTask", menuName = "Transitions/Tasks/Fade In")]
public class FadeInTask : TransitionTask
{
    [SerializeField]
    private TargetId targetId = TargetId.CombatSceneParent;

    [SerializeField]
    private float duration = 0.5f;

    public override async UniTask Execute(TransitionContext context)
    {
        GameObject target = context.GetTarget(targetId);
        if (target == null)
        {
            GameLog.LogWarning($"FadeInTask: Target '{targetId}' not found in context.");
            return;
        }

        // Try to find an existing CanvasGroup. If none, add one.
        var canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = target.AddComponent<CanvasGroup>();
        }

        // Ensure target is active so that CanvasGroup updates are visible.
        if (!target.activeSelf)
            target.SetActive(true);

        canvasGroup.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        canvasGroup.alpha = 1f;

        // If we added the CanvasGroup only for this fade, optionally leave it — removal may cause visual flicker if other tasks expect it.
    }
}

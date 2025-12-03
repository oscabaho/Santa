using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// A transition task that simply waits for a specified duration.
/// </summary>
[CreateAssetMenu(fileName = "NewWaitTask", menuName = "Transitions/Tasks/Wait")]
public class WaitTask : TransitionTask
{
    [SerializeField]
    private float duration = 1f;

    public override async UniTask Execute(TransitionContext context)
    {
        await UniTask.Delay((int)(duration * 1000));
    }
}

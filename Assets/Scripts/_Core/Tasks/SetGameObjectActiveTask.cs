using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// A transition task that activates or deactivates a target GameObject.
/// </summary>
[CreateAssetMenu(fileName = "NewSetGameObjectActiveTask", menuName = "Transitions/Tasks/Set GameObject Active")]
public class SetGameObjectActiveTask : TransitionTask
{
    [SerializeField]
    private TargetId targetId;
    [SerializeField]
    private bool active;

    public override UniTask Execute(TransitionContext context)
    {
        GameObject target = context.GetTarget(targetId);
        if (target != null)
        {
            target.SetActive(active);
        }
        else
        {
            GameLog.LogWarning($"SetGameObjectActiveTask: Target '{targetId}' not found in context.");
        }

        return UniTask.CompletedTask;
    }
}

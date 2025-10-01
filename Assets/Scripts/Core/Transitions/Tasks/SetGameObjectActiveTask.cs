using System.Collections;
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

    public override IEnumerator Execute(TransitionContext context)
    {
        GameObject target = context.GetTarget(targetId);
        if (target != null)
        {
            target.SetActive(active);
        }
        else
        {
            Debug.LogWarning($"SetGameObjectActiveTask: Target '{targetId}' not found in context.");
        }
        yield break;
    }
}

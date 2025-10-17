using System.Collections;
using UnityEngine;

/// <summary>
/// A transition task that simply waits for a specified duration.
/// </summary>
[CreateAssetMenu(fileName = "NewWaitTask", menuName = "Transitions/Tasks/Wait")]
public class WaitTask : TransitionTask
{
    [SerializeField]
    private float duration = 1f;

    public override IEnumerator Execute(TransitionContext context)
    {
        yield return new WaitForSeconds(duration);
    }
}

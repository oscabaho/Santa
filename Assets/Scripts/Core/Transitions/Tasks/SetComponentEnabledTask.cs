using System.Collections;
using UnityEngine;

/// <summary>
/// A transition task that enables or disables a target Component on a GameObject.
/// </summary>
[CreateAssetMenu(fileName = "NewSetComponentEnabledTask", menuName = "Transitions/Tasks/Set Component Enabled")]
public class SetComponentEnabledTask : TransitionTask
{
    [SerializeField]
    private TargetId targetId;

    [Tooltip("The full name of the component type to enable/disable, e.g., 'UnityEngine.AI.NavMeshAgent'")]
    [SerializeField]
    private string componentType;

    [SerializeField]
    private bool enabled;

    public override IEnumerator Execute(TransitionContext context)
    {
        GameObject target = context.GetTarget(targetId);
        if (target == null)
        {
            GameLog.LogWarning($"SetComponentEnabledTask: Target '{targetId}' not found in context.");
            yield break;
        }

        // Note: This is a simplified way to get a component by its string name.
        // For a more robust solution, you might use reflection or a custom component registry.
        var component = target.GetComponent(componentType) as Behaviour; 
        if (component != null)
        {
            component.enabled = enabled;
        }
        else
        {
            GameLog.LogWarning($"SetComponentEnabledTask: Component '{componentType}' not found on target '{target.name}'.");
        }
        yield break;
    }
}

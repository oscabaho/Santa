using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Collider))]
public class InteractionDetector : MonoBehaviour
{
    private readonly List<IInteractable> interactablesInRange = new List<IInteractable>();

    private void Awake()
    {
        // Ensure the collider is a trigger
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IInteractable>(out var interactable) && !interactablesInRange.Contains(interactable))
        {
            interactablesInRange.Add(interactable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IInteractable>(out var interactable))
        {
            interactablesInRange.Remove(interactable);
        }
    }

    public IInteractable GetClosestInteractable(Transform relativeTo)
    {
        // Clean up any null references that might have occurred (e.g., destroyed objects)
        interactablesInRange.RemoveAll(item => item == null || (item as MonoBehaviour) == null);

        return interactablesInRange
            .OrderBy(interactable => Vector3.Distance(relativeTo.position, (interactable as MonoBehaviour).transform.position))
            .FirstOrDefault();
    }
}

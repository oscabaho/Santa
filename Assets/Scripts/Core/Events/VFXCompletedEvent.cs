using UnityEngine;

/// <summary>
/// Evento publicado cuando un efecto visual gestionado por VFXManager ha terminado.
/// </summary>
public class VFXCompletedEvent
{
    public GameObject TargetObject { get; }

    public VFXCompletedEvent(GameObject targetObject)
    {
        TargetObject = targetObject;
    }
}

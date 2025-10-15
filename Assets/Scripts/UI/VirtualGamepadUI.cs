using UnityEngine;

public class VirtualGamepadUI : MonoBehaviour
{
    [Header("Child UI Elements")]
    [Tooltip("Drag the Action Button GameObject that is a child of this prefab here.")]
    [SerializeField] private GameObject actionButton;

    void Awake()
    {
        if (actionButton == null)
        {
            GameLog.LogError("The 'actionButton' is not assigned in the VirtualGamepadUI script.", this);
            return;
        }

        var gameplayUIService = ServiceLocator.Get<IGameplayUIService>();
        if (gameplayUIService != null)
        {
            gameplayUIService.RegisterActionButton(actionButton);
        }
        else
        {
            GameLog.LogError("VirtualGamepadUI: Could not find the IGameplayUIService. Make sure the GameplayUIManager is active in the scene.", this);
        }
    }
}

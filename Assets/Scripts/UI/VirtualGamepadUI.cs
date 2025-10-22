using UnityEngine;
using VContainer;

public class VirtualGamepadUI : MonoBehaviour
{
    [Header("Child UI Elements")]
    [Tooltip("Drag the Action Button GameObject that is a child of this prefab here.")]
    [SerializeField] private GameObject actionButton;

    private IGameplayUIService _gameplayUIService;

    [Inject]
    public void Construct(IGameplayUIService gameplayUIService)
    {
        _gameplayUIService = gameplayUIService;
    }

    void Awake()
    {
        if (actionButton == null)
        {
            GameLog.LogError("The 'actionButton' is not assigned in the VirtualGamepadUI script.", this);
            return;
        }

        if (_gameplayUIService != null)
        {
            _gameplayUIService.RegisterActionButton(actionButton);
        }
        else
        {
            GameLog.LogError("VirtualGamepadUI: IGameplayUIService was not injected. Make sure it's registered in the LifetimeScope.", this);
        }
    }
}

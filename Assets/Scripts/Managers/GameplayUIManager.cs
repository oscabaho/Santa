using UnityEngine;

public class GameplayUIManager : MonoBehaviour, IGameplayUIService
{
    [Header("UI Elements")]
    [Tooltip("Assign the Action Button GameObject from your UI here.")]
    [SerializeField] private GameObject actionButtonGameObject;

    private void Awake()
    {
        ServiceLocator.Register<IGameplayUIService>(this);
    }

    private void Start()
    {
        if (actionButtonGameObject != null)
        {
            actionButtonGameObject.SetActive(false);
        }
        else
        {
            GameLog.LogError("ActionButton GameObject is not assigned in the GameplayUIManager!");
        }
    }

    private void OnDestroy()
    {
        // Ensure we only unregister if this instance is the one registered
        var registeredService = ServiceLocator.Get<IGameplayUIService>();
        if ((Object)registeredService == this)
        {
            ServiceLocator.Unregister<IGameplayUIService>();
        }
    }

    public void ShowActionButton(bool show)
    {
        if (actionButtonGameObject != null)
        {
            actionButtonGameObject.SetActive(show);
        }
    }
}

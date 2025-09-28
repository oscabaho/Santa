using UnityEngine;

public class UIManager : MonoBehaviour, IUIManager
{
    private static UIManager Instance { get; set; }

    [SerializeField] private GameObject explorationUI;
    [SerializeField] private GameObject combatUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        ServiceLocator.Register<IUIManager>(this);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            ServiceLocator.Unregister<IUIManager>();
            Instance = null;
        }
    }

    public void ShowExplorationUI()
    {
        explorationUI.SetActive(true);
        combatUI.SetActive(false);
    }

    public void ShowCombatUI()
    {
        explorationUI.SetActive(false);
        combatUI.SetActive(true);
    }
}

using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

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

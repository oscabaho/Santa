using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the UI screen for choosing an ability upgrade after winning a battle.
/// </summary>
public class UpgradeUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Button option1Button;
    [SerializeField] private TextMeshProUGUI option1NameText;
    [SerializeField] private TextMeshProUGUI option1DescriptionText;
    [SerializeField] private Button option2Button;
    [SerializeField] private TextMeshProUGUI option2NameText;
    [SerializeField] private TextMeshProUGUI option2DescriptionText;

    private AbilityUpgrade _upgrade1;
    private AbilityUpgrade _upgrade2;

    private void Start()
    {
        option1Button.onClick.AddListener(OnOption1Clicked);
        option2Button.onClick.AddListener(OnOption2Clicked);
        upgradePanel.SetActive(false); // Start with the panel hidden
    }

    /// <summary>
    /// Configures the UI with two upgrade options and displays it.
    /// </summary>
    public void ShowUpgrades(AbilityUpgrade upgrade1, AbilityUpgrade upgrade2)
    {
        _upgrade1 = upgrade1;
        _upgrade2 = upgrade2;

        option1NameText.text = _upgrade1.UpgradeName;
        option1DescriptionText.text = _upgrade1.UpgradeDescription;

        option2NameText.text = _upgrade2.UpgradeName;
        option2DescriptionText.text = _upgrade2.UpgradeDescription;

        upgradePanel.SetActive(true);
    }

    private void OnOption1Clicked()
    {
        ChooseUpgrade(_upgrade1);
    }

    private void OnOption2Clicked()
    {
        ChooseUpgrade(_upgrade2);
    }

    private void ChooseUpgrade(AbilityUpgrade chosenUpgrade)
    {
        // 1. Apply the stat upgrade
        var upgradeService = ServiceLocator.Get<IUpgradeService>();
        if (upgradeService != null) upgradeService.ApplyUpgrade(chosenUpgrade);
        upgradePanel.SetActive(false);

        // 2. Liberate the current level (change visuals)
        var levelService = ServiceLocator.Get<ILevelService>();
        if (levelService != null)
        {
            levelService.LiberateCurrentLevel();
        }

        // 3. End the combat state (this was already here)
        var combatTransition = ServiceLocator.Get<ICombatTransitionService>();
        if (combatTransition != null)
        {
            combatTransition.EndCombat();
        }

        // 4. Prepare the next level/area
        if (levelService != null)
        {
            levelService.AdvanceToNextLevel();
        }
    }
}
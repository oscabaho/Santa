using System.Threading;
using Cysharp.Threading.Tasks;
using Santa.Core;
using Santa.UI; // Needed for PauseMenuAnimator
using Santa.Domain.Combat;
using Santa.Infrastructure.Combat;
using AbilityUpgrade = Santa.Domain.Combat.AbilityUpgrade;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

using Santa.Presentation.UI; // Needed for UIPanel namespace

namespace Santa.Presentation.Upgrades
{

/// <summary>
/// Manages the UI screen for choosing an ability upgrade after winning a battle.
/// Refactored to work as a prefab with modular card components.
/// </summary>
public class UpgradeUI : UIPanel, IUpgradeUI
{
    [Header("Panel References")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Card Components")]
    [SerializeField] private UpgradeCardUI option1Card;
    [SerializeField] private UpgradeCardUI option2Card;

    [Header("Optional Elements")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button closeButton; // Optional close without selection

    // fadeInDuration removed as it is handled by PauseMenuAnimator

    private IUpgradeService _upgradeService;
    private ILevelService _levelService;
    private ICombatTransitionService _combatTransitionService;
    private TurnBasedCombatManager _combatManager;

    // Track active fade cancellation to prevent overlapping animations
    private CancellationTokenSource _fadeCTS;

    protected override void Awake()
    {
        base.Awake(); // Setup UIPanel (finds CanvasGroup)

        // 1. Resolve Dependencies Manually
        if (_upgradeService == null)
            _upgradeService = FindFirstObjectByType<UpgradeManager>(); 
        
        if (_levelService == null)
            _levelService = FindFirstObjectByType<Santa.Infrastructure.Level.LevelManager>();

        if (_combatTransitionService == null)
            _combatTransitionService = FindFirstObjectByType<CombatTransitionManager>();

        if (_combatManager == null)
            _combatManager = FindFirstObjectByType<TurnBasedCombatManager>();

        // 2. Enforce Sorting Order (Overlay Priority)
        var canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        canvas.overrideSorting = true;
        canvas.sortingOrder = 6000; // Force above CombatUI (typ. 5000)

        // Ensure Animator for standard fade behavior
        if (GetComponent<Santa.UI.PauseMenuAnimator>() == null)
        {
             gameObject.AddComponent<Santa.UI.PauseMenuAnimator>();
        }

        // 3. Subscribe to card events
        if (option1Card != null)
            option1Card.OnUpgradeSelected += OnUpgradeChosen;

        if (option2Card != null)
            option2Card.OnUpgradeSelected += OnUpgradeChosen;

        // Optional close button
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseButtonClicked);

        // Ensure upgradePanel is active so Show()/Hide() work if they toggle this GO, 
        // but typically UIPanel toggle CanvasGroup alpha. 
        // If 'upgradePanel' is a separate child object, we might want to ensure it's on.
        if (upgradePanel != null) upgradePanel.SetActive(true);

        // Start hidden via base UIPanel method
        HideImmediate();
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        if (option1Card != null)
            option1Card.OnUpgradeSelected -= OnUpgradeChosen;

        if (option2Card != null)
            option2Card.OnUpgradeSelected -= OnUpgradeChosen;

        if (closeButton != null)
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
    }

    /// <summary>
    /// Configures the UI with two upgrade options and displays them.
    /// </summary>
    public void ShowUpgrades(AbilityUpgrade upgrade1, AbilityUpgrade upgrade2)
    {
        if (upgrade1 == null || upgrade2 == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning("Cannot show upgrades: one or both upgrades are null.");
#endif
            return;
        }

        // Configure cards
        option1Card?.Setup(upgrade1);
        option2Card?.Setup(upgrade2);

        // Set title via centralized UI strings
        if (titleText != null)
        {
            titleText.text = Santa.Core.Config.UIStrings.UpgradeTitle;
        }

        // Mostrar el panel - Calls usage base.Show() which triggers PauseMenuAnimator
        Show();
    }

    /// <summary>
    /// Callback when an upgrade is selected from any card.
    /// </summary>
    private void OnUpgradeChosen(AbilityUpgrade chosenUpgrade)
    {
        if (chosenUpgrade == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning("Chosen upgrade is null.");
#endif
            return;
        }

        // Disable both cards to avoid double-click
        option1Card?.SetInteractable(false);
        option2Card?.SetInteractable(false);

        // 1. Apply the stat upgrade
        _upgradeService?.ApplyUpgrade(chosenUpgrade);

        // 2. Hide the UI
        Hide();

        // 3. Liberate the current level (change visuals)
        _levelService?.LiberateCurrentLevel();

        // 4. End the combat state
        _combatTransitionService?.EndCombat(true);

        // 5. Deactivate TurnBasedCombatManager now that upgrade is selected
        // This was previously happening too early in TurnBasedCombatManager.EndCombat()
        var combatManager = _combatManager;
        if (combatManager == null)
        {
            combatManager = FindFirstObjectByType<TurnBasedCombatManager>();
        }
        if (combatManager != null)
        {
            combatManager.gameObject.SetActive(false);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log("UpgradeUI: Deactivated TurnBasedCombatManager after upgrade selection.");
#endif
        }

        // 6. Prepare the next level/area
        _levelService?.AdvanceToNextLevel();
    }

    /// <summary>
    /// Callback to close without choosing (optional, useful for testing).
    /// </summary>
    private void OnCloseButtonClicked()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.LogWarning("Upgrade selection closed without choosing.");
#endif
        Hide();
        _combatTransitionService?.EndCombat(true);

        // Deactivate combat manager when closing without selection
        var combatManager = _combatManager;
        if (combatManager == null)
        {
            combatManager = FindFirstObjectByType<TurnBasedCombatManager>();
        }
        if (combatManager != null)
        {
            combatManager.gameObject.SetActive(false);
        }
    }
}
}

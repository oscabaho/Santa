using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// Manages the UI screen for choosing an ability upgrade after winning a battle.
/// Refactored to work as a prefab with modular card components.
/// </summary>
public class UpgradeUI : MonoBehaviour, IUpgradeUI
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

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;

    private IUpgradeService _upgradeService;
    private ILevelService _levelService;
    private ICombatTransitionService _combatTransitionService;
    private TurnBasedCombatManager _combatManager;

    // Track active fade coroutine to prevent StopAllCoroutines from canceling other coroutines
    private Coroutine _fadeCoroutine;

    [Inject]
    public void Construct(IUpgradeService upgradeService, ILevelService levelService, ICombatTransitionService combatTransitionService, TurnBasedCombatManager combatManager = null)
    {
        _upgradeService = upgradeService;
        _levelService = levelService;
        _combatTransitionService = combatTransitionService;
        _combatManager = combatManager;
    }

    private void Awake()
    {
        // Subscribe to card events
        if (option1Card != null)
            option1Card.OnUpgradeSelected += OnUpgradeChosen;

        if (option2Card != null)
            option2Card.OnUpgradeSelected += OnUpgradeChosen;

        // Optional close button
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseButtonClicked);

        // Ensure canvas group exists
        if (canvasGroup == null)
            canvasGroup = upgradePanel?.GetComponent<CanvasGroup>();

        // Start hidden
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

        // Mostrar el panel
        Show();
    }

    /// <summary>
    /// Shows the panel with a smooth fade-in.
    /// </summary>
    private void Show()
    {
        if (upgradePanel != null)
            upgradePanel.SetActive(true);

        // Fade in with CanvasGroup
        if (canvasGroup != null)
        {
            // Stop only the active fade coroutine, not all coroutines
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }
            _fadeCoroutine = StartCoroutine(FadeIn());
        }
    }

    /// <summary>
    /// Hides the panel immediately.
    /// </summary>
    private void HideImmediate()
    {
        if (upgradePanel != null)
            upgradePanel.SetActive(false);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        // Clear fade coroutine reference
        _fadeCoroutine = null;
    }

    /// <summary>
    /// Hides the panel with a smooth fade-out.
    /// </summary>
    private void Hide()
    {
        if (canvasGroup != null)
        {
            // Stop only the active fade coroutine, not all coroutines
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }
            _fadeCoroutine = StartCoroutine(FadeOut());
        }
        else
        {
            HideImmediate();
        }
    }

    private System.Collections.IEnumerator FadeIn()
    {
        float elapsed = 0f;
        canvasGroup.interactable = false; // Disable during animation

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        _fadeCoroutine = null; // Clear reference when complete
    }

    private System.Collections.IEnumerator FadeOut()
    {
        float elapsed = 0f;
        canvasGroup.interactable = false;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / fadeInDuration));
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        if (upgradePanel != null)
            upgradePanel.SetActive(false);

        _fadeCoroutine = null; // Clear reference when complete
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

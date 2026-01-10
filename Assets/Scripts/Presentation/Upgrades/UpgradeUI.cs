using System.Threading;
using Cysharp.Threading.Tasks;
using Santa.Core;
using Santa.Domain.Combat;
using Santa.Infrastructure.Combat;
using AbilityUpgrade = Santa.Domain.Combat.AbilityUpgrade;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Santa.Presentation.Upgrades
{

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

    // Track active fade cancellation to prevent overlapping animations
    private CancellationTokenSource _fadeCTS;

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
        if (canvasGroup == null && upgradePanel != null)
            canvasGroup = upgradePanel.GetComponent<CanvasGroup>();

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
            // Stop only the active fade
            _fadeCTS?.Cancel();
            _fadeCTS = new CancellationTokenSource();
            FadeIn(_fadeCTS.Token).Forget();
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

        // Clear fade CTS reference
        _fadeCTS?.Cancel();
        _fadeCTS = null;
    }

    /// <summary>
    /// Hides the panel with a smooth fade-out.
    /// </summary>
    private void Hide()
    {
        if (canvasGroup != null)
        {
            // Stop only the active fade
            _fadeCTS?.Cancel();
            _fadeCTS = new CancellationTokenSource();
            FadeOut(_fadeCTS.Token).Forget();
        }
        else
        {
            HideImmediate();
        }
    }

    private async UniTaskVoid FadeIn(CancellationToken token)
    {
        try
        {
            float elapsed = 0f;
            canvasGroup.interactable = false; // Disable during animation

            while (elapsed < fadeInDuration)
            {
                if (token.IsCancellationRequested) return;

                elapsed += Time.unscaledDeltaTime;
                if (canvasGroup == null) return;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
                await UniTask.Yield(PlayerLoopTiming.Update);
                if (canvasGroup == null) return;
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        catch (System.OperationCanceledException)
        {
            // Expected during scene transitions
        }
        catch (System.Exception ex)
        {
            GameLog.LogError($"UpgradeUI.FadeIn: Exception: {ex.Message}");
            GameLog.LogException(ex);
            // Ensure UI is in a valid state
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    private async UniTaskVoid FadeOut(CancellationToken token)
    {
        try
        {
            float elapsed = 0f;
            canvasGroup.interactable = false;

            while (elapsed < fadeInDuration)
            {
                if (token.IsCancellationRequested) return;

                elapsed += Time.unscaledDeltaTime;
                if (canvasGroup == null) return;
                canvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / fadeInDuration));
                await UniTask.Yield(PlayerLoopTiming.Update);
                if (canvasGroup == null) return;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;

            if (upgradePanel != null)
                upgradePanel.SetActive(false);
        }
        catch (System.OperationCanceledException)
        {
            // Expected during scene transitions
        }
        catch (System.Exception ex)
        {
            GameLog.LogError($"UpgradeUI.FadeOut: Exception: {ex.Message}");
            GameLog.LogException(ex);
            // Ensure UI is hidden
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            if (upgradePanel != null)
                upgradePanel.SetActive(false);
        }
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

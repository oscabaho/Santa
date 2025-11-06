using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;

/// <summary>
/// Manages the UI screen for choosing an ability upgrade after winning a battle.
/// Now refactored to work as a prefab with modular card components.
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
    [SerializeField] private Button closeButton; // Para cerrar sin elegir (opcional)

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;

    private IUpgradeService _upgradeService;
    private ILevelService _levelService;
    private ICombatTransitionService _combatTransitionService;

    [Inject]
    public void Construct(IUpgradeService upgradeService, ILevelService levelService, ICombatTransitionService combatTransitionService)
    {
        _upgradeService = upgradeService;
        _levelService = levelService;
        _combatTransitionService = combatTransitionService;
    }

    private void Awake()
    {
        // Suscribirse a los eventos de las tarjetas
        if (option1Card != null)
            option1Card.OnUpgradeSelected += OnUpgradeChosen;

        if (option2Card != null)
            option2Card.OnUpgradeSelected += OnUpgradeChosen;

        // Botón de cerrar opcional
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseButtonClicked);

        // Asegurarse de que el canvas group existe
        if (canvasGroup == null)
            canvasGroup = upgradePanel?.GetComponent<CanvasGroup>();

        // Iniciar oculto
        HideImmediate();
    }

    private void OnDestroy()
    {
        // Desuscribirse para evitar memory leaks
        if (option1Card != null)
            option1Card.OnUpgradeSelected -= OnUpgradeChosen;

        if (option2Card != null)
            option2Card.OnUpgradeSelected -= OnUpgradeChosen;

        if (closeButton != null)
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
    }

    /// <summary>
    /// Configures the UI with two upgrade options and displays it.
    /// </summary>
    public void ShowUpgrades(AbilityUpgrade upgrade1, AbilityUpgrade upgrade2)
    {
        if (upgrade1 == null || upgrade2 == null)
        {
            GameLog.LogWarning("Cannot show upgrades: one or both upgrades are null.");
            return;
        }

        // Configurar las tarjetas
        option1Card?.Setup(upgrade1);
        option2Card?.Setup(upgrade2);

        // Mostrar el panel
        Show();
    }

    /// <summary>
    /// Muestra el panel con un fade-in suave.
    /// </summary>
    private void Show()
    {
        if (upgradePanel != null)
            upgradePanel.SetActive(true);

        // Fade in con CanvasGroup
        if (canvasGroup != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeIn());
        }
    }

    /// <summary>
    /// Oculta el panel inmediatamente.
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
    }

    /// <summary>
    /// Oculta el panel con un fade-out suave.
    /// </summary>
    private void Hide()
    {
        if (canvasGroup != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeOut());
        }
        else
        {
            HideImmediate();
        }
    }

    private System.Collections.IEnumerator FadeIn()
    {
        float elapsed = 0f;
        canvasGroup.interactable = false; // Deshabilitar durante la animación

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
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
    }

    /// <summary>
    /// Callback cuando se selecciona un upgrade desde cualquier tarjeta.
    /// </summary>
    private void OnUpgradeChosen(AbilityUpgrade chosenUpgrade)
    {
        if (chosenUpgrade == null)
        {
            GameLog.LogWarning("Chosen upgrade is null.");
            return;
        }

        // Deshabilitar ambas tarjetas para evitar doble-click
        option1Card?.SetInteractable(false);
        option2Card?.SetInteractable(false);

        // 1. Apply the stat upgrade
        _upgradeService?.ApplyUpgrade(chosenUpgrade);

        // 2. Hide the UI
        Hide();

        // 3. Liberate the current level (change visuals)
        _levelService?.LiberateCurrentLevel();

        // 4. End the combat state
        _combatTransitionService?.EndCombat();

        // 5. Prepare the next level/area
        _levelService?.AdvanceToNextLevel();
    }

    /// <summary>
    /// Callback para cerrar sin elegir (opcional, puede usarse para testing).
    /// </summary>
    private void OnCloseButtonClicked()
    {
        GameLog.LogWarning("Upgrade selection closed without choosing.");
        Hide();
        _combatTransitionService?.EndCombat();
    }
}

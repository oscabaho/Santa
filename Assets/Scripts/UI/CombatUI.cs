using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

/// <summary>
/// Manages the combat UI, including player stats display and action buttons.
/// </summary>
public class CombatUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject playerTurnPanel;

    [Header("Player Stat Displays")]
    [SerializeField] private Slider playerHealthSlider;
    [SerializeField] private TextMeshProUGUI playerAPText;

    [Header("Action Buttons")]
    [SerializeField] private Button directAttackButton;
    [SerializeField] private Button areaAttackButton;
    [SerializeField] private Button specialAttackButton;
    [SerializeField] private Button meditateButton; // New button for gaining AP

    [Header("Ability Assets")]
    [Tooltip("Assign the ScriptableObject for the Direct Attack here.")]
    [SerializeField] private Ability _directAttackAbility;
    [Tooltip("Assign the ScriptableObject for the Area Attack here.")]
    [SerializeField] private Ability _areaAttackAbility;
    [Tooltip("Assign the ScriptableObject for the Special Attack here.")]
    [SerializeField] private Ability _specialAttackAbility;
    [Tooltip("Assign the ScriptableObject for the Meditate Ability here.")]
    [SerializeField] private Ability _meditateAbility;

    private HealthComponentBehaviour _playerHealth;
    private ActionPointComponentBehaviour _playerAP;

    private void Start()
    {
        if (_directAttackAbility == null || _areaAttackAbility == null || _specialAttackAbility == null)
        {
            Debug.LogError("One or more Ability assets are not assigned in CombatUI.", this);
            directAttackButton.interactable = false;
            areaAttackButton.interactable = false;
            specialAttackButton.interactable = false;
        }

        directAttackButton.onClick.AddListener(() => RequestAbility(_directAttackAbility));
        areaAttackButton.onClick.AddListener(() => RequestAbility(_areaAttackAbility));
        specialAttackButton.onClick.AddListener(() => RequestAbility(_specialAttackAbility));
        meditateButton.onClick.AddListener(() => RequestAbility(_meditateAbility));

        playerTurnPanel.SetActive(false);
    }

    private void OnEnable()
    {
        var combatService = ServiceLocator.Get<ICombatService>();
        if (combatService != null)
        {
            combatService.OnPlayerTurnStarted += OnPlayerTurnStarted;
            combatService.OnPlayerTurnEnded += OnPlayerTurnEnded;
        }
    }

    private void OnDisable()
    {
        var combatService = ServiceLocator.Get<ICombatService>();
        if (combatService != null)
        {
            combatService.OnPlayerTurnStarted -= OnPlayerTurnStarted;
            combatService.OnPlayerTurnEnded -= OnPlayerTurnEnded;
        }
        UnsubscribeFromPlayerEvents();
    }

    private void OnPlayerTurnStarted()
    {
        playerTurnPanel.SetActive(true);
        SubscribeToPlayerEvents();
    }

    private void OnPlayerTurnEnded()
    {
        playerTurnPanel.SetActive(false);
        UnsubscribeFromPlayerEvents();
    }

    private void SubscribeToPlayerEvents()
    {
        // Find player and subscribe to their stat changes
        var combatService = ServiceLocator.Get<ICombatService>();
        if (combatService == null) return;

        // This assumes the player is the first non-enemy combatant
        GameObject player = combatService.AllCombatants.FirstOrDefault(c => c != null && c.CompareTag("Player"));

        if (player != null)
        {
            _playerHealth = player.GetComponent<HealthComponentBehaviour>();
            _playerAP = player.GetComponent<ActionPointComponentBehaviour>();

            if (_playerHealth != null)
            {
                _playerHealth.Health.OnValueChanged += UpdateHealthUI;
                UpdateHealthUI(_playerHealth.CurrentValue, _playerHealth.MaxValue);
            }
            if (_playerAP != null)
            {
                _playerAP.ActionPoints.OnValueChanged += UpdateAPUI;
                UpdateAPUI(_playerAP.CurrentValue, _playerAP.MaxValue);
            }
        }
    }

    private void UnsubscribeFromPlayerEvents()
    {
        if (_playerHealth != null)
        {
            _playerHealth.Health.OnValueChanged -= UpdateHealthUI;
        }
        if (_playerAP != null)
        {
            _playerAP.ActionPoints.OnValueChanged -= UpdateAPUI;
        }
        _playerHealth = null;
        _playerAP = null;
    }

    private void UpdateHealthUI(int current, int max)
    {
        if (playerHealthSlider != null) playerHealthSlider.value = (float)current / max;
    }

    private void UpdateAPUI(int current, int max)
    {
        if (playerAPText != null) playerAPText.text = $"PA: {current}";
    }

    private void RequestAbility(Ability ability)
    {
        var combatService = ServiceLocator.Get<ICombatService>();
        if (ability == null || combatService == null) return;

        GameObject primaryTarget = combatService.Enemies.FirstOrDefault(enemy => enemy != null && enemy.activeInHierarchy);

        if (primaryTarget == null && (ability.Targeting == TargetingStyle.SingleEnemy || ability.Targeting == TargetingStyle.RandomEnemies))
        {
            Debug.LogWarning("Could not find a valid enemy to target for this UI.");
        }

        combatService.SubmitPlayerAction(ability, primaryTarget);
    }

}

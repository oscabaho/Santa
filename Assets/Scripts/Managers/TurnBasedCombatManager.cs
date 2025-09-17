using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the logic for turn-based combat, including turn order and combat state.
/// </summary>
public class TurnBasedCombatManager : MonoBehaviour
{
    public static TurnBasedCombatManager Instance { get; private set; }

    [Header("Action Point Settings")]
    [Tooltip("How many player turns must pass before AP is restored.")]
    [SerializeField] private int turnsPerAPRestore = 2;
    [Tooltip("The amount of AP to restore when the condition is met.")]
    [SerializeField] private int amountOfAPToRestore = 10;

    private Queue<GameObject> _turnOrder = new Queue<GameObject>();
    private GameObject _currentCombatant;
    private int _playerTurnCounter = 0;

    public GameObject CurrentCombatant => _currentCombatant;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void StartCombat(List<GameObject> participants)
    {
        if (participants == null || participants.Count == 0)
        {
            Debug.LogError("StartCombat called with no participants.");
            return;
        }

        _playerTurnCounter = 0;
        _turnOrder.Clear();
        foreach (var participant in participants)
        {
            _turnOrder.Enqueue(participant);
        }

        Debug.Log($"Combat started with {participants.Count} participants!");
        NextTurn();
    }

    public void NextTurn()
    {
        // This is a placeholder for checking win/loss conditions.
        // For now, we'll just check if the queue is empty.
        if (_turnOrder.Count == 0)
        {
            EndCombat();
            return;
        }

        _currentCombatant = _turnOrder.Dequeue();
        _turnOrder.Enqueue(_currentCombatant);

        Debug.Log($"Turn started for: {_currentCombatant.name}");

        // Check if the new turn belongs to the player to handle AP restoration.
        if (_currentCombatant.CompareTag("Player"))
        {
            _playerTurnCounter++;
            if (_playerTurnCounter > 1 && (_playerTurnCounter - 1) % turnsPerAPRestore == 0)
            {
                var stamina = _currentCombatant.GetComponent<StaminaComponentBehaviour>();
                if (stamina != null)
                {
                    stamina.AffectValue(amountOfAPToRestore);
                    Debug.Log($"Player restored {amountOfAPToRestore} AP! Current AP: {stamina.CurrentValue}");
                }
            }
        }
    }

    private void EndCombat()
    {
        Debug.Log("Combat has ended.");
        // Notify the transition manager to switch back to exploration mode.
        CombatTransitionManager.Instance.EndCombat();
    }
}
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds all the data for the current combat session.
/// This class is responsible for managing the state of combatants, not the flow of combat.
/// </summary>
public class CombatState
{
    public IReadOnlyList<GameObject> AllCombatants => _combatants;
    public IReadOnlyList<GameObject> Enemies => _enemies;
    public GameObject Player => _player;

    public IReadOnlyDictionary<GameObject, IHealthController> HealthComponents => _healthComponents;
    public IReadOnlyDictionary<GameObject, IActionPointController> APComponents => _apComponents;
    public IReadOnlyDictionary<GameObject, IBrain> Brains => _brains;

    public List<PendingAction> PendingActions { get; } = new List<PendingAction>();

    private readonly List<GameObject> _combatants = new List<GameObject>();
    private readonly List<GameObject> _enemies = new List<GameObject>();
    private GameObject _player;

    private readonly Dictionary<GameObject, IHealthController> _healthComponents = new Dictionary<GameObject, IHealthController>();
    private readonly Dictionary<GameObject, IActionPointController> _apComponents = new Dictionary<GameObject, IActionPointController>();
    private readonly Dictionary<GameObject, IBrain> _brains = new Dictionary<GameObject, IBrain>();

    public void Initialize(List<GameObject> participants)
    {
        Clear();
        _combatants.AddRange(participants);

        foreach (var combatant in _combatants)
        {
            if (combatant == null) continue;

            // Cache components
            var health = combatant.GetComponent<IHealthController>();
            if (health != null) _healthComponents[combatant] = health;

            var ap = combatant.GetComponent<IActionPointController>();
            if (ap != null)
            {
                _apComponents[combatant] = ap;
                ap.SetValue(ap.MaxValue);
            }

            var brain = combatant.GetComponent<IBrain>();
            if (brain != null) _brains[combatant] = brain;

            // Identify player and enemies
            if (combatant.CompareTag("Player"))
            {
                _player = combatant;
            }
            else if (combatant.CompareTag("Enemy"))
            {
                _enemies.Add(combatant);
            }
        }
    }

    public void Clear()
    {
        _combatants.Clear();
        _enemies.Clear();
        _player = null;
        _healthComponents.Clear();
        _apComponents.Clear();
        _brains.Clear();
        PendingActions.Clear();
    }
}

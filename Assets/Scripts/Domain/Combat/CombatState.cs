using System.Collections.Generic;
using Santa.Core;
using Santa.Core.Config;
using UnityEngine;

namespace Santa.Domain.Combat
{
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

        private readonly List<GameObject> _combatants = new();
        private readonly List<GameObject> _enemies = new();
        private GameObject _player;

        private readonly Dictionary<GameObject, IHealthController> _healthComponents = new();
        private readonly Dictionary<GameObject, IActionPointController> _apComponents = new();
        private readonly Dictionary<GameObject, IBrain> _brains = new();

        public void Initialize(List<GameObject> participants)
        {
            Clear();
            _combatants.AddRange(participants);

            foreach (var combatant in _combatants)
            {
                if (combatant == null) continue;

                // Cache components
                if (combatant.TryGetComponent<IHealthController>(out var health))
                    _healthComponents[combatant] = health;

                if (combatant.TryGetComponent<IActionPointController>(out var ap))
                {
                    _apComponents[combatant] = ap;
                    ap.SetValue(ap.MaxValue);
                }

                if (combatant.TryGetComponent<IBrain>(out var brain))
                    _brains[combatant] = brain;

                // Identify player and enemies
                if (combatant.CompareTag(GameConstants.Tags.Player))
                {
                    _player = combatant;
                }
                else if (combatant.CompareTag(GameConstants.Tags.Enemy))
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
}

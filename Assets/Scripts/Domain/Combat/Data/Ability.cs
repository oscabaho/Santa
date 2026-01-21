using System.Collections.Generic;
using Santa.Core;
using UnityEngine;

namespace Santa.Domain.Combat
{
    /// <summary>
    /// Base class for all abilities, implemented as a ScriptableObject.
    /// This allows creating abilities as data assets in the editor.
    /// </summary>
    public abstract class Ability : ScriptableObject
    {
        [Header("Info")]
        [SerializeField] private string _abilityName;
        [SerializeField][TextArea] private string _description;

        [Header("Properties")]
        [SerializeField] private int _apCost = 1;
        [SerializeField] private TargetingStrategy _targeting;
        [Tooltip("Percentage of targets to affect for RandomEnemies style (0.0 to 1.0)")]
        [SerializeField][Range(0f, 1f)] private float _targetPercentage = 1f;
        [Tooltip("Determines turn order. Higher value = faster action (executes earlier in the turn).")]
        [SerializeField] private int _actionSpeed = 100;

        public string AbilityName => _abilityName;
        public string Description => _description;
        public int ApCost => _apCost;
        public TargetingStrategy Targeting => _targeting;
        public float TargetPercentage => _targetPercentage;
        public int ActionSpeed => _actionSpeed;

        /// <summary>
        /// Executes the ability's logic on the given targets.
        /// </summary>
        /// <param name="targets">The list of targets, determined by the combat manager.</param>
        /// <param name="caster">The GameObject performing the ability.</param>
        /// <param name="upgradeService">Service providing player stats from upgrades.</param>
        /// <param name="allCombatants">All combatants in the battle, for abilities that need additional targeting context.</param>
        /// <param name="combatLogService">Service for logging combat messages to the UI.</param>
        public abstract void Execute(List<GameObject> targets, GameObject caster, IUpgradeService upgradeService, IReadOnlyList<GameObject> allCombatants, ICombatLogService combatLogService);

        /// <summary>
        /// Rolls for a critical hit based on upgrade service critical chance.
        /// </summary>
        protected bool RollCriticalHit(IUpgradeService upgradeService)
        {
            return upgradeService != null
                && upgradeService.CriticalHitChance > 0f
                && Random.value < upgradeService.CriticalHitChance;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Validates ability data in the Unity Editor to prevent invalid configurations.
        /// </summary>
        private void OnValidate()
        {
            // Ensure AP cost is at least 1
            _apCost = Mathf.Max(1, _apCost);

            // Ensure action speed is not negative
            _actionSpeed = Mathf.Max(0, _actionSpeed);

            // Clamp target percentage to valid range
            _targetPercentage = Mathf.Clamp01(_targetPercentage);

            // Auto-generate ability name from asset name if empty
            if (string.IsNullOrEmpty(_abilityName))
            {
                _abilityName = name;
            }
        }
#endif
    }
}

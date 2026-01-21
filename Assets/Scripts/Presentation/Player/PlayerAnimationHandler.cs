using UnityEngine;

namespace Santa.Presentation.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationHandler : MonoBehaviour
    {
        private Animator animator;

        // Hash IDs for performance
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");
        private static readonly int IsInCombatHash = Animator.StringToHash("IsInCombat");

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Updates the movement speed in the animator.
        /// </summary>
        /// <param name="speed">Current movement speed (0 for Idle, >0.1 for Walk/Run)</param>
        public void UpdateSpeed(float speed)
        {
            animator.SetFloat(SpeedHash, speed);
        }

        /// <summary>
        /// Triggers the attack animation.
        /// </summary>
        public void TriggerAttack()
        {
            animator.SetTrigger(AttackTriggerHash);
        }

        /// <summary>
        /// Switches between exploration and combat animations.
        /// </summary>
        /// <param name="inCombat">True for BattleIdle, False for normal Idle</param>
        public void SetCombatState(bool inCombat)
        {
            animator.SetBool(IsInCombatHash, inCombat);
        }
    }
}

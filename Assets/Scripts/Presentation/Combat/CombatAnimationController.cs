using UnityEngine;

namespace Santa.Presentation.Combat
{
    [RequireComponent(typeof(Animator))]
    public class CombatAnimationController : MonoBehaviour
    {
        private Animator _animator;
        private static readonly int StartBattleHash = Animator.StringToHash("StartBattle");
        private static readonly int AttackHash = Animator.StringToHash("Attack");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Triggers the BattleStart animation. 
        /// Should be called when the camera transition is complete.
        /// </summary>
        public void PlayBattleStart()
        {
            if (_animator != null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.Log($"[CombatAnimationController] PlayBattleStart called on {name}. Animator found: YES");
#endif
                _animator.SetTrigger(StartBattleHash);
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogError($"[CombatAnimationController] PlayBattleStart called on {name} but Animator is NULL!");
#endif
            }
        }

        /// <summary>
        /// Triggers the Attack animation.
        /// Should be called by the ActionExecutor when performing an attack.
        /// </summary>
        public void TriggerAttack()
        {
            if (_animator != null)
            {
                _animator.SetTrigger(AttackHash);
            }
        }
    }
}

using UnityEngine;

namespace ProyectSecret.MonoBehaviours.Player
{
    /// <summary>
    /// --- a/e:\osbah\Entregables\Proyect-Secret\Assets\Scripts\MonoBehaviours\Player\PlayerAnimationController.cs
    /// +++ b/e:\osbah\Entregables\Proyect-Secret\Assets\Scripts\MonoBehaviours\Player\PlayerAnimationController.cs
    /// Responsabilidad Única: Controlar el Animator y el SpriteRenderer del jugador.
    /// Lee el estado de otros componentes (Input, Physics) para actualizar la vista.
    /// </summary>
    [RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
    [RequireComponent(typeof(PlayerInputController), typeof(PaperMarioPlayerMovement))]
    public class PlayerAnimationController : MonoBehaviour
    {
        // Referencias a componentes
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private PlayerInputController _input;
        private PaperMarioPlayerMovement _movement;

        // Hashes de parámetros del Animator para optimización
        private readonly int _moveXHash = Animator.StringToHash("MoveX");
        private readonly int _moveYHash = Animator.StringToHash("MoveY");
        private readonly int _isMovingHash = Animator.StringToHash("IsMoving");
        private readonly int _isGroundedHash = Animator.StringToHash("IsGrounded");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _input = GetComponent<PlayerInputController>();
            _movement = GetComponent<PaperMarioPlayerMovement>();

#if UNITY_EDITOR
            ValidateAnimatorParameters();
#endif
        }

        private void Update()
        {
            Vector2 moveInput = _input.MoveInput;

            _animator.SetFloat(_moveXHash, moveInput.x);
            _animator.SetFloat(_moveYHash, moveInput.y);
            _animator.SetBool(_isMovingHash, moveInput.sqrMagnitude > 0.01f);
            _animator.SetBool(_isGroundedHash, _movement.IsGrounded);

            // FlipX para mirar a la derecha/izquierda según el input
            if (Mathf.Abs(moveInput.x) > 0.1f)
            {
                _spriteRenderer.flipX = moveInput.x < 0f;
            }
        }

#if UNITY_EDITOR
        // Este método se ejecuta solo en el editor para advertir sobre parámetros faltantes.
        private void ValidateAnimatorParameters()
        {
            if (_animator == null || _animator.runtimeAnimatorController == null) return;

            var parameters = new System.Collections.Generic.HashSet<string>();
            foreach (var param in _animator.parameters)
            {
                parameters.Add(param.name);
            }

            if (!parameters.Contains("MoveX")) Debug.LogWarning("PlayerAnimationController: Falta el parámetro 'MoveX' (Float) en el Animator.", this);
            if (!parameters.Contains("MoveY")) Debug.LogWarning("PlayerAnimationController: Falta el parámetro 'MoveY' (Float) en el Animator.", this);
            if (!parameters.Contains("IsMoving")) Debug.LogWarning("PlayerAnimationController: Falta el parámetro 'IsMoving' (Bool) en el Animator.", this);
            if (!parameters.Contains("IsGrounded")) Debug.LogWarning("PlayerAnimationController: Falta el parámetro 'IsGrounded' (Bool) en el Animator.", this);
        }
#endif
    }
}
using UnityEngine;
using ProyectSecret.Audio;
using ProyectSecret.Managers;

namespace ProyectSecret.MonoBehaviours.Player
{
    /// <summary>
    /// Responsabilidad Única: Gestionar los efectos de sonido del jugador.
    /// </summary>
    [RequireComponent(typeof(PaperMarioPlayerMovement), typeof(PlayerInputController))]
    public class PlayerAudioController : MonoBehaviour
    {
        [Header("Audio Data")]
        [SerializeField] private AudioData moveSoundData;

        private PaperMarioPlayerMovement _movement;
        private PlayerInputController _input;
        private bool _wasMovingLastFrame = false;

        private void Awake()
        {
            _movement = GetComponent<PaperMarioPlayerMovement>();
            _input = GetComponent<PlayerInputController>();
        }

        private void Update()
        {
            bool isCurrentlyMoving = _movement.CurrentVelocity.sqrMagnitude > 0.01f;

            if (isCurrentlyMoving && !_wasMovingLastFrame)
            {
                if (moveSoundData != null && moveSoundData.clips.Length > 0)
                    AudioManager.Instance?.PlayLoopingSoundOnObject(moveSoundData, gameObject);
                else
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("PlayerAudioController: 'Move Sound Data' no está asignado o no tiene clips. No se reproducirá sonido de movimiento.", this);
                    #endif
                }
            }
            else if (!isCurrentlyMoving && _wasMovingLastFrame)
                AudioManager.Instance?.StopLoopingSoundOnObject(gameObject);

            _wasMovingLastFrame = isCurrentlyMoving;
        }
    }
}
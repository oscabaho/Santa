using System;
using Santa.Core;
using Santa.Infrastructure.Input;
using UnityEngine;
using VContainer;

namespace Santa.Infrastructure.Combat
{
    // It is expected that this component is attached to the same GameObject as the Movement script.
    // It requires an InputReader to be assigned in the inspector.
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Dependencies")]
        [Tooltip("The InputReader ScriptableObject that provides player input events.")]
        [SerializeField] private InputReader inputReader;

        private CombatTrigger _currentCombatTrigger;
        private IGameplayUIService _gameplayUIService;
        private IGameStateService _gameStateService;
        private IObjectResolver _resolver;
        private bool? _desiredActionVisible; // null = no preference yet
        private bool _readyHooked;
        private Action _onReadyHandler; // Store handler to properly unsubscribe
        private float _lastNoTriggerLogTime;
        private const float NoTriggerLogCooldown = 2f; // seconds between warning logs

        // [Inject] removed to support runtime instantiation without strict scoping

        private void Awake()
        {
            // Try to auto-acquire InputReader if not assigned
            if (inputReader == null)
            {
                // Fallback: Try to load from Resources by name
                inputReader = Resources.Load<InputReader>("InputReader");
                if (inputReader != null)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    GameLog.LogVerbose("PlayerInteraction: Loaded 'InputReader' from Resources.", this);
#else
                GameLog.Log("PlayerInteraction: Loaded InputReader from Resources");
#endif
                }
                else
                {
                    // Last ditch: Find any loaded instance (editor-only mainly)
                    var readers = Resources.FindObjectsOfTypeAll<InputReader>();
                    if (readers != null && readers.Length > 0)
                    {
                        inputReader = readers[0];
                        GameLog.Log("PlayerInteraction: inputReader fallback found via FindObjectsOfTypeAll (unsafe for build).");
                    }
                    else
                    {
                        GameLog.LogError("PlayerInteraction: InputReader NOT FOUND in Resources! Mobile combat will not work.", this);
                    }
                }
            }

            if (inputReader == null)
            {
                GameLog.LogError($"PlayerInteraction CRITICAL: InputReader is NULL on {gameObject.name}! Cannot subscribe to interact events.", this);
            }
        }

        private void OnEnable()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogVerbose($"PlayerInteraction: OnEnable. InputReader={inputReader?.name ?? "NULL"}", this);
#else
        // Log even in Release
        GameLog.Log($"PlayerInteraction: OnEnable. InputReader={(inputReader != null ? "Ready" : "NULL - CRITICAL ERROR")}");
#endif

            if (_gameplayUIService == null)
            {
                var ui = FindFirstObjectByType<Santa.Presentation.UI.GameplayUIManager>();
                if (ui != null)
                {
                    _gameplayUIService = ui;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    GameLog.LogVerbose("PlayerInteraction: Found GameplayUIManager.", this);
#endif
                }
                else
                {
                    GameLog.LogError("PlayerInteraction: GameplayUIManager NOT found! Action button visibility won't be managed.", this);
                }
            }

            if (_gameStateService == null)
            {
                // Try to find the global state manager
                var gs = FindFirstObjectByType<Santa.Infrastructure.State.GameStateManager>();
                if (gs != null)
                {
                    _gameStateService = gs;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    GameLog.LogVerbose("PlayerInteraction: Found GameStateManager.", this);
#endif
                }
            }

            // Subscribe to combat end event to clear interaction state
            if (_gameStateService != null)
            {
                _gameStateService.OnCombatEnded += OnCombatEnded;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose("PlayerInteraction: Subscribed to OnCombatEnded.", this);
#endif
            }
            else
            {
                GameLog.LogWarning("PlayerInteraction: GameStateService not found - combat end events won't be handled.", this);
            }

            // Hook a single ready handler to apply latest desired state when UI becomes ready
            if (_gameplayUIService != null && !_readyHooked)
            {
                _readyHooked = true;
                _onReadyHandler = () =>
                {
                    if (_desiredActionVisible.HasValue)
                    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        GameLog.LogVerbose($"PlayerInteraction: UI Ready - applying desired button state: {_desiredActionVisible.Value}", this);
#endif
                        _gameplayUIService.ShowActionButton(_desiredActionVisible.Value);
                    }
                };
                _gameplayUIService.Ready += _onReadyHandler;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose("PlayerInteraction: Hooked to GameplayUIService.Ready event.", this);
#endif
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from gameplay UI ready event
            if (_gameplayUIService != null && _onReadyHandler != null)
            {
                _gameplayUIService.Ready -= _onReadyHandler;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose("PlayerInteraction: Unsubscribed from UI Ready event.", this);
#endif
            }

            // Unsubscribe from combat end event
            if (_gameStateService != null)
            {
                _gameStateService.OnCombatEnded -= OnCombatEnded;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose("PlayerInteraction: Unsubscribed from OnCombatEnded.", this);
#endif
            }
        }

        private void OnCombatEnded(bool playerWon)
        {
            // Clear interaction state when combat ends
            // This ensures the button is hidden when returning to exploration
            _currentCombatTrigger = null;
            _desiredActionVisible = false;

            if (_gameplayUIService != null)
            {
                _gameplayUIService.ShowActionButton(false);
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogVerbose("PlayerInteraction: Combat ended, cleared interaction state and hid action button.", this);
#endif
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<CombatTrigger>(out var combatTrigger))
            {
                // Removed manual injection via resolver to avoid dependency issues. 
                // CombatTrigger should rely on singleton access or FindFirstObjectByType if needed.
                _currentCombatTrigger = combatTrigger;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose($"PlayerInteraction: Entered trigger zone '{combatTrigger.name}'. Setting as current trigger.", this);
#else
            GameLog.Log($"PlayerInteraction: Combat trigger area entered - button will become active");
#endif

                _desiredActionVisible = true;

                if (_gameplayUIService == null)
                {
                    var ui = FindFirstObjectByType<Santa.Presentation.UI.GameplayUIManager>();
                    if (ui != null)
                    {
                        _gameplayUIService = ui;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        GameLog.LogVerbose("PlayerInteraction: Late-acquired GameplayUIManager.", this);
#endif
                    }
                    else
                    {
                        GameLog.LogError("PlayerInteraction: GameplayUIManager still not found in OnTriggerEnter! Button won't show.", this);
                    }
                }

                if (_gameplayUIService != null)
                {
                    // Command Pattern: Pass the restart logic directly to the button
                    _gameplayUIService.ShowActionButton(true, () =>
                    {
                        if (_currentCombatTrigger != null)
                        {
                            GameLog.Log($"PlayerInteraction: Executing command on '{_currentCombatTrigger.name}'");
                            _currentCombatTrigger.StartCombatInteraction().Forget();
                        }
                    });

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    GameLog.LogVerbose($"PlayerInteraction: Called ShowActionButton(true) for '{combatTrigger.name}'", this);
#else
                    GameLog.Log("PlayerInteraction: Action button shown - ready to trigger combat");
#endif
                }
                else
                {
                    GameLog.LogError("PlayerInteraction CRITICAL: GameplayUIService is NULL in OnTriggerEnter! Button cannot be shown.", this);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<CombatTrigger>(out var combatTrigger) && combatTrigger == _currentCombatTrigger)
            {
                _currentCombatTrigger = null;
                _desiredActionVisible = false;
                if (_gameplayUIService != null)
                {
                    _gameplayUIService.ShowActionButton(false);
                }
            }
        }
    }
}
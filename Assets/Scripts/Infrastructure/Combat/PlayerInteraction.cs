using System;
using UnityEngine;
using VContainer;
using Santa.Core;
using Santa.Infrastructure.Input;

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
            var readers = Resources.FindObjectsOfTypeAll<InputReader>();
            if (readers != null && readers.Length > 0)
            {
                inputReader = readers[0];
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose("PlayerInteraction: Auto-acquired InputReader from Resources.", this);
#endif
            }
        }

        if (inputReader == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError($"InputReader is not assigned in the inspector on {gameObject.name}!", this);
#endif
        }
    }

    private void OnEnable()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.LogVerbose($"PlayerInteraction: OnEnable. InputReader={inputReader?.name ?? "NULL"}", this);
#endif

        if (_gameplayUIService == null)
        {
             var ui = FindFirstObjectByType<Santa.Presentation.UI.GameplayUIManager>();
             if (ui != null) _gameplayUIService = ui;
        }

        if (_gameStateService == null)
        {
             // Try to find the global state manager
             var gs = FindFirstObjectByType<Santa.Infrastructure.State.GameStateManager>();
             if (gs != null) _gameStateService = gs;
        }

        if (inputReader != null)
        {
            inputReader.InteractEvent += OnInteract;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogVerbose($"PlayerInteraction: Subscribed to InteractEvent on InputReader '{inputReader.name}'.", this);
#else
            // CRITICAL: Confirm subscription even in Release
            GameLog.Log($"PlayerInteraction: Subscribed to InputReader '{inputReader.name}'");
#endif
        }
        else
        {
            GameLog.LogError("PlayerInteraction: InputReader is NULL - cannot subscribe to InteractEvent!");
        }

        // Subscribe to combat end event to clear interaction state
        if (_gameStateService != null)
        {
            _gameStateService.OnCombatEnded += OnCombatEnded;
        }

        // Hook a single ready handler to apply latest desired state when UI becomes ready
        if (_gameplayUIService != null && !_readyHooked)
        {
            _readyHooked = true;
            _onReadyHandler = () =>
            {
                if (_desiredActionVisible.HasValue)
                {
                    _gameplayUIService.ShowActionButton(_desiredActionVisible.Value);
                }
            };
            _gameplayUIService.Ready += _onReadyHandler;
        }
    }

    private void OnDisable()
    {
        if (inputReader != null)
        {
            inputReader.InteractEvent -= OnInteract;
        }

        // Unsubscribe from gameplay UI ready event
        if (_gameplayUIService != null && _onReadyHandler != null)
        {
            _gameplayUIService.Ready -= _onReadyHandler;
        }

        // Unsubscribe from combat end event
        if (_gameStateService != null)
        {
            _gameStateService.OnCombatEnded -= OnCombatEnded;
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

    private void OnInteract()
    {
        // CRITICAL: Always log in Release for APK debugging
        GameLog.Log($"PlayerInteraction.OnInteract: Trigger={(_currentCombatTrigger != null ? _currentCombatTrigger.name : "NULL")}");

        if (_currentCombatTrigger != null)
        {
            GameLog.Log($"PlayerInteraction: Starting combat on '{_currentCombatTrigger.name}'");
            _currentCombatTrigger.StartCombatInteraction().Forget();
        }
        else
        {
            // Make this ERROR visible even in Release - player expects combat to start
            GameLog.LogError("PlayerInteraction: Button pressed but no combat trigger active! Button should be hidden.");
        }
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
#endif

            _desiredActionVisible = true;

            if (_gameplayUIService == null)
            {
                var ui = FindFirstObjectByType<Santa.Presentation.UI.GameplayUIManager>();
                if (ui != null) 
                {
                    _gameplayUIService = ui;
                    // Also hook up the ready handler if needed, though for now just showing is enough priority.
                }
            }

            if (_gameplayUIService != null)
            {
                 // Even if not "Ready" (registered), we can call ShowActionButton because GameplayUIManager queues the request.
                 // The "IsReady" check in GameplayUIManager is about the button being registered, but ShowActionButton handles unregistered execution by queuing.
                 // So we can just call it.
                 _gameplayUIService.ShowActionButton(true);
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
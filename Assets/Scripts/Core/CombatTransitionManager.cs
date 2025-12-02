using System.Collections;
using UnityEngine;
using VContainer;

/// <summary>
/// Manages the visual transition between the exploration state and the combat state.
/// This version is decoupled from scene references and discovers objects at runtime.
/// </summary>
public class CombatTransitionManager : MonoBehaviour, ICombatTransitionService
{
    [Header("Transition Sequences")]
    [Tooltip("Sequence of tasks to execute when starting combat.")]
    [SerializeField] private TransitionSequence startCombatSequence;
    [Tooltip("Sequence of tasks to execute when ending combat.")]
    [SerializeField] private TransitionSequence endCombatSequence;

    [Header("Respawn Settings")]
    [Tooltip("The transform where the player should respawn upon defeat.")]
    [SerializeField] private Transform respawnPoint;

    // --- Injected References ---
    private IUIManager _uiManager;
    private IGameStateService _gameStateService;
    private ICombatCameraManager _combatCameraManager;

    // --- Discovered References ---
    private GameObject _explorationCamera;
    private GameObject _explorationPlayer;
    private Santa.Core.Player.IPlayerReference _playerRef;

    // --- Runtime State ---
    private GameObject _currentCombatSceneParent;
    private TransitionContext _currentContext;
    private Coroutine _startSequenceRoutine;
    private Coroutine _endSequenceRoutine;

    [Inject]
    public void Construct(IUIManager uiManager, IGameStateService gameStateService, ICombatCameraManager combatCameraManager = null, Santa.Core.Player.IPlayerReference playerRef = null)
    {
        _uiManager = uiManager;
        _gameStateService = gameStateService;
        _combatCameraManager = combatCameraManager; // May be null; will log on use.
        _playerRef = playerRef;
    }

    private void Awake()
    {
        // Discover persistent exploration objects
        // Prefer injected player reference
        _explorationPlayer = _playerRef != null ? _playerRef.Player : null;
        if (_explorationPlayer == null)
        {
            var playerIdentifier = FindFirstObjectByType<ExplorationPlayerIdentifier>();
            _explorationPlayer = playerIdentifier != null ? playerIdentifier.gameObject : null;
        }
        _explorationCamera = Camera.main != null ? Camera.main.gameObject : null;

        if (_explorationPlayer == null)
        {
            GameLog.LogError(Santa.Core.Config.LogMessages.CombatTransition.ExplorationPlayerNotFound, this);
            enabled = false;
        }
        if (_explorationCamera == null)
        {
            GameLog.LogError(Santa.Core.Config.LogMessages.CombatTransition.MainCameraNotFound, this);
            enabled = false;
        }
    }

    public void StartCombat(GameObject combatSceneParent)
    {
        _currentCombatSceneParent = combatSceneParent;

        // Discover combat-specific objects within the instantiated prefab
        var combatPlayerIdentifier = _currentCombatSceneParent.GetComponentInChildren<CombatPlayerIdentifier>();
        var combatPlayer = combatPlayerIdentifier != null ? combatPlayerIdentifier.gameObject : null;
        if (combatPlayer == null)
        {
            GameLog.LogError(string.Format(Santa.Core.Config.LogMessages.CombatTransition.CombatPlayerNotFound, _currentCombatSceneParent.name), this);
            return;
        }

        // --- DYNAMIC CAMERA ASSIGNMENT ---
        _currentCombatSceneParent.TryGetComponent<CombatArenaSettings>(out var arenaSettings);
        if (arenaSettings != null)
        {
            if (_combatCameraManager != null)
            {
                _combatCameraManager.SetCombatCameras(arenaSettings.MainCombatCamera, arenaSettings.TargetSelectionCamera);
            }
            else
            {
                GameLog.LogWarning(Santa.Core.Config.LogMessages.CombatTransition.CameraManagerNotInjected);
            }
        }
        else
        {
            GameLog.LogWarning(string.Format(Santa.Core.Config.LogMessages.CombatTransition.NoArenaSettings, _currentCombatSceneParent.name));
        }

        // Build and store the context for both start and end transitions
        _currentContext = new TransitionContext();
        _currentContext.AddTarget(TargetId.ExplorationCamera, _explorationCamera);
        _currentContext.AddTarget(TargetId.ExplorationPlayer, _explorationPlayer);
        _currentContext.AddTarget(TargetId.CombatPlayer, combatPlayer);
        _currentContext.AddTarget(TargetId.CombatSceneParent, _currentCombatSceneParent);
        _currentContext.AddToContext("UIManager", _uiManager);
        _currentContext.AddToContext("GameStateService", _gameStateService);

        if (startCombatSequence == null)
        {
            return;
        }

        if (_startSequenceRoutine != null)
        {
            StopCoroutine(_startSequenceRoutine);
        }

        _startSequenceRoutine = StartCoroutine(ExecuteStartSequence());
    }

    public void EndCombat(bool playerWon)
    {
        if (_currentCombatSceneParent == null || _currentContext == null)
        {
            GameLog.LogWarning(Santa.Core.Config.LogMessages.CombatTransition.EndCombatNoContext, this);
            _gameStateService?.EndCombat(playerWon);
            CleanupContext();
            return;
        }

        if (_endSequenceRoutine != null)
        {
            StopCoroutine(_endSequenceRoutine);
        }

        if (endCombatSequence != null)
        {
            _endSequenceRoutine = StartCoroutine(ExecuteEndSequence(playerWon));
        }
        else
        {
            // If no transition sequence, reposition immediately
            if (!playerWon)
            {
                RepositionPlayerOnDefeat();
            }
            _gameStateService?.EndCombat(playerWon);
            CleanupContext();
        }
    }

    private IEnumerator ExecuteStartSequence()
    {
        yield return startCombatSequence.Execute(_currentContext);
        _startSequenceRoutine = null;
    }

    private IEnumerator ExecuteEndSequence(bool playerWon)
    {
        // Change game state FIRST, before visual transitions
        // This ensures that events (OnCombatEnded) fire before UI changes
        _gameStateService?.EndCombat(playerWon);

        // Now execute visual transitions (UI switch, camera transitions, etc.)
        yield return endCombatSequence.Execute(_currentContext);

        // Deactivate combat cameras explicitly AFTER the visual transition is complete.
        // This ensures Cinemachine can blend from the active combat camera to the exploration camera.
        if (_combatCameraManager != null)
        {
            _combatCameraManager.DeactivateCameras();
        }

        // Reposition player AFTER the camera transition is complete
        if (!playerWon)
        {
            RepositionPlayerOnDefeat();
        }

        CleanupContext();
        _endSequenceRoutine = null;
    }

    private void RepositionPlayerOnDefeat()
    {
        if (respawnPoint != null && _explorationPlayer != null)
        {
            GameLog.Log(string.Format(Santa.Core.Config.LogMessages.CombatTransition.PlayerDefeatedRespawning, respawnPoint.position));
            _explorationPlayer.transform.position = respawnPoint.position;
        }
        else
        {
            GameLog.LogWarning(Santa.Core.Config.LogMessages.CombatTransition.PlayerDefeatedNoRespawn);
        }
    }

    private void CleanupContext()
    {
        _currentCombatSceneParent = null;
        _currentContext = null;
    }
}
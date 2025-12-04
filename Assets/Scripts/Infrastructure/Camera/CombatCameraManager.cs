using Santa.Core;
using Unity.Cinemachine;
using UnityEngine;
using VContainer;

namespace Santa.Infrastructure.Camera
{

public class CombatCameraManager : MonoBehaviour, ICombatCameraManager
{
    private CinemachineCamera _mainCombatCamera;
    private CinemachineCamera _targetSelectionCamera;

    private ICombatService _combatService;
    private IGameStateService _gameStateService;
    private IObjectResolver _resolver;
    private bool _inCombat = false;

    private const int ACTIVE_PRIORITY = 1000;
    private const int INACTIVE_PRIORITY = 0;

    [Inject]
    public void Construct(IGameStateService gameStateService, IObjectResolver resolver)
    {
        _gameStateService = gameStateService;
        _resolver = resolver;

        if (_gameStateService != null)
        {
            _gameStateService.OnCombatStarted += HandleCombatStarted;
            _gameStateService.OnCombatEnded += HandleCombatEnded;
        }

        // Start with both combat cameras inactive during Exploration
        SetBothCamerasInactive();
    }

    private void OnDestroy()
    {
        if (_combatService != null)
        {
            _combatService.OnPhaseChanged -= HandlePhaseChanged;
        }
        if (_gameStateService != null)
        {
            _gameStateService.OnCombatStarted -= HandleCombatStarted;
            _gameStateService.OnCombatEnded -= HandleCombatEnded;
        }
    }

    private void HandlePhaseChanged(CombatPhase phase)
    {
        if (!_inCombat) return;

        if (phase == CombatPhase.Targeting)
        {
            SwitchToTargetSelectionCamera();
        }
        else
        {
            SwitchToMainCamera();
        }
    }

    private void HandleCombatStarted()
    {
        _inCombat = true;

        // Lazily resolve the combat service when combat starts to avoid DI cycles
        if (_combatService == null && _resolver != null)
        {
            try
            {
                _combatService = _resolver.Resolve<ICombatService>();
                if (_combatService != null)
                {
                    _combatService.OnPhaseChanged += HandlePhaseChanged;
                }
            }
            catch (System.Exception ex)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogError(string.Format(Santa.Core.Config.LogMessages.CombatCamera.FailedToResolveCombatService, ex.Message));
#endif
            }
        }

        SwitchToMainCamera();
    }

    private void HandleCombatEnded(bool playerWon)
    {
        _inCombat = false;
        if (_combatService != null)
        {
            _combatService.OnPhaseChanged -= HandlePhaseChanged;
            _combatService = null;
        }
        // DeactivateCameras() must be called explicitly by the transition manager.
    }

    /// <summary>
    /// Sets the cameras for the current combat arena.
    /// Called by the CombatTransitionManager when a new arena is instantiated.
    /// </summary>
    public void SetCombatCameras(CinemachineCamera main, CinemachineCamera target)
    {
        _mainCombatCamera = main;
        _targetSelectionCamera = target;

        SetBothCamerasInactive();
    }

    public void SwitchToMainCamera()
    {
        SwitchCamera(_mainCombatCamera, _targetSelectionCamera);
    }

    public void SwitchToTargetSelectionCamera()
    {
        SwitchCamera(_targetSelectionCamera, _mainCombatCamera);
    }

    public void DeactivateCameras()
    {
        SetBothCamerasInactive();
    }

    private void SetBothCamerasInactive()
    {
        if (_mainCombatCamera != null)
        {
            _mainCombatCamera.Priority = INACTIVE_PRIORITY;
            _mainCombatCamera.gameObject.SetActive(false);
        }
        if (_targetSelectionCamera != null)
        {
            _targetSelectionCamera.Priority = INACTIVE_PRIORITY;
            _targetSelectionCamera.gameObject.SetActive(false);
        }
    }

    private void SwitchCamera(CinemachineCamera activeCam, CinemachineCamera inactiveCam)
    {
        if (activeCam != null)
        {
            activeCam.gameObject.SetActive(true);
            activeCam.enabled = true;
            activeCam.Priority = ACTIVE_PRIORITY;
        }

        if (inactiveCam != null)
        {
            inactiveCam.gameObject.SetActive(true);
            inactiveCam.enabled = true;
            inactiveCam.Priority = INACTIVE_PRIORITY;
        }
    }
}
}
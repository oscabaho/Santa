using UnityEngine;
using Unity.Cinemachine;
using VContainer;

public class CombatCameraManager : MonoBehaviour, ICombatCameraManager
{
    [Header("Cameras")]
    [SerializeField] private CinemachineCamera _mainCombatCamera;
    [SerializeField] private CinemachineCamera _targetSelectionCamera;

    private ICombatService _combatService;
    private IGameStateService _gameStateService;
    private bool _inCombat = false;

    private const int ACTIVE_PRIORITY = 100;
    private const int INACTIVE_PRIORITY = 0;

    [Inject]
    public void Construct(ICombatService combatService, IGameStateService gameStateService)
    {
        _combatService = combatService;
        _gameStateService = gameStateService;
        if (_combatService != null)
        {
            _combatService.OnPhaseChanged += HandlePhaseChanged;
        }
        else
        {
            GameLog.LogError("CombatCameraManager could not receive ICombatService via injection.");
        }

        if (_gameStateService != null)
        {
            _gameStateService.OnCombatStarted += HandleCombatStarted;
            _gameStateService.OnCombatEnded += HandleCombatEnded;
        }
        else
        {
            GameLog.LogError("CombatCameraManager could not receive IGameStateService via injection.");
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
        if (!_inCombat)
        {
            // Ignore phase changes when not in combat (defensive)
            return;
        }

        if (phase == CombatPhase.Targeting)
        {
            SwitchToTargetSelectionCamera();
        }
        else
        {
            // In any other phase, always use main camera
            SwitchToMainCamera();
        }
    }

    private void HandleCombatStarted()
    {
        _inCombat = true;
        
        // If cameras are not manually assigned, find them automatically.
        if (_mainCombatCamera == null || _targetSelectionCamera == null)
        {
            FindAndAssignCameras();
        }

        SwitchToMainCamera();
    }

    private void HandleCombatEnded()
    {
        _inCombat = false;
        SetBothCamerasInactive();
    }

    /// <summary>
    /// Finds and assigns combat cameras by looking for their tags in the scene.
    /// This allows for robust, automatic setup without manual assignments in the inspector.
    /// </summary>
    private void FindAndAssignCameras()
    {
        GameLog.Log("CombatCameraManager is searching for cameras by tag...");

        if (_mainCombatCamera == null)
        {
            GameObject mainCamObj = GameObject.FindWithTag("MainCombatCamera");
            if (mainCamObj != null)
            {
                _mainCombatCamera = mainCamObj.GetComponent<CinemachineCamera>();
                GameLog.Log("Found and assigned MainCombatCamera.");
            }
            else
            {
                GameLog.LogError("Could not find GameObject with tag 'MainCombatCamera'.");
            }
        }

        if (_targetSelectionCamera == null)
        {
            GameObject targetCamObj = GameObject.FindWithTag("TargetSelectionCamera");
            if (targetCamObj != null)
            {
                _targetSelectionCamera = targetCamObj.GetComponent<CinemachineCamera>();
                GameLog.Log("Found and assigned TargetSelectionCamera.");
            }
            else
            {
                GameLog.LogError("Could not find GameObject with tag 'TargetSelectionCamera'.");
            }
        }
    }

    public void SwitchToMainCamera()
    {
        if (_mainCombatCamera != null)
        {
            _mainCombatCamera.Priority = ACTIVE_PRIORITY;
            _mainCombatCamera.gameObject.SetActive(true);
        }
        else
        {
            GameLog.LogError("MainCombatCamera is not assigned in CombatCameraManager.");
        }

        if (_targetSelectionCamera != null)
        {
            _targetSelectionCamera.Priority = INACTIVE_PRIORITY;
            _targetSelectionCamera.gameObject.SetActive(false);
        }
        else
        {
            GameLog.LogError("TargetSelectionCamera is not assigned in CombatCameraManager.");
        }

        GameLog.Log($"Switched to MAIN camera. Main(active={_mainCombatCamera?.gameObject.activeSelf}, prio={_mainCombatCamera?.Priority}), Target(active={_targetSelectionCamera?.gameObject.activeSelf}, prio={_targetSelectionCamera?.Priority})");
    }

    public void SwitchToTargetSelectionCamera()
    {
        if (_targetSelectionCamera != null)
        {
            _targetSelectionCamera.Priority = ACTIVE_PRIORITY;
            _targetSelectionCamera.gameObject.SetActive(true);
        }
        else
        {
            GameLog.LogError("TargetSelectionCamera is not assigned in CombatCameraManager.");
        }

        if (_mainCombatCamera != null)
        {
            _mainCombatCamera.Priority = INACTIVE_PRIORITY;
            _mainCombatCamera.gameObject.SetActive(false);
        }
        else
        {
            GameLog.LogError("MainCombatCamera is not assigned in CombatCameraManager.");
        }

        GameLog.Log($"Switched to TARGET-SELECTION camera. Target(active={_targetSelectionCamera?.gameObject.activeSelf}, prio={_targetSelectionCamera?.Priority}), Main(active={_mainCombatCamera?.gameObject.activeSelf}, prio={_mainCombatCamera?.Priority})");
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
        GameLog.Log("Combat cameras set to INACTIVE (Exploration mode).");
    }
}
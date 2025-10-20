using UnityEngine;
using Unity.Cinemachine;

public class CombatCameraManager : MonoBehaviour, ICombatCameraManager
{
    [Header("Cameras")]
    [SerializeField] private CinemachineCamera _mainCombatCamera;
    [SerializeField] private CinemachineCamera _targetSelectionCamera;

    private ICombatService _combatService;

    private const int ACTIVE_PRIORITY = 100;
    private const int INACTIVE_PRIORITY = 0;

    private void Awake()
    {
        ServiceLocator.Register<ICombatCameraManager>(this);
    }

    private void Start()
    {
        _combatService = ServiceLocator.Get<ICombatService>();
        if (_combatService != null)
        {
            _combatService.OnPhaseChanged += HandlePhaseChanged;
            // Set initial camera based on current phase, in case we missed the initial event
            HandlePhaseChanged(_combatService.CurrentPhase);
        }
        else
        {
            GameLog.LogError("CombatCameraManager could not find ICombatService.");
        }
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<ICombatCameraManager>();
        if (_combatService != null)
        {
            _combatService.OnPhaseChanged -= HandlePhaseChanged;
        }
    }

    private void HandlePhaseChanged(CombatPhase phase)
    {
        switch (phase)
        {
            case CombatPhase.Targeting:
                SwitchToTargetSelectionCamera();
                break;

            case CombatPhase.Selection:
            case CombatPhase.Execution:
            case CombatPhase.Victory:
            case CombatPhase.Defeat:
            default:
                SwitchToMainCamera();
                break;
        }
    }

    public void SwitchToMainCamera()
    {
        if (_mainCombatCamera != null)
        {
            _mainCombatCamera.Priority = ACTIVE_PRIORITY;
        }
        else
        {
            GameLog.LogError("MainCombatCamera is not assigned in CombatCameraManager.");
        }

        if (_targetSelectionCamera != null)
        {
            _targetSelectionCamera.Priority = INACTIVE_PRIORITY;
        }
        else
        {
            GameLog.LogError("TargetSelectionCamera is not assigned in CombatCameraManager.");
        }
    }

    public void SwitchToTargetSelectionCamera()
    {
        if (_targetSelectionCamera != null)
        {
            _targetSelectionCamera.Priority = ACTIVE_PRIORITY;
        }
        else
        {
            GameLog.LogError("TargetSelectionCamera is not assigned in CombatCameraManager.");
        }

        if (_mainCombatCamera != null)
        {
            _mainCombatCamera.Priority = INACTIVE_PRIORITY;
        }
        else
        {
            GameLog.LogError("MainCombatCamera is not assigned in CombatCameraManager.");
        }
    }
}
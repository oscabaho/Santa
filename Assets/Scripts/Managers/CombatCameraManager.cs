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
    private IObjectResolver _resolver;
    private bool _inCombat = false;

    private const int ACTIVE_PRIORITY = 1000;
    private const int INACTIVE_PRIORITY = 0;

    [Inject]
    public void Construct(IGameStateService gameStateService, IObjectResolver resolver = null)
    {
        _gameStateService = gameStateService;
        _resolver = resolver;

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

        // Lazily resolve or find the combat service when combat starts to avoid DI cycles
        if (_combatService == null)
        {
            try
            {
                if (_resolver != null)
                {
                    _combatService = _resolver.Resolve<ICombatService>();
                }
            }
            catch
            {
                var manager = FindFirstObjectByType<TurnBasedCombatManager>(FindObjectsInactive.Include);
                _combatService = manager as ICombatService;
            }

            if (_combatService != null)
            {
                _combatService.OnPhaseChanged += HandlePhaseChanged;
            }
            else
            {
                GameLog.LogWarning("CombatCameraManager: ICombatService not found on combat start; camera switching will ignore phase changes.");
            }
        }
        if (_mainCombatCamera == null || _targetSelectionCamera == null)
        {
            FindAndAssignCameras();
        }
        SwitchToMainCamera();
    }

    private void HandleCombatEnded()
    {
        _inCombat = false;
        if (_combatService != null)
        {
            _combatService.OnPhaseChanged -= HandlePhaseChanged;
            _combatService = null;
        }
        SetBothCamerasInactive();
    }

    /// <summary>
    /// Sets the cameras for the current combat arena.
    /// Called by the CombatTransitionManager when a new arena is instantiated.
    /// </summary>
    public void SetCombatCameras(CinemachineCamera main, CinemachineCamera target)
    {
        _mainCombatCamera = main;
        _targetSelectionCamera = target;
        if (_mainCombatCamera == null) GameLog.LogError("SetCombatCameras received a null Main Camera.");
        if (_targetSelectionCamera == null) GameLog.LogError("SetCombatCameras received a null Target Camera.");
        SetBothCamerasInactive();
        GameLog.Log($"CombatCameraManager: Cameras assigned dynamically. Main='{main?.name}', Target='{target?.name}'");
    }

    /// <summary>
    /// Finds and assigns combat cameras by looking for their tags in the scene.
    /// </summary>
    private void FindAndAssignCameras()
    {
        GameLog.Log("CombatCameraManager is searching for cameras by tag (Fallback)...");
        if (_mainCombatCamera == null)
        {
            var mainCamObj = GameObject.FindWithTag("MainCombatCamera");
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
            var targetCamObj = GameObject.FindWithTag("TargetSelectionCamera");
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
        SwitchCamera(_mainCombatCamera, _targetSelectionCamera);
        GameLog.Log($"Switched to MAIN camera. Main(prio={ACTIVE_PRIORITY}), Target(prio={INACTIVE_PRIORITY})");
        StartCoroutine(DebugActiveCamera());
    }

    public void SwitchToTargetSelectionCamera()
    {
        SwitchCamera(_targetSelectionCamera, _mainCombatCamera);
        GameLog.Log($"Switched to TARGET-SELECTION camera. Target(prio={ACTIVE_PRIORITY}), Main(prio={INACTIVE_PRIORITY})");
        StartCoroutine(DebugActiveCamera());
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
        GameLog.Log("Combat cameras set to INACTIVE and deactivated (Exploration mode).");
    }

    private System.Collections.IEnumerator DebugActiveCamera()
    {
        yield return new WaitForSeconds(0.5f);
        var brain = CinemachineBrain.GetActiveBrain(0);
        if (brain != null)
        {
            var activeCam = brain.ActiveVirtualCamera as UnityEngine.Object;
            GameLog.Log($"[CM DEBUG] Active Brain Camera: {activeCam?.name ?? "NULL"}");
            GameLog.Log($"[CM DEBUG] MainCam Priority: {_mainCombatCamera?.Priority}, Active: {_mainCombatCamera?.gameObject.activeInHierarchy}");
            GameLog.Log($"[CM DEBUG] TargetCam Priority: {_targetSelectionCamera?.Priority}, Active: {_targetSelectionCamera?.gameObject.activeInHierarchy}");
        }
        else
        {
            GameLog.LogError("[CM DEBUG] No Active CinemachineBrain found!");
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
        else
        {
            GameLog.LogError("Attempted to switch to a null camera.");
        }

        if (inactiveCam != null)
        {
            inactiveCam.Priority = INACTIVE_PRIORITY;
            // Do not deactivate immediately. Wait for blend.
            StartCoroutine(DisableCameraAfterBlend(inactiveCam));
        }
    }

    private System.Collections.IEnumerator DisableCameraAfterBlend(CinemachineCamera cam)
    {
        float blendTime = 0f;
        var brain = CinemachineBrain.GetActiveBrain(0);
        if (brain != null)
        {
            blendTime = brain.DefaultBlend.Time;
        }

        // Wait for the blend to finish plus a small buffer
        yield return new WaitForSeconds(blendTime + 0.1f);

        if (cam != null && cam.Priority == INACTIVE_PRIORITY)
        {
            cam.gameObject.SetActive(false);
            cam.enabled = false;
        }
    }
}
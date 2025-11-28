using Unity.Cinemachine;
using UnityEngine;
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

    private readonly WaitForSeconds _debugWait = new(0.5f);

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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError("CombatCameraManager could not receive IGameStateService via injection.");
#endif
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning("CombatCameraManager: ICombatService not found on combat start; camera switching will ignore phase changes.");
#endif
            }
        }
        if (_mainCombatCamera == null || _targetSelectionCamera == null)
        {
            FindAndAssignCameras();
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (_mainCombatCamera == null) GameLog.LogError("SetCombatCameras received a null Main Camera.");
        if (_targetSelectionCamera == null) GameLog.LogError("SetCombatCameras received a null Target Camera.");
#endif
        SetBothCamerasInactive();

        string mainName = main != null ? main.name : "null";
        string targetName = target != null ? target.name : "null";
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"CombatCameraManager: Cameras assigned dynamically. Main='{mainName}', Target='{targetName}'");
#endif
    }

    /// <summary>
    /// Finds and assigns combat cameras by looking for their tags in the scene.
    /// </summary>
    private void FindAndAssignCameras()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log("CombatCameraManager is searching for cameras by tag (Fallback)...");
#endif
        if (_mainCombatCamera == null)
        {
            var mainCamObj = GameObject.FindWithTag(GameConstants.Tags.MainCombatCamera);
            if (mainCamObj != null)
            {
                _mainCombatCamera = mainCamObj.GetComponent<CinemachineCamera>();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.Log("Found and assigned MainCombatCamera.");
#endif
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogError($"Could not find GameObject with tag '{GameConstants.Tags.MainCombatCamera}'.");
#endif
            }
        }
        if (_targetSelectionCamera == null)
        {
            var targetCamObj = GameObject.FindWithTag(GameConstants.Tags.TargetSelectionCamera);
            if (targetCamObj != null)
            {
                _targetSelectionCamera = targetCamObj.GetComponent<CinemachineCamera>();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.Log("Found and assigned TargetSelectionCamera.");
#endif
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogError($"Could not find GameObject with tag '{GameConstants.Tags.TargetSelectionCamera}'.");
#endif
            }
        }
    }

    public void SwitchToMainCamera()
    {
        SwitchCamera(_mainCombatCamera, _targetSelectionCamera);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"Switched to MAIN camera. Main(prio={ACTIVE_PRIORITY}), Target(prio={INACTIVE_PRIORITY})");
#endif
        StartCoroutine(DebugActiveCamera());
    }

    public void SwitchToTargetSelectionCamera()
    {
        SwitchCamera(_targetSelectionCamera, _mainCombatCamera);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"Switched to TARGET-SELECTION camera. Target(prio={ACTIVE_PRIORITY}), Main(prio={INACTIVE_PRIORITY})");
#endif
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log("Combat cameras set to INACTIVE and deactivated (Exploration mode).");
#endif
    }

    private System.Collections.IEnumerator DebugActiveCamera()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        yield return _debugWait;
        var brain = CinemachineBrain.GetActiveBrain(0);
        if (brain != null)
        {
            var activeCam = brain.ActiveVirtualCamera as UnityEngine.Object;
            string activeCamName = activeCam != null ? activeCam.name : "NULL";
            GameLog.Log($"[CM DEBUG] Active Brain Camera: {activeCamName}");

            if (_mainCombatCamera != null)
            {
                GameLog.Log($"[CM DEBUG] MainCam Priority: {_mainCombatCamera.Priority}, Active: {_mainCombatCamera.gameObject.activeInHierarchy}");
            }
            else
            {
                GameLog.Log("[CM DEBUG] MainCam is NULL");
            }

            if (_targetSelectionCamera != null)
            {
                GameLog.Log($"[CM DEBUG] TargetCam Priority: {_targetSelectionCamera.Priority}, Active: {_targetSelectionCamera.gameObject.activeInHierarchy}");
            }
            else
            {
                GameLog.Log("[CM DEBUG] TargetCam is NULL");
            }
        }
        else
        {
            GameLog.LogError("[CM DEBUG] No Active CinemachineBrain found!");
        }
#else
        yield break;
#endif
    }

    private void SwitchCamera(CinemachineCamera activeCam, CinemachineCamera inactiveCam)
    {
        if (activeCam != null)
        {
            // Keep cameras enabled; rely on Cinemachine priorities and brain default blend
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
            inactiveCam.gameObject.SetActive(true);
            inactiveCam.enabled = true;
            inactiveCam.Priority = INACTIVE_PRIORITY;
        }
    }
    // Removed manual disable-after-blend; rely on Cinemachine Brain default blends
}
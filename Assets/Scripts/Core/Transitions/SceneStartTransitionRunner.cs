using System.Collections;
using UnityEngine;
using VContainer;

/// <summary>
/// Runs a TransitionSequence at scene start (Awake/Start) to perform things like a fade-in from black
/// using the same Transitions system as CombatTransitionManager. This helps unify behavior and timing.
/// </summary>
public class SceneStartTransitionRunner : MonoBehaviour
{
    [Header("Sequence")]
    [Tooltip("Sequence to execute when the scene starts (e.g., Fade In from black).")]
    [SerializeField] private TransitionSequence sceneStartSequence;

    [Header("Timing")]
    [SerializeField] private bool runOnAwake = true;
    [SerializeField] private bool runOnStart = false;

    // Injected services (optional but recommended for tasks that need them)
    private ScreenFade _screenFade;
    private IUIManager _uiManager;
    private IGameStateService _gameStateService;

    [Inject]
    public void Construct(ScreenFade screenFade = null, IUIManager uiManager = null, IGameStateService gameStateService = null)
    {
        _screenFade = screenFade;
        _uiManager = uiManager;
        _gameStateService = gameStateService;
    }

    private void Awake()
    {
        if (runOnAwake)
        {
            RunSequence();
        }
    }

    private void Start()
    {
        if (runOnStart)
        {
            RunSequence();
        }
    }

    private void RunSequence()
    {
        if (sceneStartSequence == null)
        {
            return;
        }

        var context = new TransitionContext();
        context.AddToContext("ScreenFade", _screenFade ?? FindFirstObjectByType<ScreenFade>(FindObjectsInactive.Include));
        if (_uiManager != null) context.AddToContext("UIManager", _uiManager);
        if (_gameStateService != null) context.AddToContext("GameStateService", _gameStateService);

        StartCoroutine(Run(sceneStartSequence, context));
    }

    private IEnumerator Run(TransitionSequence seq, TransitionContext ctx)
    {
        yield return seq.Execute(ctx);
    }
}

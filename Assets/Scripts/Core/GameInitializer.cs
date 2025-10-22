using UnityEngine;
using VContainer;

public class GameInitializer : MonoBehaviour
{
    private const string InitialUIPanelAddress = "VirtualGamepad";
    private IUIManager _uiManager;

    [Inject]
    public void Construct(IUIManager uiManager)
    {
        _uiManager = uiManager;
    }

    void Start()
    {
        // Espera un frame para asegurarse de que todos los managers (como UIManager) han completado su Awake().
        // Esto es una forma sencilla de manejar el orden de ejecución.
        StartCoroutine(ShowInitialUI());
    }

    private System.Collections.IEnumerator ShowInitialUI()
    {
        // Espera al final del primer frame.
        yield return new WaitForEndOfFrame();

        if (_uiManager != null)
        {
            var task = _uiManager.ShowPanel(InitialUIPanelAddress);
            yield return new WaitUntil(() => task.IsCompleted);
        }
        else
        {
            GameLog.LogError("GameInitializer: IUIManager service was not injected. Make sure it's registered in a LifetimeScope.");
        }
    }
}

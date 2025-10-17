using UnityEngine;
using UnityEngine.AddressableAssets;

public class GameInitializer : MonoBehaviour
{
    [SerializeField]
    private AssetReferenceGameObject initialUIPanel;

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

        var uiManager = ServiceLocator.Get<IUIManager>();
        if (uiManager != null)
        {
            if (initialUIPanel != null && initialUIPanel.RuntimeKeyIsValid())
            {
                var task = uiManager.ShowPanel(initialUIPanel);
                yield return new WaitUntil(() => task.IsCompleted);
            }
            else
            {
                Debug.LogError("GameInitializer: Initial UI Panel reference is not set or not valid.");
            }
        }
        else
        {
            Debug.LogError("GameInitializer: IUIManager service not found. Make sure UIManager has registered itself with the ServiceLocator.");
        }
    }
}

using UnityEngine;

public class GameInitializer : MonoBehaviour
{
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
            // Usamos la "dirección" del Addressable: "VirtualGamepad"
            uiManager.ShowPanel("VirtualGamepad");
        }
        else
        {
            Debug.LogError("GameInitializer: IUIManager service not found. Make sure UIManager has registered itself with the ServiceLocator.");
        }
    }
}

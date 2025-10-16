using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class UIManager : MonoBehaviour, IUIManager
{
    private static UIManager Instance { get; set; }

    // Cambiado para almacenar el GameObject instanciado, para poder liberarlo.
    private Dictionary<string, GameObject> _instantiatedPanels;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Inicializa el diccionario. Se poblará bajo demanda.
        _instantiatedPanels = new Dictionary<string, GameObject>();

        ServiceLocator.Register<IUIManager>(this);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            // Libera todos los paneles instanciados cuando el manager se destruye
            foreach (var panel in _instantiatedPanels.Values)
            {
                Addressables.ReleaseInstance(panel);
            }
            _instantiatedPanels.Clear();

            ServiceLocator.Unregister<IUIManager>();
            Instance = null;
        }
    }

    // Modificado para cargar paneles a través de Addressables
    public async Task ShowPanel(string panelId)
    {
        // Si el panel ya está cargado, simplemente muéstralo.
        if (_instantiatedPanels.TryGetValue(panelId, out var panelInstance))
        {
            panelInstance.GetComponent<UIPanel>()?.Show();
            return;
        }

        // Si no está cargado, instáncialo desde Addressables
        var handle = Addressables.InstantiateAsync(panelId, transform);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            var newPanelInstance = handle.Result;
            _instantiatedPanels[panelId] = newPanelInstance;
            newPanelInstance.GetComponent<UIPanel>()?.Show();
            // Asumiendo que GameLog tiene un método Log. Si no, ajústalo a tu logger.
            GameLog.Log($"UIManager: Panel with ID '{panelId}' loaded and shown.");
        }
        else
        {
            // Asumiendo que GameLog tiene un método LogError.
            GameLog.LogError($"UIManager: Failed to load panel with ID '{panelId}'.");
        }
    }

    // Modificado para liberar el panel de la memoria
    public void HidePanel(string panelId)
    {
        if (_instantiatedPanels.TryGetValue(panelId, out var panelInstance))
        {
            // Para la optimización de la memoria, liberar es mejor que solo ocultar.
            Addressables.ReleaseInstance(panelInstance);
            _instantiatedPanels.Remove(panelId);
            GameLog.Log($"UIManager: Panel with ID '{panelId}' hidden and released.");
        }
        else
        {
            GameLog.LogWarning($"UIManager: Panel with ID '{panelId}' not found or not instantiated.");
        }
    }

    public async Task SwitchToPanel(string panelId)
    {
        // Oculta todos los demás paneles primero
        var currentPanelIds = _instantiatedPanels.Keys.ToList();
        foreach (var id in currentPanelIds)
        {
            if (id != panelId)
            {
                HidePanel(id);
            }
        }

        // Muestra el panel de destino (se cargará si aún no lo está)
        await ShowPanel(panelId);
    }
}

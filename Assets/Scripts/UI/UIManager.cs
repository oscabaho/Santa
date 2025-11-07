using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VContainer;

public class UIManager : MonoBehaviour, IUIManager
{
    private readonly Dictionary<string, GameObject> _addressToInstanceMap = new Dictionary<string, GameObject>();
    private IObjectResolver _resolver;

    [Inject]
    public void Construct(IObjectResolver resolver)
    {
        _resolver = resolver;
    }

    /// <summary>
    /// Inyecta dependencias recursivamente en todos los MonoBehaviour del GameObject y sus hijos
    /// </summary>
    private void InjectRecursively(GameObject instance)
    {
        var components = instance.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var component in components)
        {
            _resolver.Inject(component);
        }
    }

    private void OnDestroy()
    {
        // Release all instantiated panels
        foreach (var panelInstance in _addressToInstanceMap.Values)
        {
            Addressables.ReleaseInstance(panelInstance);
        }
        _addressToInstanceMap.Clear();
    }

    public async Task ShowPanel(string panelAddress)
    {
        if (string.IsNullOrEmpty(panelAddress))
        {
            GameLog.LogError("UIManager: Panel address is null or empty.");
            return;
        }

        // If panel is already cached, just show it.
        if (_addressToInstanceMap.TryGetValue(panelAddress, out var panelInstance))
        {
            var panelComponent = panelInstance.GetComponent<UIPanel>();
            if (panelComponent != null)
            {
                panelComponent.Show();
            }
            else
            {
                GameLog.LogError($"UIManager: Cached panel with address '{panelAddress}' is missing its UIPanel component. The panel will not be shown.");
            }
            return;
        }

        // If not cached, load and instantiate it.
        var handle = Addressables.InstantiateAsync(panelAddress, transform);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            var newPanelInstance = handle.Result;
            _addressToInstanceMap[panelAddress] = newPanelInstance;
            
            // Inyectar dependencias en el panel cargado dinámicamente
            if (_resolver != null)
            {
                InjectRecursively(newPanelInstance);
            }
            
            var panelComponent = newPanelInstance.GetComponent<UIPanel>();
            if (panelComponent != null)
            {
                panelComponent.Show();
                GameLog.Log($"UIManager: Panel with address '{panelAddress}' loaded and shown.");
            }
            else
            {
                // If the component is missing, log an error. The prefab is likely misconfigured.
                GameLog.LogError($"UIManager: Prefab with address '{panelAddress}' does not have a UIPanel component on its root object. The panel will not be shown.");
            }
        }
        else
        {
            GameLog.LogError($"UIManager: Failed to load panel with address '{panelAddress}'.");
        }
    }

    public void HidePanel(string panelAddress)
    {
        if (string.IsNullOrEmpty(panelAddress))
        {
            GameLog.LogError("UIManager: Panel address is null or empty for hiding.");
            return;
        }

        if (_addressToInstanceMap.TryGetValue(panelAddress, out var panelInstance))
        {
            panelInstance.GetComponent<UIPanel>()?.Hide();
            GameLog.Log($"UIManager: Panel with address '{panelAddress}' hidden.");
        }
        else
        {
            GameLog.LogWarning($"UIManager: Panel with address '{panelAddress}' not found or not instantiated.");
        }
    }

    public async Task SwitchToPanel(string panelAddress)
    {
        if (string.IsNullOrEmpty(panelAddress))
        {
            GameLog.LogError("UIManager: Panel address is null or empty for switching.");
            return;
        }

        // Hide all panels except the one we are switching to
        var keysToHide = _addressToInstanceMap.Keys.Where(key => key != panelAddress).ToList();
        foreach (var key in keysToHide)
        {
            HidePanel(key);
        }

        // Show the target panel
        await ShowPanel(panelAddress);
    }

    /// <summary>
    /// Preloads a UI panel by address without showing it, caching the instance for later use.
    /// If the panel is already cached, this is a no-op.
    /// </summary>
    public async Task PreloadPanel(string panelAddress)
    {
        if (string.IsNullOrEmpty(panelAddress))
        {
            GameLog.LogError("UIManager: Panel address is null or empty for preloading.");
            return;
        }

        if (_addressToInstanceMap.ContainsKey(panelAddress))
        {
            // Already cached
            return;
        }

        var handle = Addressables.InstantiateAsync(panelAddress, transform);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            var instance = handle.Result;
            _addressToInstanceMap[panelAddress] = instance;

            // Inyectar dependencias en el panel precargado
            if (_resolver != null)
            {
                InjectRecursively(instance);
            }

            var panel = instance.GetComponent<UIPanel>();
            if (panel != null)
            {
                // Ensure panel remains hidden after preload
                panel.Hide();
                GameLog.Log($"UIManager: Panel '{panelAddress}' preloaded and hidden.");
            }
            else
            {
                GameLog.LogError($"UIManager: Prefab with address '{panelAddress}' is missing UIPanel component on root.");
            }
        }
        else
        {
            GameLog.LogError($"UIManager: Failed to preload panel with address '{panelAddress}'.");
        }
    }
}

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class UIManager : MonoBehaviour, IUIManager
{
    private readonly Dictionary<string, GameObject> _addressToInstanceMap = new Dictionary<string, GameObject>();

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
}

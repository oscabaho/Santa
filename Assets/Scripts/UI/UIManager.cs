using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class UIManager : MonoBehaviour, IUIManager
{
    private static UIManager Instance { get; set; }

    private readonly Dictionary<string, GameObject> _guidToInstanceMap = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        ServiceLocator.Register<IUIManager>(this);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            // Create a copy of the list to avoid modification during iteration
            var allInstances = _guidToInstanceMap.Values.ToList();
            foreach (var panelInstance in allInstances)
            {
                // The AssetReference is not stored here, so we must release the instance directly.
                // This assumes the panel was loaded via Addressables.
                Addressables.ReleaseInstance(panelInstance);
            }
            _guidToInstanceMap.Clear();

            ServiceLocator.Unregister<IUIManager>();
            Instance = null;
        }
    }

    public async Task ShowPanel(AssetReferenceGameObject panelReference)
    {
        string key = panelReference.AssetGUID;
        if (!panelReference.RuntimeKeyIsValid())
        {
            GameLog.LogError($"UIManager: Panel reference is not valid.");
            return;
        }

        if (_guidToInstanceMap.TryGetValue(key, out var panelInstance))
        {
            panelInstance.GetComponent<UIPanel>()?.Show();
            return;
        }

        var handle = panelReference.InstantiateAsync(transform);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            var newPanelInstance = handle.Result;
            _guidToInstanceMap[key] = newPanelInstance;
            newPanelInstance.GetComponent<UIPanel>()?.Show();
            GameLog.Log($"UIManager: Panel with key '{key}' loaded and shown.");
        }
        else
        {
            GameLog.LogError($"UIManager: Failed to load panel with key '{key}'.");
        }
    }

    public void HidePanel(AssetReferenceGameObject panelReference)
    {
        string key = panelReference.AssetGUID;
        if (!panelReference.RuntimeKeyIsValid())
        {
            GameLog.LogError($"UIManager: Panel reference is not valid for hiding.");
            return;
        }

        if (_guidToInstanceMap.TryGetValue(key, out var panelInstance))
        {
            panelReference.ReleaseInstance(panelInstance);
            _guidToInstanceMap.Remove(key);
            GameLog.Log($"UIManager: Panel with key '{key}' hidden and released.");
        }
        else
        {
            GameLog.LogWarning($"UIManager: Panel with key '{key}' not found or not instantiated.");
        }
    }

    public async Task SwitchToPanel(AssetReferenceGameObject panelReference)
    {
        if (!panelReference.RuntimeKeyIsValid())
        {
            GameLog.LogError($"UIManager: Panel reference is not valid for switching.");
            return;
        }

        string keyToKeep = panelReference.AssetGUID;
        var keysToHide = _guidToInstanceMap.Keys.Where(key => key != keyToKeep).ToList();

        foreach (var key in keysToHide)
        {
            if (_guidToInstanceMap.TryGetValue(key, out var panelInstance))
            {
                // To release it, we need the original AssetReference.
                // This is a limitation. A more robust system might map instances back to references.
                // For now, we'll just release the instance.
                Addressables.ReleaseInstance(panelInstance);
                _guidToInstanceMap.Remove(key);
                GameLog.Log($"UIManager: Panel with key '{key}' hidden and released during switch.");
            }
        }

        await ShowPanel(panelReference);
    }
}

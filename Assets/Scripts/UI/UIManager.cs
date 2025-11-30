using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
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
    /// Injects dependencies recursively into all MonoBehaviours on the GameObject and its children
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError(Santa.Core.Config.LogMessages.UI.PanelAddressNull);
#endif
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogError(string.Format(Santa.Core.Config.LogMessages.UI.CachedPanelMissingComponent, panelAddress));
#endif
            }
            return;
        }

        // If not cached, load and instantiate it.
        try
        {
            var handle = Addressables.InstantiateAsync(panelAddress, transform);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var newPanelInstance = handle.Result;
                _addressToInstanceMap[panelAddress] = newPanelInstance;

                // Inject dependencies into the dynamically loaded panel
                if (_resolver != null)
                {
                    InjectRecursively(newPanelInstance);
                }

                var panelComponent = newPanelInstance.GetComponent<UIPanel>();
                if (panelComponent != null)
                {
                    panelComponent.Show();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    GameLog.Log(string.Format(Santa.Core.Config.LogMessages.UI.PanelLoadedAndShown, panelAddress));
#endif
                }
                else
                {
                    // If the component is missing, log an error. The prefab is likely misconfigured.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    GameLog.LogError(string.Format(Santa.Core.Config.LogMessages.UI.PrefabMissingComponent, panelAddress));
#endif
                }
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogError(string.Format(Santa.Core.Config.LogMessages.UI.LoadFailed, panelAddress, handle.Status));
#endif
            }
        }
        catch (System.Exception ex)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError(string.Format(Santa.Core.Config.LogMessages.UI.LoadException, panelAddress, ex.Message));
#endif
        }
    }

    public void HidePanel(string panelAddress)
    {
        if (string.IsNullOrEmpty(panelAddress))
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError(Santa.Core.Config.LogMessages.UI.PanelAddressNullHide);
#endif
            return;
        }

        if (_addressToInstanceMap.TryGetValue(panelAddress, out var panelInstance))
        {
            panelInstance.GetComponent<UIPanel>()?.Hide();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log(string.Format(Santa.Core.Config.LogMessages.UI.PanelHidden, panelAddress));
#endif
        }
        else
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning(string.Format(Santa.Core.Config.LogMessages.UI.PanelNotFound, panelAddress));
#endif
        }
    }

    public async Task SwitchToPanel(string panelAddress)
    {
        if (string.IsNullOrEmpty(panelAddress))
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError(Santa.Core.Config.LogMessages.UI.PanelAddressNullSwitch);
#endif
            return;
        }

        // Hide all panels except the one we are switching to
        // Avoid LINQ allocation: _addressToInstanceMap.Keys.Where(...).ToList()
        var keys = _addressToInstanceMap.Keys;
        foreach (var key in keys)
        {
            if (key != panelAddress)
            {
                HidePanel(key);
            }
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError(Santa.Core.Config.LogMessages.UI.PanelAddressNullPreload);
#endif
            return;
        }

        if (_addressToInstanceMap.ContainsKey(panelAddress))
        {
            // Already cached
            return;
        }

        try
        {
            var handle = Addressables.InstantiateAsync(panelAddress, transform);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var instance = handle.Result;
                _addressToInstanceMap[panelAddress] = instance;

                // Inject dependencies into the preloaded panel
                if (_resolver != null)
                {
                    InjectRecursively(instance);
                }

                var panel = instance.GetComponent<UIPanel>();
                if (panel != null)
                {
                    // Ensure the panel remains hidden after preload
                    panel.Hide();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    GameLog.Log(string.Format(Santa.Core.Config.LogMessages.UI.PanelPreloaded, panelAddress));
#endif
                }
                else
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    GameLog.LogError(string.Format(Santa.Core.Config.LogMessages.UI.PreloadMissingComponent, panelAddress));
#endif
                }
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogError(string.Format(Santa.Core.Config.LogMessages.UI.PreloadFailed, panelAddress, handle.Status));
#endif
            }
        }
        catch (System.Exception ex)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError(string.Format(Santa.Core.Config.LogMessages.UI.PreloadException, panelAddress, ex.Message));
#endif
        }
    }
}

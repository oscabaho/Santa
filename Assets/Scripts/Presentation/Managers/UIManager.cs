using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Santa.Core;
using Santa.Core.Addressables;
using Santa.Core.Config;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;

namespace Santa.Presentation.UI
{

    public class UIManager : MonoBehaviour, IUIManager
    {
        private readonly Dictionary<string, GameObject> _addressToInstanceMap = new();
        private IObjectResolver _resolver;
        [SerializeField] private Transform dynamicPanelsParent; // Optional parent for runtime Addressables panels

        public Transform DynamicPanelsParent => dynamicPanelsParent;

        [Inject]
        public void Construct(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        private void Awake()
        {
            // Auto-find UI/DynamicPanels if not assigned, to avoid manual setup
            if (dynamicPanelsParent == null)
            {
                // Prefer an object named exactly "DynamicPanels"
                var allTransforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                for (int i = 0; i < allTransforms.Length; i++)
                {
                    var t = allTransforms[i];
                    if (t != null && t.name == GameConstants.Hierarchy.DynamicPanels)
                    {
                        dynamicPanelsParent = t;
                        break;
                    }
                }
            }
        }

        private static void EnsureUnderCanvas(GameObject instance)
        {
            if (instance == null) return;

            // If the instance already has a Canvas in its parent chain, we're good
            var parentCanvas = instance.GetComponentInParent<Canvas>(true);
            if (parentCanvas != null) return;

            // Find any Canvas in the scene (prefer one named "StaticCanvas" if present)
            Canvas targetCanvas = null;
            var canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < canvases.Length; i++)
            {
                var c = canvases[i];
                if (c == null) continue;
                if (c.name == GameConstants.Hierarchy.StaticCanvas) { targetCanvas = c; break; }
                if (targetCanvas == null) targetCanvas = c;
            }

            if (targetCanvas != null)
            {
                instance.transform.SetParent(targetCanvas.transform, false);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.Log($"UIManager: Reparented '{instance.name}' under Canvas '{targetCanvas.name}' to ensure visibility.");
#endif
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning($"UIManager: No Canvas found in scene. UI element '{instance.name}' may not render.");
#endif
            }
        }

        private static void BringToFront(GameObject instance)
        {
            if (instance == null) return;

            // If this panel has its own Canvas, bump its sorting order
            if (instance.TryGetComponent<Canvas>(out var selfCanvas))
            {
                selfCanvas.overrideSorting = true;
                // Use different orders to guarantee PauseMenu above HUD
                var isPauseMenu = instance.name.StartsWith("PauseMenu");
                var targetOrder = isPauseMenu ? 6000 : 5000;
                if (selfCanvas.sortingOrder != targetOrder)
                    selfCanvas.sortingOrder = targetOrder;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.Log($"UIManager: '{instance.name}' Canvas sortingOrder set to {selfCanvas.sortingOrder}.");
#endif
                return;
            }

            // Otherwise, move to the end within its parent to render on top within that Canvas
            instance.transform.SetAsLastSibling();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"UIManager: '{instance.name}' moved to last sibling for top render order.");
#endif
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
                if (panelInstance != null)
                    Addressables.ReleaseInstance(panelInstance);
            }
            _addressToInstanceMap.Clear();
        }

        public async UniTask ShowPanel(string panelAddress)
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
                if (panelInstance.TryGetComponent<UIPanel>(out var panelComponent))
                {
                    BringToFront(panelInstance);
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

            // If not cached, verify key exists, then load and instantiate it.
            try
            {
                if (!await IsAddressableKeyValid(panelAddress, "load")) return;

                var parent = dynamicPanelsParent != null ? dynamicPanelsParent : transform;
                var handle = Addressables.InstantiateAsync(panelAddress, parent);
                await handle.ToUniTask();

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    var newPanelInstance = handle.Result;
                    _addressToInstanceMap[panelAddress] = newPanelInstance;

                    // Inject dependencies into the dynamically loaded panel
                    if (_resolver != null)
                    {
                        InjectRecursively(newPanelInstance);
                    }

                    // Ensure visibility by parenting under a Canvas if needed
                    EnsureUnderCanvas(newPanelInstance);

                    if (newPanelInstance.TryGetComponent<UIPanel>(out var panelComponent))
                    {
                        BringToFront(newPanelInstance);
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

        // Convenience helpers to avoid hardcoded keys
        public UniTask ShowPauseMenuPanel() => ShowPanel(AddressableKeys.UIPanels.PauseMenu);
        public UniTask ShowCombatUIPanel() => ShowPanel(AddressableKeys.UIPanels.CombatUI);
        public UniTask PreloadPauseMenu() => PreloadPanel(AddressableKeys.UIPanels.PauseMenu);
        public UniTask PreloadCombatUI() => PreloadPanel(AddressableKeys.UIPanels.CombatUI);

        private async UniTask<bool> IsAddressableKeyValid(string key, string context)
        {
            var locationsHandle = Addressables.LoadResourceLocationsAsync(key);
            await locationsHandle.ToUniTask();

            bool isValid = locationsHandle.Status == AsyncOperationStatus.Succeeded && locationsHandle.Result != null && locationsHandle.Result.Count > 0;
            Addressables.Release(locationsHandle);

            if (!isValid)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning($"UIManager: Addressables key '{key}' not found for {context}. Skipping.");
#endif
            }

            return isValid;
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
                if (panelInstance.TryGetComponent<UIPanel>(out var panel))
                {
                    panel.Hide();
                }
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

        public async UniTask SwitchToPanel(string panelAddress)
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
        public async UniTask PreloadPanel(string panelAddress)
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
                if (!await IsAddressableKeyValid(panelAddress, "preload")) return;

                var parent = dynamicPanelsParent != null ? dynamicPanelsParent : transform;
                var handle = Addressables.InstantiateAsync(panelAddress, parent);
                await handle.ToUniTask();

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    var instance = handle.Result;
                    _addressToInstanceMap[panelAddress] = instance;

                    // Inject dependencies into the preloaded panel
                    if (_resolver != null)
                    {
                        InjectRecursively(instance);
                    }

                    // Ensure visibility by parenting under a Canvas if needed
                    EnsureUnderCanvas(instance);

                    if (instance.TryGetComponent<UIPanel>(out var panel))
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
}

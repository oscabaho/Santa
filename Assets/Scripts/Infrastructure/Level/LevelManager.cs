using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Santa.Core;
using Santa.Core.Pooling;
using Santa.Infrastructure.Input;
using Unity.Cinemachine;
using UnityEngine;
using VContainer;

namespace Santa.Infrastructure.Level
{

    /// <summary>
    /// Manages the game's level progression, including visual transformation of areas by instantiating prefabs.
    /// </summary>
    public class LevelManager : MonoBehaviour, ILevelService
    {
        [Header("Level Data")]
        [Tooltip("List of all levels in the game.")]
        [SerializeField] private List<LevelData> levels;

        [Header("References")]
        [Tooltip("Optional parent transform for level visuals. If null, uses this GameObject's transform.")]
        [SerializeField] private Transform levelVisualsParent;
        [Tooltip("If true, combines level visuals into a single mesh for performance (Static Batching). Bypasses pooling.")]
        [SerializeField] private bool useStaticBatching = true;
        [Tooltip("Controller for the World Space Dissolve shader effect.")]
        [SerializeField] private EnvironmentDissolveController dissolveController;
        [Tooltip("Prefab for the camera used during liberation transition.")]
        [SerializeField] private CinemachineCamera liberationCameraPrefab;

        private int currentLevelIndex = -1;
        private readonly List<GameObject> _activeGentrifiedVisuals = new();
        private readonly List<GameObject> _activeLiberatedVisuals = new();
        private GameObject _gentrifiedContainer;
        private GameObject _liberatedContainer;
        private Santa.Core.Save.EnvironmentDecorState _decorState;
        private IPoolService _pool;
        private InputReader _inputReader;
        private CinemachineCamera _currentLiberationCamera;
        private readonly HashSet<LevelData> _prewarmedLevels = new();
        private CancellationTokenSource _levelChangeCancellation;

        [VContainer.Inject]
        public void Construct(IPoolService pool, InputReader inputReader)
        {
            _pool = pool;
            _inputReader = inputReader;
        }

        private void Start()
        {
            if (dissolveController == null)
            {
                dissolveController = FindFirstObjectByType<EnvironmentDissolveController>();
            }
            // Ensure shader starts clean
            dissolveController?.ResetShaders();
            // Locate EnvironmentDecorState to persist liberation changes (optional, uses scene search)
            _decorState = FindFirstObjectByType<Santa.Core.Save.EnvironmentDecorState>(FindObjectsInactive.Include);
            if (_decorState == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.Log("LevelManager: EnvironmentDecorState not found. Level liberation state won't persist across saves.");
#endif
            }
            if (levels != null && levels.Count > 0)
            {
                SetLevel(0).Forget();
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning("LevelManager: No levels assigned in the inspector.");
#endif
            }
        }

        /// <summary>
        /// Gets the data for the current level.
        /// </summary>
        public LevelData GetCurrentLevelData()
        {
            if (currentLevelIndex >= 0 && currentLevelIndex < levels.Count)
            {
                return levels[currentLevelIndex];
            }
            return null;
        }

        /// <summary>
        /// Called after winning combat to transform the level to its 'liberated' state.
        /// </summary>
        /// <summary>
        /// Called after winning combat to transform the level to its 'liberated' state.
        /// </summary>
        public void LiberateCurrentLevel()
        {
            LiberateCurrentLevelSequence().Forget();
        }

        private async UniTaskVoid LiberateCurrentLevelSequence()
        {
            LevelData currentLevel = GetCurrentLevelData();
            if (currentLevel == null) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"Liberating level: {currentLevel.levelName} (Sequence Started)");
#endif
            // 1. Block Input
            if (_inputReader != null)
            {
                _inputReader.DisableGameplayInput();
            }

            // 2. Setup Camera
            if (liberationCameraPrefab != null && currentLevel.transitionCenter != Vector3.zero)
            {
                // Create or move camera
                if (_currentLiberationCamera == null)
                {
                    _currentLiberationCamera = Instantiate(liberationCameraPrefab);
                }
                _currentLiberationCamera.transform.position = currentLevel.transitionCenter + new Vector3(0, 10, -10); // Offset defaults
                _currentLiberationCamera.transform.LookAt(currentLevel.transitionCenter);
                _currentLiberationCamera.Priority = 2000; // Override everything
                _currentLiberationCamera.gameObject.SetActive(true);
            }
            else
            {
                // Fallback if no specific camera logic: ensure we at least wait a bit
                Debug.LogWarning("LevelManager: Liberation Camera not assigned or transition center missing.");
            }

            // 3. Prepare Visuals
            // Both must be active for the dissolve blend
            if (useStaticBatching)
            {
                if (_gentrifiedContainer != null) _gentrifiedContainer.SetActive(true);
                if (_liberatedContainer != null) _liberatedContainer.SetActive(true);
            }
            else
            {
                // If not batching, enable liberated objects (gentrified are already active)
                foreach (var visual in _activeLiberatedVisuals)
                    if (visual) visual.SetActive(true);
            }

            // 4. Wait for focus
            await UniTask.Delay(1000);

            // 5. Animate Dissolve
            if (dissolveController != null)
            {
                await dissolveController.AnimateDissolveAsync(currentLevel.transitionCenter, currentLevel.transitionRadius, 4.0f);
            }
            else
            {
                // Instant fallback
                await UniTask.Delay(500);
            }

            // 6. Cleanup
            if (useStaticBatching)
            {
                if (_gentrifiedContainer != null) _gentrifiedContainer.SetActive(false);
            }
            else
            {
                foreach (var visual in _activeGentrifiedVisuals)
                    if (visual) visual.SetActive(false);
            }

            // 7. Restore Gameplay
            if (_currentLiberationCamera != null)
            {
                _currentLiberationCamera.gameObject.SetActive(false);
            }

            if (_inputReader != null)
            {
                _inputReader.EnableGameplayInput();
            }

            // Record liberation
            if (!string.IsNullOrEmpty(currentLevel.levelName) && _decorState != null)
            {
                _decorState.ApplyChange($"liberated:{currentLevel.levelName}");
            }
        }

        /// <summary>
        /// Called after the upgrade screen to progress to the next area.
        /// </summary>
        public void AdvanceToNextLevel()
        {
            int nextLevelIndex = currentLevelIndex + 1;
            if (levels != null && nextLevelIndex < levels.Count)
            {
                SetLevel(nextLevelIndex).Forget();
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.Log("LevelManager: All levels have been liberated! Game Over.");
#endif
                // TODO: Handle game completion logic
            }
        }

        private async UniTaskVoid SetLevel(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= levels.Count)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogError($"LevelManager: Invalid level index {levelIndex}.");
#endif
                return;
            }

            // Cancel any in-progress level change
            _levelChangeCancellation?.Cancel();
            _levelChangeCancellation?.Dispose();
            _levelChangeCancellation = new CancellationTokenSource();
            var ct = _levelChangeCancellation.Token;

            try
            {
                // Destroy all visuals from the previous level before setting up the new one.
                if (currentLevelIndex != -1)
                {
                    DestroyActiveVisuals();
                }

                currentLevelIndex = levelIndex;
                LevelData newLevel = levels[currentLevelIndex];

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.Log($"Setting up level: {newLevel.levelName}");
#endif

                // Prewarm and instantiate the initial 'gentrified' visuals for the new level asynchronously.
                // Only prewarm if NOT using static batching (batching modifies meshes, so we can't pool effectively)
                if (!useStaticBatching && _pool != null && !_prewarmedLevels.Contains(newLevel))
                {
                    await PrewarmLevelVisualsAsync(newLevel);
                    if (ct.IsCancellationRequested) return;
                    _prewarmedLevels.Add(newLevel);
                }
                await InstantiateLevelVisualsAsync(newLevel);
                if (ct.IsCancellationRequested) return;

                if (useStaticBatching)
                {
                    BatchLevelVisuals();
                }

                // Proactively prewarm the next level to hide spikes
                PrewarmNextLevelIfAny();
            }
            catch (System.OperationCanceledException)
            {
                // Level change was cancelled, cleanup already happened
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.Log("LevelManager: Level change cancelled.");
#endif
            }
            catch (System.Exception ex)
            {
                GameLog.LogError($"LevelManager.SetLevel: Exception during level change: {ex.Message}");
                GameLog.LogException(ex);
            }
        }

        private async UniTask InstantiateLevelVisualsAsync(LevelData levelData)
        {
            // Use the level-specific parent if available, otherwise fall back to the manager's parent or this transform.
            Transform parent = levelData.levelVisualsParent != null ? levelData.levelVisualsParent : (levelVisualsParent != null ? levelVisualsParent : transform);

            if (useStaticBatching)
            {
                _gentrifiedContainer = new GameObject($"Gentrified_Batch_{levelData.levelName}");
                _gentrifiedContainer.transform.SetParent(parent, false);

                _liberatedContainer = new GameObject($"Liberated_Batch_{levelData.levelName}");
                _liberatedContainer.transform.SetParent(parent, false);
                // Liberated elements start hidden, but we might need them active briefly for batching
                _liberatedContainer.SetActive(false);
            }

            int itemsProcessed = 0;
            const int batchSize = 5; // Process 5 items before yielding

            foreach (var prefab in levelData.gentrifiedVisuals)
            {
                if (prefab != null)
                {
                    GameObject instance;
                    if (useStaticBatching)
                    {
                        instance = Instantiate(prefab, _gentrifiedContainer.transform);
                    }
                    else
                    {
                        instance = _pool != null
                            ? _pool.Get(prefab.name, prefab, parent.position, parent.rotation, parent)
                            : Instantiate(prefab, parent);
                        _activeGentrifiedVisuals.Add(instance);
                    }

                    itemsProcessed++;

                    if (itemsProcessed % batchSize == 0)
                    {
                        await UniTask.Yield();
                    }
                }
            }
            foreach (var prefab in levelData.liberatedVisuals)
            {
                if (prefab != null)
                {
                    GameObject instance;
                    if (useStaticBatching)
                    {
                        instance = Instantiate(prefab, _liberatedContainer.transform);
                    }
                    else
                    {
                        instance = _pool != null
                            ? _pool.Get(prefab.name, prefab, parent.position, parent.rotation, parent)
                            : Instantiate(prefab, parent);
                        instance.SetActive(false);
                        _activeLiberatedVisuals.Add(instance);
                    }

                    itemsProcessed++;

                    if (itemsProcessed % batchSize == 0)
                    {
                        await UniTask.Yield();
                    }
                }
            }

            // Instantiate dynamic decorations
            foreach (var decor in levelData.dynamicDecors)
            {
                if (decor.prefab != null)
                {
                    Transform targetParent = parent.Find(decor.targetAreaName);
                    if (targetParent == null)
                    {
                        Debug.LogWarning($"DynamicDecor: Target area '{decor.targetAreaName}' not found under {parent.name}.");
                        itemsProcessed++;
                        if (itemsProcessed % batchSize == 0)
                        {
                            await UniTask.Yield();
                        }
                        continue;
                    }
                    GameObject instance;
                    if (useStaticBatching)
                    {
                        instance = Instantiate(decor.prefab, targetParent);
                    }
                    else
                    {
                        instance = _pool != null
                            ? _pool.Get(decor.prefab.name, decor.prefab, targetParent.position, targetParent.rotation, targetParent)
                            : Instantiate(decor.prefab, targetParent);
                        _activeGentrifiedVisuals.Add(instance);
                    }
                    itemsProcessed++;
                    if (itemsProcessed % batchSize == 0)
                    {
                        await UniTask.Yield();
                    }
                }
            }
        }

        private async UniTask PrewarmLevelVisualsAsync(LevelData levelData)
        {
            // Build counts per prefab across both visual lists (avoid LINQ allocations)
            var countMap = new Dictionary<GameObject, int>(32);
            void AddCount(GameObject prefab)
            {
                if (prefab == null) return;
                if (countMap.TryGetValue(prefab, out var c))
                {
                    countMap[prefab] = c + 1;
                }
                else
                {
                    countMap[prefab] = 1;
                }
            }

            for (int i = 0; i < levelData.gentrifiedVisuals.Count; i++) AddCount(levelData.gentrifiedVisuals[i]);
            for (int i = 0; i < levelData.liberatedVisuals.Count; i++) AddCount(levelData.liberatedVisuals[i]);
            for (int i = 0; i < levelData.dynamicDecors.Count; i++) AddCount(levelData.dynamicDecors[i].prefab);

            int processed = 0;
            foreach (var kv in countMap)
            {
                var prefab = kv.Key;
                int count = kv.Value;
                await _pool.PrewarmAsync(prefab.name, prefab, count);
                processed++;
                if ((processed & 3) == 3)
                {
                    await UniTask.Yield();
                }
            }
        }

        private void PrewarmNextLevelIfAny()
        {
            if (useStaticBatching) return; // Skip prewarming if we are not pooling
            if (_pool == null || levels == null) return;
            int next = currentLevelIndex + 1;
            if (next >= 0 && next < levels.Count)
            {
                var nextLevel = levels[next];
                if (!_prewarmedLevels.Contains(nextLevel))
                {
                    PrewarmLevelVisualsAsync(nextLevel).Forget();
                    _prewarmedLevels.Add(nextLevel);
                }
            }
        }

        private void DestroyActiveVisuals()
        {
            if (useStaticBatching)
            {
                if (_gentrifiedContainer != null) Destroy(_gentrifiedContainer);
                if (_liberatedContainer != null) Destroy(_liberatedContainer);
                return;
            }

            foreach (var visual in _activeGentrifiedVisuals)
            {
                if (visual != null)
                {
                    if (_pool != null)
                    {
                        _pool.Return(visual.name, visual);
                    }
                    else
                    {
                        Destroy(visual);
                    }
                }
            }
            _activeGentrifiedVisuals.Clear();

            foreach (var visual in _activeLiberatedVisuals)
            {
                if (visual != null)
                {
                    if (_pool != null)
                    {
                        _pool.Return(visual.name, visual);
                    }
                    else
                    {
                        Destroy(visual);
                    }
                }
            }
            _activeLiberatedVisuals.Clear();
        }

        private void BatchLevelVisuals()
        {
            if (_gentrifiedContainer != null)
            {
                StaticBatchingUtility.Combine(_gentrifiedContainer);
            }

            if (_liberatedContainer != null)
            {
                // The container must be active to be batched initially, 
                // though we want the Liberated visuals hidden to start with.
                bool wasActive = _liberatedContainer.activeSelf;
                if (!wasActive) _liberatedContainer.SetActive(true);

                StaticBatchingUtility.Combine(_liberatedContainer);

                if (!wasActive) _liberatedContainer.SetActive(false);
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log("LevelManager: Static Batching applied to level visuals.");
#endif
        }
    }
}

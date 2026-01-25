using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using Santa.Core;

namespace Santa.Infrastructure.Combat
{
    public class CombatEncounterManager : MonoBehaviour, ICombatEncounterManager
{
    private IGameStateService _gameStateService;
    private ICombatTransitionService _combatTransitionService;
    private CombatScenePool _combatScenePool;
    private ICombatService _combatService;

        // [Inject] removed to support safe runtime discovery
        // public void Construct(...) ...

        private void EnsureDependencies()
        {
            if (_gameStateService == null)
            {
               var manager = FindFirstObjectByType<Santa.Infrastructure.State.GameStateManager>();
               if (manager != null) _gameStateService = manager;
            }
            if (_combatTransitionService == null)
            {
                var trans = FindFirstObjectByType<CombatTransitionManager>();
                if (trans != null) _combatTransitionService = trans;
            }
        }

    public async UniTask<bool> StartEncounterAsync(CombatEncounter encounter)
    {
        if (encounter == null)
        {
            GameLog.LogError(Santa.Core.Config.LogMessages.CombatEncounter.NullEncounter);
            return false;
        }

        var poolKey = encounter.GetPoolKey();
        GameObject activeInstance = null;

        // Ensure we have dependencies before proceeding
        EnsureDependencies();

        if (_gameStateService == null || _combatTransitionService == null)
        {
             GameLog.LogError("CombatEncounterManager: Critical dependencies (GameStateService or CombatTransitionService) not found.");
             return false;
        }

        try
        {
            // 0. Ensure CombatScenePool is found (It lives in Gameplay scene)
            if (_combatScenePool == null)
            {
                _combatScenePool = FindFirstObjectByType<CombatScenePool>();
            }

            if (_combatScenePool == null)
            {
                 GameLog.LogError("CombatEncounterManager: CombatScenePool not found in scene. Cannot start encounter.");
                 return false;
            }

            // 1. Get Instance from Pool
            activeInstance = await _combatScenePool.GetInstanceAsync(poolKey, encounter);
            if (activeInstance == null)
            {
                GameLog.LogError(Santa.Core.Config.LogMessages.CombatEncounter.PoolInstanceFailed);
                return false;
            }

            activeInstance.SetActive(true);

            // 2. Validate Arena and Participants
            var combatArena = activeInstance.GetComponentInChildren<CombatArena>(true);
            if (combatArena == null)
            {
                GameLog.LogError(string.Format(Santa.Core.Config.LogMessages.CombatEncounter.NoArenaFound, activeInstance.name));
                ReleaseInstance(poolKey, activeInstance, encounter);
                return false;
            }

            List<GameObject> participants = combatArena.Combatants
                .Where(go => go != null)
                .Distinct()
                .ToList();

            if (participants.Count == 0)
            {
                GameLog.LogError(Santa.Core.Config.LogMessages.CombatEncounter.NoParticipants);
                ReleaseInstance(poolKey, activeInstance, encounter);
                return false;
            }

            // 3. Start Combat Services
            var tcs = new UniTaskCompletionSource<bool>();

            void OnCombatEnded(bool playerWon)
            {
                _gameStateService.OnCombatEnded -= OnCombatEnded;
                tcs.TrySetResult(playerWon);
            }

            _gameStateService.OnCombatEnded += OnCombatEnded;

            if (_combatService == null)
            {
                 // Find the Combat Service (Local in Gameplay)
                 var combatManager = FindFirstObjectByType<TurnBasedCombatManager>();
                 if (combatManager != null) _combatService = combatManager;
            }

            if (_combatService == null)
            {
                GameLog.LogError("CombatEncounterManager: CombatService not found (Local dependency missing in Global scope).");
                ReleaseInstance(poolKey, activeInstance, encounter);
                return false;
            }

            _combatTransitionService.StartCombat(activeInstance);
            _combatService.StartCombat(participants);
            _gameStateService.StartCombat();

            // 4. Wait for Result
            bool result = await tcs.Task;

            // 5. Cleanup
            ReleaseInstance(poolKey, activeInstance, encounter);
            return result;
        }
        catch (Exception ex)
        {
            GameLog.LogError(string.Format(Santa.Core.Config.LogMessages.CombatEncounter.EncounterException, ex));
            if (activeInstance != null)
            {
                ReleaseInstance(poolKey, activeInstance, encounter);
            }
            return false;
        }
    }

    private void ReleaseInstance(string key, GameObject instance, CombatEncounter encounter)
    {
        if (_combatScenePool != null && instance != null)
        {
            bool releaseAddressables = encounter != null && encounter.ReleaseAddressablesInstances;
            _combatScenePool.ReleaseInstance(key, instance, releaseAddressables);
        }
    }
}
}

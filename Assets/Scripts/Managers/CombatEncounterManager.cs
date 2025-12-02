using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class CombatEncounterManager : MonoBehaviour, ICombatEncounterManager
{
    private IGameStateService _gameStateService;
    private ICombatTransitionService _combatTransitionService;
    private CombatScenePool _combatScenePool;
    private ICombatService _combatService;

    [Inject]
    public void Construct(
        IGameStateService gameStateService,
        ICombatTransitionService combatTransitionService,
        CombatScenePool combatScenePool,
        ICombatService combatService)
    {
        _gameStateService = gameStateService;
        _combatTransitionService = combatTransitionService;
        _combatScenePool = combatScenePool;
        _combatService = combatService;
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

        try
        {
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

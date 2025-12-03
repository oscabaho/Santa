using System.Collections.Generic;
using UnityEngine;
using Santa.Core;
using Santa.Core.Config;
using Santa.Domain.Combat;
using Santa.Presentation.Combat;

namespace Santa.Infrastructure.Combat
{
    /// <summary>
    /// Manages the state of combat including participants, components, and synchronization.
    /// Separates state management from combat flow logic.
    /// </summary>
    public class CombatStateManager : ICombatStateManager
{
    public CombatState State { get; private set; } = new CombatState();
    public bool IsInitialized { get; private set; }

    private readonly List<EnemyTarget> _enemyTargets = new List<EnemyTarget>();

    public void Initialize(List<GameObject> participants, IUpgradeService upgradeService)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log("CombatStateManager: Initializing combat state...");
#endif

        // Initialize the core state
        State.Initialize(participants);
        IsInitialized = true;

        // Cache enemy targets for later use
        CacheEnemyTargets();

        // Sync player stats with upgrade service
        SyncPlayerStats(upgradeService);

        // Validate initialization
        ValidateInitialization();
    }

    private void CacheEnemyTargets()
    {
        _enemyTargets.Clear();
        
        foreach (var enemy in State.Enemies)
        {
            if (enemy == null) continue;

            if (enemy.TryGetComponent<EnemyTarget>(out var target))
            {
                _enemyTargets.Add(target);
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.LogVerbose($"CombatStateManager: Cached {_enemyTargets.Count} enemy targets.");
#endif
    }

    public void SyncPlayerStats(IUpgradeService upgradeService)
    {
        if (upgradeService == null || State.Player == null)
        {
            return;
        }

        // Sync Action Points
        if (State.APComponents.TryGetValue(State.Player, out var playerAP))
        {
            int maxAP = upgradeService.MaxActionPoints;
            playerAP.SetMaxValue(maxAP);
            playerAP.SetValue(maxAP); // Start combat with full AP
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogVerbose($"CombatStateManager: Synced Player AP. MaxAP = {maxAP}");
#endif
        }
        else
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning("CombatStateManager: Player has no ActionPointComponent to sync.");
#endif
        }

        // Sync Health Points
        if (State.HealthComponents.TryGetValue(State.Player, out var playerHealth))
        {
            int maxHealth = upgradeService.MaxHealth;
            playerHealth.SetMaxValue(maxHealth);
            playerHealth.SetValue(maxHealth); // Heal to full at start of combat
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogVerbose($"CombatStateManager: Synced Player Health. MaxHealth = {maxHealth}");
#endif
        }
        else
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning("CombatStateManager: Player has no HealthComponent to sync.");
#endif
        }
    }

    public void Clear()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log("CombatStateManager: Clearing combat state.");
#endif

        State.Clear();
        _enemyTargets.Clear();
        IsInitialized = false;
    }

    public IReadOnlyList<EnemyTarget> GetEnemyTargets()
    {
        return _enemyTargets;
    }

    private void ValidateInitialization()
    {
        if (State.Player == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError($"CombatStateManager: Player is null after initialization! No participant with tag '{GameConstants.Tags.Player}' was found.");
#endif
            return;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.LogVerbose($"CombatStateManager: Initialized successfully. Player: {State.Player.name}, Enemies: {State.Enemies.Count}");
#endif
    }
}
}

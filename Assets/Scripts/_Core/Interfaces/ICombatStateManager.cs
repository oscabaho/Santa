using System.Collections.Generic;
using UnityEngine;
using Santa.Domain.Combat;
using Santa.Presentation.Combat;

namespace Santa.Core
{
    /// <summary>
    /// Interface for managing the state of combat, including participants, components, and synchronization.
    /// </summary>
    public interface ICombatStateManager
{
    /// <summary>
    /// The current combat state containing all participants and their components.
    /// </summary>
    CombatState State { get; }

    /// <summary>
    /// Whether combat has been initialized.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Initializes the combat state with the given participants and syncs player stats.
    /// </summary>
    /// <param name="participants">List of GameObjects participating in combat</param>
    /// <param name="upgradeService">Optional upgrade service for player stat synchronization</param>
    void Initialize(List<GameObject> participants, IUpgradeService upgradeService);

    /// <summary>
    /// Clears all combat state and resets initialization flag.
    /// </summary>
    void Clear();

    /// <summary>
    /// Synchronizes player stats (HP, AP) with the upgrade service.
    /// </summary>
    /// <param name="upgradeService">Upgrade service containing player stat values</param>
    void SyncPlayerStats(IUpgradeService upgradeService);

    /// <summary>
    /// Gets the cached list of enemy target components.
    /// </summary>
    /// <returns>Read-only list of EnemyTarget components</returns>
    IReadOnlyList<EnemyTarget> GetEnemyTargets();
    }
}

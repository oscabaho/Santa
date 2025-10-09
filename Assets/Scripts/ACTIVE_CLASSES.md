ACTIVE CLASSES (Assets/Scripts)

This file lists active, public classes and interfaces found under `Assets/Scripts` with a short description and file path. Use this as a quick reference for navigating core gameplay and engine classes.

Machine-readable: `ACTIVE_CLASSES.json` lives alongside this file for tool consumption: `Assets/Scripts/ACTIVE_CLASSES.json`

Format: ClassName — brief description (file path)

Core services and utilities

- [GameLog](Core/GameLog.cs) — Lightweight runtime logging wrapper; enabled in Editor/Development builds or when `GAME_LOGS_ENABLED` is defined. (Assets/Scripts/Core/GameLog.cs)
- [ServiceLocator](Core/ServiceLocator.cs) — Static service locator for registering and fetching global services (Assets/Scripts/Core/ServiceLocator.cs)
- [GameEventBus](Core/GameEventBus.cs) — Event bus implementation for decoupled game events (Assets/Scripts/Core/GameEventBus.cs)

Game state & flow

- [GameStateManager](Managers/GameStateManager.cs) — Manages global game states (Exploration, Combat) and publishes OnCombatStarted/OnCombatEnded events. (Assets/Scripts/Managers/GameStateManager.cs)
- [TurnBasedCombatManager](Managers/TurnBasedCombatManager.cs) — Coordinates the turn-based combat flow (planning → executing) and other combat services. (Assets/Scripts/Managers/TurnBasedCombatManager.cs)
- [CombatState](Core/Combat/CombatState.cs) — Holds combat state data and lists of combatants. (Assets/Scripts/Core/Combat/CombatState.cs)

Combat & transitions

- [CombatTransitionManager](Core/CombatTransitionManager.cs) — Handles visual transitions between exploration and combat, using TransitionSequence tasks. (Assets/Scripts/Core/CombatTransitionManager.cs)
- [CombatScenePool](Core/CombatScenePool.cs) — Pool for combat scene prefabs with Addressables support and fallback to Resources/prefab. (Assets/Scripts/Core/CombatScenePool.cs)
- [CombatEncounter](Gameplay/CombatEncounter.cs) — Component used to configure a specific combat encounter (prefab/address/resources, prewarm options). (Assets/Scripts/Gameplay/CombatEncounter.cs)
- [CombatTrigger](Gameplay/CombatTrigger.cs) — Component that starts a combat encounter when the player interacts with it. (Assets/Scripts/Gameplay/CombatTrigger.cs)

UI & gameplay

- [CombatUI](UI/CombatUI.cs) — Manages combat UI: player stats, action buttons, and inputs. (Assets/Scripts/UI/CombatUI.cs)
- [UIManager](Managers/UIManager.cs) — Top-level UI manager for showing/hiding panels and menus. (Assets/Scripts/Managers/UIManager.cs)
- [GameplayUIManager](Managers/GameplayUIManager.cs) — Manages in-game HUD/overlays and UI transitions. (Assets/Scripts/Managers/GameplayUIManager.cs)

Abilities & components

- Ability (abstract) — Base class for ability ScriptableObjects. (Assets/Scripts/Core/Ability.cs)
- DamageAbility, GainAPAbility, SpecialAttackAbility — Example ability implementations. (Assets/Scripts/Abilities/*)
- ActionExecutor — Executes queued player/enemy actions during combat. (Assets/Scripts/Gameplay/ActionExecutor.cs)
- AbilityHolder — Component to attach to characters that can use abilities. (Assets/Scripts/Components/AbilityHolder.cs)
- HealthComponent, EnergyComponent, ActionPointComponent — Common stat components used by characters. (Assets/Scripts/Components/*)

AI & characters

- AIManager / IAIManager — Manages enemy decision-making. (Assets/Scripts/Gameplay/AIManager.cs)
- EnemyBrain / AllyBrain — AI brain components for NPC behavior. (Assets/Scripts/Enemies/*, Assets/Scripts/Allies/*)

VFX & Audio

- VFXManager — Manages pooled VFX and VFX events. (Assets/Scripts/VFX/VFXManager.cs)
- PooledParticleSystem — Particle system wrapper for pooling. (Assets/Scripts/VFX/PooledParticleSystem.cs)
- AudioManager / PooledAudioSource — Audio playback service and pooling. (Assets/Scripts/Audio/*)

Systems & tools

- CoreSystemsInitializer — Bootstraps essential core systems at scene load. (Assets/Scripts/Managers/CoreSystemsInitializer.cs)
- SceneBootstrapper — Scene-scoped initialization helper. (Assets/Scripts/Core/SceneBootstrapper.cs)

Interfaces (selection)

- IGameStateService — Interface for game state management (Assets/Scripts/Core/Interfaces/IGameStateService.cs)
- ICombatTransitionService — Interface for transition manager (Assets/Scripts/Core/Interfaces/ICombatTransitionService.cs)
- ICombatService / IUIManager / IAudioService / IVFXService — Core service interfaces (Assets/Scripts/Core/Interfaces/*)

Notes & maintenance

- This list is generated from active public class/interface declarations found under `Assets/Scripts` and includes a subset of representative files. It intentionally omits sample folders and editor-only utilities.
- If you want a full, automated list including private/internal classes, or a CSV/JSON export, I can generate that next.

Generated: 2025-10-08

JSON: `ACTIVE_CLASSES.json` provides a machine-readable index for tooling and other AIs.

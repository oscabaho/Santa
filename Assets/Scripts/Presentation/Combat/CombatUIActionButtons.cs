using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Santa.Domain.Combat;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.UI;

namespace Santa.Presentation.Combat
{

    /// <summary>
    /// Manages combat action buttons and ability loading.
    /// Handles button interactability based on AP cost and ability availability.
    /// </summary>
    public class CombatUIActionButtons : MonoBehaviour
    {
        // Abilities loaded dynamically via Addressables
        private Ability _directAttackAbility;
        private Ability _areaAttackAbility;
        private Ability _specialAttackAbility;
        private Ability _meditateAbility;

        private bool _abilitiesLoaded = false;
        private int _lastKnownAP = 0; // Store AP to refresh state after loading

        // Button references
        private Button _directAttackButton;
        private Button _areaAttackButton;
        private Button _specialAttackButton;
        private Button _meditateButton;
        private List<Button> _actionButtons;

        /// <summary>
        /// Event fired when a player requests to use an ability.
        /// Parameters: (Ability ability, GameObject primaryTarget)
        /// Primary target may be null for non-targeted abilities.
        /// </summary>
        public event Action<Ability, GameObject> OnAbilityRequested;

        /// <summary>
        /// Returns true if abilities have been successfully loaded from Addressables.
        /// </summary>
        public bool AreAbilitiesLoaded => _abilitiesLoaded;

        /// <summary>
        /// Initializes the action buttons component with button references.
        /// </summary>
        public void Initialize(Button directAttack, Button areaAttack, Button specialAttack, Button meditate)
        {
            _directAttackButton = directAttack;
            _areaAttackButton = areaAttack;
            _specialAttackButton = specialAttack;
            _meditateButton = meditate;

            // Start loading abilities
            _ = LoadAbilitiesAsync();
        }

        /// <summary>
        /// Loads all combat abilities from Addressables asynchronously.
        /// </summary>
        public async UniTask LoadAbilitiesAsync()
        {
            try
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose("CombatUIActionButtons: Loading abilities via Addressables...");
#endif

                var directTask = Addressables.LoadAssetAsync<Ability>(Santa.Core.Addressables.AddressableKeys.Abilities.Direct).ToUniTask();
                var areaTask = Addressables.LoadAssetAsync<Ability>(Santa.Core.Addressables.AddressableKeys.Abilities.Area).ToUniTask();
                var specialTask = Addressables.LoadAssetAsync<Ability>(Santa.Core.Addressables.AddressableKeys.Abilities.Special).ToUniTask();
                var meditateTask = Addressables.LoadAssetAsync<Ability>(Santa.Core.Addressables.AddressableKeys.Abilities.GainAP).ToUniTask();

                var (direct, area, special, meditate) = await UniTask.WhenAll(directTask, areaTask, specialTask, meditateTask);

                _directAttackAbility = direct;
                _areaAttackAbility = area;
                _specialAttackAbility = special;
                _meditateAbility = meditate;

                _abilitiesLoaded = true;
                SetupButtonListeners();

                // Refresh state immediately with last known AP
                RefreshButtonInteractability(_lastKnownAP);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose("CombatUIActionButtons: Abilities loaded successfully.");
#endif
            }
            catch (OperationException ex)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogError($"CombatUIActionButtons: Failed to load abilities via Addressables. Operation failed: {ex.Message}");
#endif
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogError($"CombatUIActionButtons: Unexpected error while loading abilities. {ex.Message}");
#endif
            }
        }

        /// <summary>
        /// Sets all action buttons to be interactable or not.
        /// </summary>
        public void SetButtonsInteractable(bool interactable)
        {
            if (_actionButtons == null) return;

            foreach (var button in _actionButtons)
            {
                if (button != null)
                {
                    button.interactable = interactable;
                }
            }
        }

        /// <summary>
        /// Refreshes button interactability based on current player AP.
        /// Buttons are disabled if player doesn't have enough AP for the ability.
        /// </summary>
        public void RefreshButtonInteractability(int currentAP)
        {
            _lastKnownAP = currentAP; // Cache for late-loading abilities

            if (!_abilitiesLoaded) return;

            SetButtonState(_directAttackButton, _directAttackAbility, currentAP);
            SetButtonState(_areaAttackButton, _areaAttackAbility, currentAP);
            SetButtonState(_specialAttackButton, _specialAttackAbility, currentAP);
            SetButtonState(_meditateButton, _meditateAbility, currentAP);
        }

        private void SetupButtonListeners()
        {
            if (!_abilitiesLoaded)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning("CombatUIActionButtons: Abilities not loaded yet, deferring button setup.");
#endif
                return;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_directAttackButton == null) GameLog.LogWarning("Direct Attack Button is NULL");
            if (_areaAttackButton == null) GameLog.LogWarning("Area Attack Button is NULL");
            if (_specialAttackButton == null) GameLog.LogWarning("Special Attack Button is NULL");
            if (_meditateButton == null) GameLog.LogWarning("Meditate Button is NULL");
#endif

            _actionButtons = new List<Button>();

            AddButtonListener(_directAttackButton, _directAttackAbility);
            AddButtonListener(_areaAttackButton, _areaAttackAbility);
            AddButtonListener(_specialAttackButton, _specialAttackAbility);
            AddButtonListener(_meditateButton, _meditateAbility);
        }

        private void AddButtonListener(Button button, Ability ability)
        {
            if (button != null)
            {
                button.onClick.AddListener(() => RequestAbility(ability));
                _actionButtons.Add(button);
            }
        }

        private void RequestAbility(Ability ability)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogVerbose($"CombatUIActionButtons: Ability requested - {(ability != null ? ability.AbilityName : "NULL")}");
#endif
            if (ability == null) return;

            // Notify listeners (CombatUI will handle the request)
            OnAbilityRequested?.Invoke(ability, null);
        }

        private void SetButtonState(Button button, Ability ability, int currentAP)
        {
            if (button != null && ability != null)
            {
                button.interactable = currentAP >= ability.ApCost;
            }
        }

        private void OnDestroy()
        {
            // Clear button listeners to prevent memory leaks
            if (_actionButtons != null)
            {
                foreach (var button in _actionButtons)
                {
                    if (button != null)
                    {
                        button.onClick.RemoveAllListeners();
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Santa.Core;
using Santa.Core.Config;
using Santa.Infrastructure.Combat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Santa.Presentation.Combat
{

/// <summary>
/// Manages the display of player and enemy stats in the combat UI.
/// Subscribes to health and AP changes and updates UI sliders accordingly.
/// </summary>
public class CombatUIStatsDisplay : MonoBehaviour
{
    // UI References
    private Slider _playerHealthSlider;
    private TextMeshProUGUI _playerAPText;
    private Slider _leftEnemyHealthSlider;
    private Slider _centerEnemyHealthSlider;
    private Slider _rightEnemyHealthSlider;

    // Player stat controllers
    private IHealthController _playerHealth;
    private IActionPointController _playerAP;

    // Enemy health data with cached position information
    private struct EnemyHealthData
    {
        public IHealthController Health;
        public Action<int, int> Callback;
        public CombatPosition Position;
    }
    private readonly Dictionary<GameObject, EnemyHealthData> _enemyHealthData = new Dictionary<GameObject, EnemyHealthData>();

    /// <summary>
    /// Gets the current player AP value, or 0 if player AP is not subscribed.
    /// </summary>
    public int CurrentPlayerAP => _playerAP?.CurrentValue ?? 0;

    /// <summary>
    /// Initializes the stats display with references to UI elements.
    /// </summary>
    public void Initialize(
        Slider playerHealthSlider,
        TextMeshProUGUI playerAPText,
        Slider leftEnemyHealthSlider,
        Slider centerEnemyHealthSlider,
        Slider rightEnemyHealthSlider)
    {
        _playerHealthSlider = playerHealthSlider;
        _playerAPText = playerAPText;
        _leftEnemyHealthSlider = leftEnemyHealthSlider;
        _centerEnemyHealthSlider = centerEnemyHealthSlider;
        _rightEnemyHealthSlider = rightEnemyHealthSlider;
    }

    /// <summary>
    /// Subscribes to player health and AP changes.
    /// </summary>
    public void SubscribeToPlayer(IHealthController health, IActionPointController ap)
    {
        // Unsubscribe first to avoid double subscription
        UnsubscribeFromPlayer();

        _playerHealth = health;
        _playerAP = ap;

        if (_playerHealth != null)
        {
            _playerHealth.OnValueChanged += UpdatePlayerHealthUI;
            UpdatePlayerHealthUI(_playerHealth.CurrentValue, _playerHealth.MaxValue);
        }

        if (_playerAP != null)
        {
            _playerAP.OnValueChanged += UpdatePlayerAPUI;
            UpdatePlayerAPUI(_playerAP.CurrentValue, _playerAP.MaxValue);
        }
    }

    /// <summary>
    /// Unsubscribes from player health and AP changes.
    /// </summary>
    public void UnsubscribeFromPlayer()
    {
        if (_playerHealth != null)
        {
            _playerHealth.OnValueChanged -= UpdatePlayerHealthUI;
        }
        if (_playerAP != null)
        {
            _playerAP.OnValueChanged -= UpdatePlayerAPUI;
        }

        _playerHealth = null;
        _playerAP = null;
    }

    /// <summary>
    /// Subscribes to enemy health changes. Uses position identifier to map enemies to health bars.
    /// </summary>
    public void SubscribeToEnemies(IReadOnlyList<GameObject> enemies)
    {
        if (enemies == null || enemies.Count == 0 || _enemyHealthData.Count > 0)
        {
            return; // Already subscribed or no enemies
        }

        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;

            var registry = enemy.GetComponent<IComponentRegistry>();
            if (registry != null && registry.HealthController != null)
            {
                var health = registry.HealthController;
                Action<int, int> callback = null;
                CombatPosition position = CombatPosition.Center; // Default

                // Cache position from component
                var posId = enemy.GetComponent<CombatPositionIdentifier>();
                if (posId != null)
                {
                    position = posId.Position;
                    callback = GetCallbackForPosition(position);
                }
                else
                {
                    // Fallback to name-based identification (Legacy)
                    position = GetPositionFromName(enemy.name);
                    callback = GetCallbackForPosition(position);
                }

                if (callback != null)
                {
                    health.OnValueChanged += callback;

                    // Cache all data together
                    _enemyHealthData[enemy] = new EnemyHealthData
                    {
                        Health = health,
                        Callback = callback,
                        Position = position
                    };

                    // Initial update
                    callback(health.CurrentValue, health.MaxValue);
                }
            }
        }
    }

    /// <summary>
    /// Unsubscribes from all enemy health changes.
    /// </summary>
    public void UnsubscribeFromEnemies()
    {
        foreach (var kvp in _enemyHealthData)
        {
            if (kvp.Value.Health != null)
            {
                kvp.Value.Health.OnValueChanged -= kvp.Value.Callback;
            }
        }
        _enemyHealthData.Clear();
    }

    private void UpdatePlayerHealthUI(int current, int max)
    {
        if (_playerHealthSlider != null && max > 0)
        {
            _playerHealthSlider.value = (float)current / max;
        }
    }

    private void UpdatePlayerAPUI(int current, int max)
    {
        if (_playerAPText != null)
        {
            _playerAPText.text = string.Format(Santa.Core.Config.UIStrings.ActionPointsLabelFormat, current);
        }
    }

    private void UpdateSlider(Slider slider, int current, int max)
    {
        if (slider != null && max > 0)
        {
            slider.value = (float)current / max;
        }
    }

    private Action<int, int> GetCallbackForPosition(CombatPosition position)
    {
        switch (position)
        {
            case CombatPosition.Right:
                return (curr, max) => UpdateSlider(_rightEnemyHealthSlider, curr, max);
            case CombatPosition.Left:
                return (curr, max) => UpdateSlider(_leftEnemyHealthSlider, curr, max);
            case CombatPosition.Center:
                return (curr, max) => UpdateSlider(_centerEnemyHealthSlider, curr, max);
            default:
                return null;
        }
    }

    private CombatPosition GetPositionFromName(string enemyName)
    {
        if (enemyName.Contains("Right", StringComparison.OrdinalIgnoreCase))
            return CombatPosition.Right;
        if (enemyName.Contains("Left", StringComparison.OrdinalIgnoreCase))
            return CombatPosition.Left;
        if (enemyName.Contains("Central", StringComparison.OrdinalIgnoreCase))
            return CombatPosition.Center;

        return CombatPosition.Center; // Default
    }

    private void OnDestroy()
    {
        UnsubscribeFromPlayer();
        UnsubscribeFromEnemies();
    }
}
}

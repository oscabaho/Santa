using UnityEngine;
using VContainer;
using Santa.Presentation.Upgrades;

namespace Santa.Infrastructure.Combat
{
    /// <summary>
    /// Initializes the player's health based on the UpgradeManager's MaxHealth stat.
    /// Should be attached to the Player GameObject.
    /// </summary>
    [RequireComponent(typeof(HealthComponentBehaviour))]
    public class PlayerHealthInitializer : MonoBehaviour
{
    private UpgradeManager _upgradeManager;
    private HealthComponentBehaviour _healthComponent;

    [Inject]
    public void Construct(UpgradeManager upgradeManager)
    {
        _upgradeManager = upgradeManager;
    }

    private void Awake()
    {
        _healthComponent = GetComponent<HealthComponentBehaviour>();
        if (_healthComponent == null)
        {
            GameLog.LogError("PlayerHealthInitializer: Missing HealthComponentBehaviour on Player!", this);
        }
    }

    private void Start()
    {
        if (_upgradeManager != null && _healthComponent != null)
        {
            int maxHealth = _upgradeManager.MaxHealth;
            GameLog.Log($"PlayerHealthInitializer: Setting Max Health to {maxHealth} from UpgradeManager.", this);

            _healthComponent.SetMaxValue(maxHealth);
            _healthComponent.SetToMax();
        }
        else
        {
            if (_upgradeManager == null)
                GameLog.LogError("PlayerHealthInitializer: UpgradeManager not injected!", this);
        }
    }
}
}

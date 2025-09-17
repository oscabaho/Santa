using UnityEngine;

/// <summary>
/// ScriptableObject base para armas. Define sus propiedades y comportamiento al ser equipado.
/// </summary>
[CreateAssetMenu(fileName = "WeaponItem", menuName = "Inventory/WeaponItem")]
public class WeaponItem : MysteryItem, IEquipable
{
    [Header("Datos del arma")]
    [SerializeField] private int weaponDamage = 10;
    [SerializeField, Range(0f, 1f)] private float criticalHitChance = 0.05f;
    [SerializeField] private float criticalHitMultiplier = 2f;
    [SerializeField] private int staminaCost = 10;
    [SerializeField] private float attackSpeed = 1f;
    [SerializeField] private float attackDuration = 0.5f;
    [SerializeField] private float maxDurability = 100f;
    [SerializeField] private AnimationCurve durabilityCurve = null;
    [SerializeField] private AnimationCurve masteryCurve = null;
    [SerializeField] private int maxMasteryHits = 100;
    
    [Header("Prefab de hitbox de ataque")]
    [SerializeField] private GameObject hitBoxPrefab;
    [Header("Prefab visual del arma")]
    [SerializeField] private GameObject weaponPrefab;

    [Header("Efectos de Ataque")]
    [Tooltip("La clave del VFX del pool que se reproducirá al atacar (ej. el swing del arma).")]
    [SerializeField] private string attackVfxKey;

    [Header("Efectos de Impacto")]
    [SerializeField] private AudioData impactSoundData;
    [SerializeField] private string impactVFXKey = "ImpactEffect";

    public int WeaponDamage => weaponDamage;
    public int StaminaCost => staminaCost;
    public float AttackSpeed => attackSpeed;
    public float AttackDuration => attackDuration;
    public float MaxDurability => maxDurability;
    public AnimationCurve DurabilityCurve => durabilityCurve;
    public AnimationCurve MasteryCurve => masteryCurve;
    public int MaxMasteryHits => maxMasteryHits;
    public GameObject HitBoxPrefab => hitBoxPrefab;
    public GameObject WeaponPrefab => weaponPrefab;
    public AudioData ImpactSoundData => impactSoundData;
    public string ImpactVFXKey => impactVFXKey;
    public string AttackVfxKey => attackVfxKey;

    /// <summary>
    /// Aplica daño al objetivo usando la lógica del arma.
    /// </summary>
    public virtual void ApplyDamage(GameObject owner, GameObject target)
    {
        var damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            int finalDamage = weaponDamage;
            bool isCritical = Random.value < criticalHitChance;

            if (isCritical)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * criticalHitMultiplier);
            }

            damageable.TakeDamage(finalDamage);
            #if UNITY_EDITOR
            string logMessage = isCritical ? "¡GOLPE CRÍTICO! " : "";
            logMessage += $"{owner.name} infligió {finalDamage} de daño a {target.name} con {name}.";
            Debug.Log(logMessage);
            #endif
        }
    }

    // Implementación de IEquipable
    public EquipmentSlotType GetSlotType() => EquipmentSlotType.Weapon;
    public virtual void OnEquip(GameObject user)
    {
        #if UNITY_EDITOR
        Debug.Log($"{DisplayName} equipada en el jugador.");
        #endif
    }
    public virtual void OnUnequip(GameObject user)
    {
        #if UNITY_EDITOR
        Debug.Log($"{DisplayName} fue desequipada.");
        #endif
    }
    public string GetId() => Id;
}

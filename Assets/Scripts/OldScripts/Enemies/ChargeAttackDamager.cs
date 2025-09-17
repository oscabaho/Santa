using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ChargeAttackDamager : MonoBehaviour
{
    private int damage;
    private bool canDamage = false;
    private Collider _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _collider.isTrigger = true; // Ensure it's a trigger
        _collider.enabled = false;  // Start disabled
    }

    public void StartCharge(int chargeDamage)
    {
        damage = chargeDamage;
        canDamage = true;
        _collider.enabled = true;
    }

    public void EndCharge()
    {
        canDamage = false;
        _collider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canDamage || !other.CompareTag("Player"))
            return;

        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage);
            // To avoid dealing damage multiple times in one charge
            canDamage = false;
        }
    }
}

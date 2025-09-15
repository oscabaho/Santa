using ProyectSecret.Inventory.Items;
using System; // Necesario para Action

namespace ProyectSecret.Inventory
{
    /// <summary>
    /// Representa la instancia de un arma equipada, con estado propio (durabilidad, golpes, etc).
    /// </summary>
    public class WeaponInstance
    {
        public event Action OnStateChanged;

        public WeaponItem WeaponData { get; private set; }
        public float CurrentDurability { get; private set; }
        public int Hits { get; private set; }

        public void SetDurability(float value)
        {
            CurrentDurability = value;
            OnStateChanged?.Invoke();
        }

        public void DecreaseDurability(float amount)
        {
            if (WeaponData == null) return;
            CurrentDurability -= amount;
            if (CurrentDurability < 0) CurrentDurability = 0;
            OnStateChanged?.Invoke();
        }

        public void SetHits(int value)
        {
            Hits = value;
            OnStateChanged?.Invoke();
        }

        public void AddHit()
        {
            Hits++;
            OnStateChanged?.Invoke();
        }

        public WeaponInstance(WeaponItem weaponData)
        {
            WeaponData = weaponData;
            CurrentDurability = weaponData.MaxDurability;
            Hits = 0;
        }
    }
}

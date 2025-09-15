using System;
using UnityEngine;
using ProyectSecret.Interfaces;

namespace ProyectSecret.Stats
{
    /// <summary>
    /// Componente base para estadísticas (vida, stamina, etc). Solo serializable si es necesario.
    /// </summary>
    [Serializable]
    public abstract class StatComponent : IStatController
    {
        /// <summary>
        /// Establece el valor actual al máximo.
        /// </summary>
        public void SetToMax()
        {
            currentValue = maxValue;
            _onValueChanged?.Invoke(this);
        }
        [field: NonSerialized]
        private event Action<StatComponent> _onValueChanged;
        [SerializeField, Tooltip("Valor máximo de la estadística")] private int maxValue = 100;
        [SerializeField, HideInInspector] private int currentValue;

        public int MaxValue { get { return maxValue; } }
        public int CurrentValue { get { return currentValue; } }

        // Exponer el evento solo para suscripción, no para asignación externa
        public event Action<StatComponent> OnValueChanged
        {
            add { _onValueChanged += value; }
            remove { _onValueChanged -= value; }
        }

        public virtual void Awake()
        {
            // Si currentValue es 0 al iniciar, iguala a maxValue
            if (currentValue <= 0)
                currentValue = maxValue;
        }

        public virtual void AffectValue(int value)
        {
            SetValue(currentValue + value);
        }

        /// <summary>
        /// Establece el valor de la estadística a un número específico, asegurándose de que esté dentro de los límites.
        /// </summary>
        public virtual void SetValue(int newValue)
        {
            currentValue = Mathf.Clamp(newValue, 0, maxValue);
            _onValueChanged?.Invoke(this);
        }

        /// <summary>
        /// Modifica el valor máximo de la estadística y ajusta el valor actual si es necesario.
        /// </summary>
        /// <param name="amount">La cantidad a añadir (puede ser negativa).</param>
        public virtual void ModifyMaxValue(int amount)
        {
            maxValue += amount;
            // Asegura que el valor actual no supere el nuevo máximo.
            currentValue = Mathf.Clamp(currentValue, 0, maxValue);
            _onValueChanged?.Invoke(this);
        }
    }
}

using System;
using ProyectSecret.Stats;

namespace ProyectSecret.Components
{
    /// <summary>
    /// Componente concreto para la stamina, hereda de StatComponent.
    /// </summary>
    [Serializable]
    public class StaminaComponent : StatComponent
    {
        /// <summary>
        /// Valor actual de stamina (alias de CurrentValue).
        /// </summary>
        public int CurrentStamina { get { return CurrentValue; } }

        /// <summary>
        /// Consume stamina llamando a AffectValue con valor negativo.
        /// </summary>
        public void UseStamina(int amount)
        {
            AffectValue(-amount);
        }

        /// <summary>
        /// Comprueba si hay suficiente stamina para una acción.
        /// </summary>
        public bool HasEnough(int amount)
        {
            return CurrentValue >= amount;
        }
    }
}

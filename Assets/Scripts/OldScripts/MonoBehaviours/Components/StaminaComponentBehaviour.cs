using UnityEngine;

namespace ProyectSecret.Components
{
    /// <summary>
    /// Componente MonoBehaviour para exponer y gestionar StaminaComponent en un GameObject.
    /// </summary>
    [DisallowMultipleComponent]
    public class StaminaComponentBehaviour : MonoBehaviour, ProyectSecret.Interfaces.IStatController
    {
        [SerializeField]
        private StaminaComponent stamina = new StaminaComponent();
        public StaminaComponent Stamina { get { return stamina; } }

        public int CurrentValue => stamina.CurrentValue;
        public int MaxValue => stamina.MaxValue;
        public void AffectValue(int value) => stamina.AffectValue(value);

        /// <summary>
        /// Establece la stamina al máximo.
        /// </summary>
        public void SetToMax()
        {
            stamina.SetToMax();
        }

        private void Awake()
        {
            // Inicialización si es necesario
        }
    }
}

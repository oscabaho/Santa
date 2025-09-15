using UnityEngine;
using ProyectSecret.Managers;

namespace ProyectSecret.UI
{
    /// <summary>
    /// Componente reutilizable que gestiona automáticamente el foco del input
    /// cuando un panel de UI se activa o desactiva.
    /// Simplemente añade este componente al GameObject raíz de cualquier panel
    /// que deba capturar el input (menú de pausa, inventario, diálogo, etc.).
    /// </summary>
    public class UIInputFocusHandler : MonoBehaviour
    {
        [Tooltip("Si es verdadero, el juego se pausará (Time.timeScale = 0) cuando este UI esté activo.")]
        [SerializeField] private bool pauseGame = true;

        // Usamos un contador estático para manejar correctamente menús anidados que pausan el juego.
        private static int activeUIPausingCount = 0;

        private void OnEnable()
        {
            // Siempre que se active un panel con este script, cambiamos al mapa de UI.
            UIInputController.EnableUIMap();
            
            if (pauseGame)
            {
                if (activeUIPausingCount == 0)
                {
                    Time.timeScale = 0f;
                }
                activeUIPausingCount++;
            }
        }

        private void OnDisable()
        {
            if (pauseGame)
            {
                activeUIPausingCount--;
                if (activeUIPausingCount <= 0)
                {
                    Time.timeScale = 1f;
                    activeUIPausingCount = 0; // Resetea en caso de que baje de 0 por error.
                }
            }
            
            // Importante: Volver al mapa de gameplay solo si no queda ningún otro menú que pause el juego activo.
            if (activeUIPausingCount == 0)
            {
                UIInputController.EnableGameplayMap();
            }
        }
    }
}
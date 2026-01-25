using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // Función para cargar la escena del juego
    public void IniciarJuego()
    {
        SceneManager.LoadScene("Gameplay");
    }

    public void IrASalida()
    {
        SceneManager.LoadScene("Menu");
    }

    public void SalirDelJuego()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}

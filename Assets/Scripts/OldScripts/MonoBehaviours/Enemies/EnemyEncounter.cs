using UnityEngine;

public class EnemyEncounter : MonoBehaviour // Este script estaría en el enemigo en la escena de exploración
{
    [SerializeField] private GameObject enemyCombatPrefab;

    private void OnTriggerEnter(Collider other)
    {
        // Usar CompareTag es más eficiente y desacopla la lógica del PlayerController.
        if (other.CompareTag("Player")) 
        {
            // El SceneTransitionManager ahora es responsable de conocer el prefab del jugador.
            SceneTransitionManager.Instance?.LoadCombatScene(enemyCombatPrefab, other.gameObject);
            gameObject.SetActive(false);
        }
    }
}

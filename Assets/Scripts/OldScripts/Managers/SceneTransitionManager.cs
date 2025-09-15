using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProyectSecret.Combat.SceneManagement;
using UnityEngine.UI;

namespace ProyectSecret.Managers
{
    /// <summary>
    /// Gestiona las transiciones de escena de forma asíncrona y centralizada.
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        [Header("Configuración de Transición")]
        [SerializeField] private float loadingBarSpeed = 1f;
        [SerializeField] private Slider loadingBar;
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float fadeDuration = 0.5f;
        
        [Header("Nombres de Escenas")]
        [SerializeField] private string combatSceneName = "CombatScene";
        [SerializeField] private string explorationSceneName = "Exploracion";

        [Header("Datos de Transferencia")]
        [SerializeField] private CombatTransferData transferData;
        [SerializeField] private PlayerPersistentData playerPersistentData;
        [SerializeField] private GameObject playerPrefab; // Referencia al prefab del jugador

        private bool isTransitioning = false;
        private string currentActiveScene;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Guardamos el nombre de la escena inicial
            currentActiveScene = SceneManager.GetActiveScene().name;
        }

        /// <summary>
        /// Carga una escena de forma genérica, sin pasar datos de combate.
        /// Ideal para ir del menú al tutorial, o del tutorial a la exploración.
        /// </summary>
        public void LoadScene(string sceneName)
        {
            if (isTransitioning) return;
            StartCoroutine(TransitionToScene(sceneName));
        }

        public void LoadCombatScene(GameObject enemyPrefab, GameObject playerInstance)
        {
            if (isTransitioning) return;
            
            if (playerPersistentData != null && playerInstance != null)
                playerPersistentData.SaveFromPlayer(playerInstance);

            transferData.playerPrefab = this.playerPrefab; // Usar la referencia del manager
            transferData.enemyPrefab = enemyPrefab;
            
            StartCoroutine(TransitionToScene(combatSceneName));
        }

        public void LoadExplorationScene(GameObject playerInstance)
        {
            if (isTransitioning) return;

            if (playerPersistentData != null && playerInstance != null)
                playerPersistentData.SaveFromPlayer(playerInstance);

            StartCoroutine(TransitionToScene(explorationSceneName));
        }

        private IEnumerator TransitionToScene(string sceneName)
        {
            isTransitioning = true;

            if (loadingBar != null)
            {
                loadingBar.value = 0;
                loadingBar.gameObject.SetActive(true);
            }

            yield return StartCoroutine(Fade(1f));

            // Descargamos la escena anterior ANTES de cargar la nueva
            if (!string.IsNullOrEmpty(currentActiveScene))
            {
                AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(currentActiveScene);
                while (!asyncUnload.isDone)
                {
                    float targetProgress = Mathf.Clamp01(asyncUnload.progress / 0.9f) * 0.5f;
                    if (loadingBar != null)
                        loadingBar.value = Mathf.MoveTowards(loadingBar.value, targetProgress, Time.deltaTime * loadingBarSpeed);
                    yield return null;
                }
            }

            // Cargamos la nueva escena de forma aditiva
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!asyncLoad.isDone)
            {
                float targetProgress = 0.5f + (Mathf.Clamp01(asyncLoad.progress / 0.9f) * 0.5f);
                if (loadingBar != null)
                    loadingBar.value = Mathf.MoveTowards(loadingBar.value, targetProgress, Time.deltaTime * loadingBarSpeed);
                yield return null;
            }

            currentActiveScene = sceneName;
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

            if (loadingBar != null) loadingBar.gameObject.SetActive(false);

            yield return StartCoroutine(Fade(0f));

            isTransitioning = false;
        }

        private IEnumerator Fade(float targetAlpha)
        {
            fadeCanvasGroup.blocksRaycasts = (targetAlpha == 1f);
            float startAlpha = fadeCanvasGroup.alpha;
            float time = 0;

            while (time < fadeDuration)
            {
                fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
                time += Time.deltaTime;
                yield return null;
            }

            fadeCanvasGroup.alpha = targetAlpha;
        }
    }
}

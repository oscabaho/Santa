using UnityEngine;


namespace ProyectSecret.Managers
{
    /// <summary>
    /// Esta clase se asegura de que los sistemas globales (como el AudioManager)
    /// se inicialicen al iniciar el juego, sin importar desde qué escena se comience.
    /// Es especialmente útil para las pruebas en el editor.
    /// </summary>
    public static class GameInitializer
    {
        private const string AudioManagerPrefabPath = "AudioManager";
        private const string GraphicsManagerPrefabPath = "GraphicsSettingsManager";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void InitializeGameSystems()
        {
            // Instanciar managers desde la carpeta Resources si no existen.
            InstantiateManagerFromResources<AudioManager>(AudioManagerPrefabPath);
            InstantiateManagerFromResources<GraphicsSettingsManager>(GraphicsManagerPrefabPath);
        }

        /// <summary>
        /// Método genérico para instanciar un manager si no existe.
        /// </summary>
        /// <typeparam name="T">El tipo del componente del manager (debe ser un MonoBehaviour).</typeparam>
        /// <param name="prefabPath">La ruta del prefab en la carpeta Resources.</param>
        private static void InstantiateManagerFromResources<T>(string prefabPath) where T : MonoBehaviour
        {
            // Usamos FindFirstObjectByType para no depender de una instancia estática.
            // Esto es más robusto si el singleton se asigna en Awake.
            if (Object.FindFirstObjectByType<T>() == null)
            {
                var managerPrefab = Resources.Load<GameObject>(prefabPath);
                if (managerPrefab != null)
                {
                    Object.Instantiate(managerPrefab);
                    // El método Awake() del manager se encargará del resto (Instance y DontDestroyOnLoad).
                }
                else
                {
                    #if UNITY_EDITOR
                    Debug.LogError($"GameInitializer: No se pudo encontrar el prefab '{prefabPath}' en una carpeta 'Resources'.");
                    #endif
                }
            }
        }
    }
}
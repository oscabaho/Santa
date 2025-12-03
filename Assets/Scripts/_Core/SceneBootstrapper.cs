using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This script lives in the Bootstrapper scene. Its only job is to load the main game scene
/// immediately, ensuring that the persistent systems in this scene are always loaded first.
/// </summary>
public class SceneBootstrapper : MonoBehaviour
{
    [SerializeField] private string _defaultSceneToLoad = "MainMenu"; // Fallback for builds

#if UNITY_EDITOR
    // This static field will be set by the editor script before entering play mode.
    public static string SceneToLoadAfterBootstrapping { get; set; }
#endif

    private void Start()
    {
#if UNITY_EDITOR
        // If the editor script has set a scene, load that one. Otherwise, load the default.
        if (!string.IsNullOrEmpty(SceneToLoadAfterBootstrapping))
        {
            SceneManager.LoadScene(SceneToLoadAfterBootstrapping);
            return;
        }
#endif
        // In a build, or if no scene was specified by the editor, load the default scene.
        SceneManager.LoadScene(_defaultSceneToLoad);
    }
}
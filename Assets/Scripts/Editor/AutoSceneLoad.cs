#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// This editor script ensures the Bootstrapper scene is always loaded first when entering Play Mode.
/// This guarantees that all persistent systems are initialized before the game logic begins.
/// </summary>
[InitializeOnLoad]
public static class AutoSceneLoad
{
    private const string BOOTSTRAPPER_SCENE_PATH = "Assets/Scenes/Bootstrapper.unity";

    static AutoSceneLoad()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // This method is called when the play mode state changes in the editor.

        // If we are about to enter play mode AND we are not already in the bootstrapper scene...
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // Store the path of the scene the user is currently working on.
            string currentScenePath = SceneManager.GetActiveScene().path;
            SceneBootstrapper.SceneToLoadAfterBootstrapping = currentScenePath;

            // Set the scene to start with when play mode begins.
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(BOOTSTRAPPER_SCENE_PATH);
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            // When exiting play mode, reset the start scene to null so that Unity's default behavior is restored.
            EditorSceneManager.playModeStartScene = null;
        }
    }
}
#endif

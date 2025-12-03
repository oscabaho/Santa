#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Santa.Editor
{
    public static class PlayerPrefsTools
    {
        [MenuItem("Santa/Clear All PlayerPrefs", false, 50)]
        public static void ClearAllPlayerPrefs()
        {
            if (!EditorUtility.DisplayDialog(
                "Clear All PlayerPrefs",
                "This will delete all PlayerPrefs for this project.",
                "Clear",
                "Cancel"))
            {
                return;
            }

            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("PlayerPrefsTools: Cleared all PlayerPrefs.");
        }

        [MenuItem("Santa/Open PersistentDataPath", false, 51)]
        public static void OpenPersistentDataPath()
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }
    }
}
#endif

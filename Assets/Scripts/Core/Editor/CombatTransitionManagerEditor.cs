using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CombatTransitionManager))]
public class CombatTransitionManagerEditor : Editor
{
    // A field to hold the GameObject to be used as the combat scene parent for testing
    private GameObject testCombatSceneParent;

    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Get a reference to the CombatTransitionManager instance
        CombatTransitionManager manager = (CombatTransitionManager)target;

        // Add a separator to distinguish the custom part of the inspector
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Testing Controls", EditorStyles.boldLabel);

        // Create a field for the user to drag and drop the test combat scene parent
        testCombatSceneParent = (GameObject)EditorGUILayout.ObjectField("Test Combat Scene", testCombatSceneParent, typeof(GameObject), true);

        // Add a button to start the combat transition
        if (GUILayout.Button("Start Combat"))
        {
            if (Application.isPlaying)
            {
                // Call the StartCombat method with the specified test GameObject
                manager.StartCombat(testCombatSceneParent);
            }
            else
            {
                GameLog.LogWarning("Combat transitions can only be tested in Play Mode.");
            }
        }

        // Add a button to end the combat transition
        if (GUILayout.Button("End Combat"))
        {
            if (Application.isPlaying)
            {
                // Call the EndCombat method
                manager.EndCombat();
            }
            else
            {
                GameLog.LogWarning("Combat transitions can only be tested in Play Mode.");
            }
        }
    }
}

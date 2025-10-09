using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(SetComponentEnabledTask))]
public class SetComponentEnabledTaskEditor : Editor
{
    private SerializedProperty targetIdProp;
    private SerializedProperty componentTypeProp;
    private SerializedProperty enabledProp;

    private void OnEnable()
    {
        targetIdProp = serializedObject.FindProperty("targetId");
        componentTypeProp = serializedObject.FindProperty("componentType");
        enabledProp = serializedObject.FindProperty("enabled");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(targetIdProp);
        EditorGUILayout.PropertyField(enabledProp);

        var task = (SetComponentEnabledTask)target;
        var targetId = (TargetId)targetIdProp.enumValueIndex;

        GameObject targetObject = FindTargetGameObject(targetId);

        if (targetObject != null)
        {
            var behaviours = targetObject.GetComponents<Behaviour>().ToList();
            var componentNames = behaviours.Select(b => b.GetType().FullName).ToList();
            string currentComponentType = componentTypeProp.stringValue;

            int currentIndex = componentNames.IndexOf(currentComponentType);
            if (currentIndex < 0) currentIndex = 0; // Default to the first component if not found

            int newIndex = EditorGUILayout.Popup("Component Type", currentIndex, componentNames.ToArray());

            if (newIndex >= 0 && newIndex < componentNames.Count)
            {
                componentTypeProp.stringValue = componentNames[newIndex];
            }
        }
        else
        {
            // Fallback to the original text field if we can't find the target
            EditorGUILayout.PropertyField(componentTypeProp, new GUIContent("Component Type (Fallback)"));
            EditorGUILayout.HelpBox("Could not find CombatTransitionManager in the scene or the specified TargetId is not set on it. Component list not available.", MessageType.Warning);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private GameObject FindTargetGameObject(TargetId targetId)
    {
        var manager = FindFirstObjectByType<CombatTransitionManager>();
        if (manager == null) return null;

        switch(targetId)
        {
            case TargetId.ExplorationPlayer: return manager.ExplorationPlayer;
            case TargetId.CombatPlayer: return manager.CombatPlayer;
            case TargetId.ExplorationCamera: return manager.ExplorationCamera;
            case TargetId.ExplorationUI: return manager.ExplorationUI;
            case TargetId.CombatUI: return manager.CombatUI;
            default: return null;
        }
    }
}

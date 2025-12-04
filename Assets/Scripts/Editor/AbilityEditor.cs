using Santa.Domain.Combat;
using UnityEditor;
using UnityEngine;

namespace Santa.Editor
{
    #if UNITY_EDITOR
    [CustomEditor(typeof(Ability), true)]
public class AbilityEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var targetingProp = serializedObject.FindProperty("_targeting");
        var targeting = targetingProp.objectReferenceValue as TargetingStrategy;
        bool shouldShowTargetPercentage = targeting is RandomEnemiesTargeting;

        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;

        while (prop.NextVisible(enterChildren))
        {
            enterChildren = false;

            // Skip Target Percentage field if it's not needed
            if (prop.name == "_targetPercentage" && !shouldShowTargetPercentage)
            {
                continue; // Skip drawing this property
            }

            // Draw the property (disable script field as usual)
            EditorGUI.BeginDisabledGroup(prop.name == "m_Script");
            EditorGUILayout.PropertyField(prop, true);
            EditorGUI.EndDisabledGroup();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
    #endif
}

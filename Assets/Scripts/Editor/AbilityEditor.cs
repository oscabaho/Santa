using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom editor for Ability to conditionally hide Target Percentage field
/// when it's not being used by the selected targeting strategy.
/// </summary>
#if UNITY_EDITOR
[CustomEditor(typeof(Ability), true)]
public class AbilityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;
        
        while (prop.NextVisible(enterChildren))
        {
            enterChildren = false;
            
            // Skip Target Percentage field if targeting is not RandomEnemiesTargeting
            if (prop.name == "_targetPercentage")
            {
                var targetingProp = serializedObject.FindProperty("_targeting");
                if (targetingProp.objectReferenceValue != null)
                {
                    var targeting = targetingProp.objectReferenceValue as TargetingStrategy;
                    // Only show for RandomEnemiesTargeting
                    if (targeting != null && !(targeting is RandomEnemiesTargeting))
                    {
                        continue; // Skip drawing this property
                    }
                }
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

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BasicFighter2D))]
public class SimpleFighterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw all properties
        SerializedProperty prop = serializedObject.GetIterator();
        prop.NextVisible(true); // Skip script property

        while (prop.NextVisible(false))
        {
            // Check if we're in the health bar section
            if (prop.name == "customHealthBar")
            {
                // Draw the custom health bar toggle
                EditorGUILayout.PropertyField(prop);
                
                // Get the customHealthBar value
                bool isCustom = prop.boolValue;
                
                // If custom health bar is enabled, show message instead of other fields
                if (isCustom)
                {
                    EditorGUILayout.HelpBox("Custom health bar enabled. Please add your own custom health bar implementation and use GetCurrentHP() / GetMaxHP() to access health values.", MessageType.Info);
                    
                    // Skip to next section
                    while (prop.NextVisible(false) && prop.name != "animIdle")
                    {
                        // Skip all health bar properties
                    }
                    EditorGUILayout.PropertyField(prop); // Draw animIdle
                }
                else
                {
                    // Draw health bar properties normally
                    while (prop.NextVisible(false) && prop.name != "animIdle")
                    {
                        EditorGUILayout.PropertyField(prop);
                    }
                    EditorGUILayout.PropertyField(prop); // Draw animIdle
                }
            }
            else if (prop.name == "healthBarSortingOrder")
            {
                // Skip private sorting order
                continue;
            }
            else
            {
                EditorGUILayout.PropertyField(prop);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif

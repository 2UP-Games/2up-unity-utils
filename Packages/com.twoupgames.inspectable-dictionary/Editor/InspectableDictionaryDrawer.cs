using System;

using UnityEngine;

using UnityEditor;
using Random = UnityEngine.Random;
/// <summary>
/// CustomPropertyDrawer for InspectableDictionary
/// </summary>
[CustomPropertyDrawer(typeof(InspectableDictionary<string, string>))]
[CustomPropertyDrawer(typeof(InspectableDictionary<   int, string>))]
[CustomPropertyDrawer(typeof(InspectableDictionary<string,    int>))]
[CustomPropertyDrawer(typeof(InspectableDictionary<   int,    int>))]
public class InspectableDictionaryDrawer : PropertyDrawer
{
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that prefab override logic works on the entire property
        EditorGUI.BeginProperty(position, label, property);

        // Manually set indent
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var   keys = property.FindPropertyRelative(  "_keys");
        var values = property.FindPropertyRelative("_values");

        var max = MathF.Min(keys.arraySize, values.arraySize);

        GUILayout.BeginVertical(label, "window");

        for (int i = 0; i < max; i++)
        {
            var   key =   keys.GetArrayElementAtIndex(i);
            var value = values.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginHorizontal();

            // Draw fields - pass GUIContent.none to each so they are drawn without labels
            EditorGUILayout.PropertyField(  key, GUIContent.none);
            EditorGUILayout.PropertyField(value, GUIContent.none);

            if (   _DrawAddButton(i)) return;
            if (_DrawRemoveButton(i)) return;

            EditorGUILayout.EndHorizontal();
        }

        // Footer

        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label($"<{keys.arrayElementType}, {values.arrayElementType}>", EditorStyles.boldLabel);

            if (_DrawAddButton(keys.arraySize)) return;

            EditorGUILayout.Space(19, false);
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.EndVertical();

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();

        #region LOCAL METHODS

        /// Returns true if collection was modified
        bool _DrawAddButton(int i)
        {
            if (GUILayout.Button("+", GUILayout.ExpandWidth(false)))
            {
                  keys.InsertArrayElementAtIndex(i);
                values.InsertArrayElementAtIndex(i);

                // Give new key a unique value so dictionary doesn't collide
                var prop = keys.GetArrayElementAtIndex(i);
                _SetUniqueValue(prop);

                property.serializedObject.ApplyModifiedProperties();
                EditorGUILayout.EndHorizontal();
                return true;
            }
            return false;
        }

        /// Returns true if collection was modified
        bool _DrawRemoveButton(int i)
        {
            if (GUILayout.Button("-", GUILayout.ExpandWidth(false)))
            {
                  keys.DeleteArrayElementAtIndex(i);
                values.DeleteArrayElementAtIndex(i);

                property.serializedObject.ApplyModifiedProperties();
                EditorGUILayout.EndHorizontal();
                return true;
            }
            return false;
        }

        void _SetUniqueValue(SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    prop.intValue = Random.Range(0, int.MaxValue);
                    break;

                case SerializedPropertyType.String:
                    prop.stringValue = Guid.NewGuid().ToString();
                    break;
            }
        }

        #endregion LOCAL METHODS
    }
}
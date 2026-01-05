using System;
using UnityEditor;
using UnityEngine;

namespace RAXY.Utility.Editor
{
    public class CustomEditorUtility
    {
        public static T DrawEnum<T>(string label,
                                    T currentValue,
                                    Action<T> onValueChanged = null
                                    ) where T : Enum
        {
            EditorGUILayout.BeginVertical();

            if (!string.IsNullOrEmpty(label))
                EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            foreach (T value in Enum.GetValues(typeof(T)))
            {
                bool isSelected = currentValue.Equals(value);

                GUIStyle style = GetButtonStyle(isSelected);

                if (GUILayout.Button(ObjectNames.NicifyVariableName(value.ToString()), style))
                {
                    if (!isSelected)
                    {
                        currentValue = value;
                        onValueChanged?.Invoke(value);
                        GUI.FocusControl(null);
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            return currentValue;
        }

        static GUIStyle GetButtonStyle(bool selected)
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniButtonMid);

            if (selected)
            {
                style.normal = style.active;
                style.fontStyle = FontStyle.Bold;
            }

            return style;
        }
    }
}
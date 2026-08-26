using UnityEditor;
using UnityEngine;

namespace CDTU.SOHelper.Editor
{
    /// <summary>
    /// Adds an Edit button beside every ScriptableObject reference.
    /// </summary>
    [CustomPropertyDrawer(typeof(ScriptableObject), true)]
    public sealed class ScriptableObjectReferenceDrawer : PropertyDrawer
    {
        private const float ButtonWidth = 42f;
        private const float Spacing = 4f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (var propertyScope = new EditorGUI.PropertyScope(position, label, property))
            {
                label = propertyScope.content;
                if (property.propertyType != SerializedPropertyType.ObjectReference)
                {
                    EditorGUI.PropertyField(position, property, label, true);
                    return;
                }

                var buttonWidth = Mathf.Min(ButtonWidth, position.width);
                var fieldRect = new Rect(
                    position.x,
                    position.y,
                    Mathf.Max(0f, position.width - buttonWidth - Spacing),
                    position.height);
                var buttonRect = new Rect(
                    position.xMax - buttonWidth,
                    position.y,
                    buttonWidth,
                    EditorGUIUtility.singleLineHeight);

                EditorGUI.PropertyField(fieldRect, property, label, true);

                var target = property.objectReferenceValue as ScriptableObject;
                var canEdit = target != null && !property.hasMultipleDifferentValues;
                using (new EditorGUI.DisabledScope(!canEdit))
                {
                    if (GUI.Button(buttonRect, new GUIContent("Edit", "Open in a focused Inspector window")))
                        ScriptableObjectInspectorWindow.Open(target);
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}

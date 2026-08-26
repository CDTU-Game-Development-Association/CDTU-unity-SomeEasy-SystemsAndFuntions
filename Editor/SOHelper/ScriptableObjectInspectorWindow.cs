using System;
using UnityEditor;
using UnityEngine;
using UnityObjectEditor = UnityEditor.Editor;

namespace CDTU.SOHelper.Editor
{
    /// <summary>
    /// Hosts the target asset's regular or custom Inspector inside a focused window.
    /// </summary>
    public sealed class ScriptableObjectInspectorWindow : EditorWindow
    {
        [SerializeField] private ScriptableObject target;
        [NonSerialized] private UnityObjectEditor targetEditor;
        [SerializeField] private Vector2 scrollPosition;

        public static void Open(ScriptableObject scriptableObject)
        {
            if (scriptableObject == null)
                throw new ArgumentNullException(nameof(scriptableObject));

            var window = GetWindow<ScriptableObjectInspectorWindow>();
            window.SetTarget(scriptableObject);
            window.Show();
            window.Focus();
        }

        private void OnEnable()
        {
            minSize = new Vector2(320f, 240f);
            UpdateTitle();
        }

        private void OnDisable()
        {
            DestroyTargetEditor();
        }

        private void OnGUI()
        {
            if (target == null)
            {
                EditorGUILayout.HelpBox(
                    "The ScriptableObject asset is missing or was deleted.",
                    MessageType.Info);
                return;
            }

            DrawToolbar();
            EnsureTargetEditor();
            if (targetEditor == null)
            {
                EditorGUILayout.HelpBox("Unity could not create an Inspector for this asset.", MessageType.Error);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            targetEditor.OnInspectorGUI();
            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label(target.name, EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Select", EditorStyles.toolbarButton, GUILayout.Width(52f)))
                {
                    Selection.activeObject = target;
                    EditorGUIUtility.PingObject(target);
                }
            }
        }

        private void SetTarget(ScriptableObject scriptableObject)
        {
            if (target == scriptableObject)
                return;

            DestroyTargetEditor();
            target = scriptableObject;
            scrollPosition = Vector2.zero;
            UpdateTitle();
            Repaint();
        }

        private void EnsureTargetEditor()
        {
            if (targetEditor == null || targetEditor.target != target)
                UnityObjectEditor.CreateCachedEditor(target, null, ref targetEditor);
        }

        private void DestroyTargetEditor()
        {
            if (targetEditor == null)
                return;

            DestroyImmediate(targetEditor);
            targetEditor = null;
        }

        private void UpdateTitle()
        {
            titleContent = new GUIContent(target == null ? "SO Inspector" : target.name);
        }
    }
}

using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using System.Collections;

[CustomEditor(typeof(UIRichText))]
[CanEditMultipleObjects]
public class UIRichTextEditor : UITextEditor
{
    private SerializedProperty _showClickProperty;

    protected override void OnEnable()
    {
        base.OnEnable();
        _showClickProperty = serializedObject.FindProperty("showClickArea");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.PropertyField(_showClickProperty);
        this.serializedObject.ApplyModifiedProperties();
    }
}

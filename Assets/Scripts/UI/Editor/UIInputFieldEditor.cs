using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(UIInputField),true)]
[CanEditMultipleObjects]
public class UIInputFieldEditor : InputFieldEditor
{
    private SerializedProperty _UIModProperty;
    private SerializedProperty _controlIDProperty;
    private SerializedProperty _enableValueChangeEventProperty;

    protected override void OnEnable()
    {
        base.OnEnable();
        _UIModProperty = serializedObject.FindProperty("uiMod");
        _controlIDProperty = serializedObject.FindProperty("controlID");
        _enableValueChangeEventProperty = serializedObject.FindProperty("enableValueChangeEvent");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(_UIModProperty);
        EditorGUILayout.PropertyField(_controlIDProperty);
        EditorGUILayout.PropertyField(_enableValueChangeEventProperty);

        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();
    }
}

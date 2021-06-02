using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using System.Collections;

[CustomEditor(typeof(UIStampTimer))]
[CanEditMultipleObjects]
public class UIStampTimerEditor : UITextEditor
{
    private SerializedProperty _UIModProperty;
    private SerializedProperty _controlIDProperty;

    protected override void OnEnable()
    {
        base.OnEnable();
        _UIModProperty      = serializedObject.FindProperty("uiMod");
        _controlIDProperty  = serializedObject.FindProperty("controlID");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(_UIModProperty);
        EditorGUILayout.PropertyField(_controlIDProperty);
        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();
    }
}

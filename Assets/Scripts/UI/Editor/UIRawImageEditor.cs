using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using System.Collections;

[CustomEditor(typeof(UIRawImage))]
[CanEditMultipleObjects]
public class UIRawImageEditor : RawImageEditor
{
    private SerializedProperty _multiResProperty;
    private SerializedProperty _multiResNativeSizeProperty;

    protected override void OnEnable()
    {
        base.OnEnable();
        _multiResProperty           = serializedObject.FindProperty("_multiRes");
        _multiResNativeSizeProperty = serializedObject.FindProperty("_multiResNativeSize");
    }

    public override void OnInspectorGUI(){
        base.OnInspectorGUI();
        EditorGUILayout.PropertyField(_multiResProperty);
        EditorGUILayout.PropertyField(_multiResNativeSizeProperty);
        serializedObject.ApplyModifiedProperties();
	}
}

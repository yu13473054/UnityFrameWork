using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using System.Collections;

[CustomEditor(typeof(UIImage))]
[CanEditMultipleObjects]
public class UIImageEditor : ImageEditor
{
    private SerializedProperty _colorByGrayFactorProperty;
    private SerializedProperty _multiResProperty;
    private SerializedProperty _multiResNativeSizeProperty;

    protected override void OnEnable()
    {
        base.OnEnable();
        _colorByGrayFactorProperty = serializedObject.FindProperty("_colorByGrayFactor");
        _multiResProperty = serializedObject.FindProperty("_multiRes");
        _multiResNativeSizeProperty = serializedObject.FindProperty("_multiResNativeSize");

    }

    public override void OnInspectorGUI(){
        base.OnInspectorGUI();
        EditorGUILayout.Slider(_colorByGrayFactorProperty, 0, 1);
        EditorGUILayout.PropertyField(_multiResProperty);
        EditorGUILayout.PropertyField(_multiResNativeSizeProperty);

        serializedObject.ApplyModifiedProperties();
    }
}

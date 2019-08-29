using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(UIScrollSlider), true)]
public class UIScrollSliderEditor : SliderEditor
{
    private SerializedProperty _UIModProperty;
    private SerializedProperty _controlIDProperty;
    //private SerializedProperty _clickIntervalProperty;
    private SerializedProperty _audioIdProperty;
    private SerializedProperty _scrollRectProperty;
    private SerializedProperty _scrollRectDirectionProperty;

    protected override void OnEnable()
    {
        base.OnEnable();
        _UIModProperty = serializedObject.FindProperty("uiMod");
        _controlIDProperty = serializedObject.FindProperty("controlID");
        //_clickIntervalProperty = serializedObject.FindProperty("clickInterval");
        _audioIdProperty = serializedObject.FindProperty("audioId");
        _scrollRectProperty = serializedObject.FindProperty("scrollRect");
        _scrollRectDirectionProperty = serializedObject.FindProperty("scrollRectDirection");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(_UIModProperty);
        EditorGUILayout.PropertyField(_controlIDProperty);
        //EditorGUILayout.PropertyField(_clickIntervalProperty);
        EditorGUILayout.PropertyField(_audioIdProperty);
        EditorGUILayout.PropertyField(_scrollRectProperty);
        EditorGUILayout.PropertyField(_scrollRectDirectionProperty);
        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();
    }
}

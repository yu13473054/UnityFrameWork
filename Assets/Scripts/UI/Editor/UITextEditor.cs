﻿using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using System.Collections;

[CustomEditor(typeof(UIText))]
[CanEditMultipleObjects]
public class UITextEditor : UnityEditor.UI.TextEditor
{
    private SerializedProperty _localizationIDProperty;
    private SerializedProperty _fontNameProperty;
    private float _lastFontSize;
    private string _lastTextID = "";

    protected override void OnEnable()
    {
        base.OnEnable();
        _localizationIDProperty = serializedObject.FindProperty("textID");
        _fontNameProperty = serializedObject.FindProperty("fontName");
    }

    public override void OnInspectorGUI()
    {
        UIText txt = target as UIText;
        string localizeID = _localizationIDProperty.stringValue;
        EditorGUILayout.PropertyField(_localizationIDProperty);
        if (_lastTextID != localizeID)
        {
            if (txt != null)
            {
                _lastTextID = localizeID;

                Localization.Init();
                string dataText = Localization.Get(localizeID);
                if (!string.IsNullOrEmpty(dataText))
                {
                    txt.text = dataText;
                }
            }
        }
        if (txt.resizeTextForBestFit)
        {
            if (_lastFontSize != txt.fontSize)
            {

                txt.resizeTextMaxSize = txt.fontSize;
                _lastFontSize = txt.fontSize;
            }
        }
        this.serializedObject.ApplyModifiedProperties();
        base.OnInspectorGUI();
        EditorGUILayout.PropertyField(_fontNameProperty);
        this.serializedObject.ApplyModifiedProperties();
    }
}

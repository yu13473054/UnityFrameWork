using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#endif

[AttributeUsage(AttributeTargets.Field)]
public class LabelAttribute : PropertyAttribute
{
    public string label;
    public float min;
    public float max;
    public LabelAttribute(string label, float min, float max)
    {
        this.label = label;
        this.min = min;
        this.max = max;
    }
    public LabelAttribute(string label)
    {
        this.label = label;
        this.min = -10000;
        this.max = -10000;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(LabelAttribute))]
public class LabelDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        LabelAttribute range = attribute as LabelAttribute;
        label.text = range.label;
        bool disableSlider = (range.min == range.max && range.max == -10000);
        if(!disableSlider)
        {
            if (property.propertyType == SerializedPropertyType.Float)
                EditorGUI.Slider(position, property, range.min, range.max, label);
            else if (property.propertyType == SerializedPropertyType.Integer)
                EditorGUI.IntSlider(position, property, (int)range.min, (int)range.max, label);
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
        }
    }

}
#endif
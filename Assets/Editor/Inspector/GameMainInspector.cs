
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameMain))]
public class GameMainInspector : Editor {


    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SerializedProperty pro = null;
        pro = serializedObject.FindProperty("_targetFrameRate");
        EditorGUILayout.PropertyField(pro, new GUIContent("游戏帧率"));
        pro = serializedObject.FindProperty("_developMode");
        EditorGUILayout.PropertyField(pro, new GUIContent("开发者模式"));
        serializedObject.ApplyModifiedProperties();
    }
}

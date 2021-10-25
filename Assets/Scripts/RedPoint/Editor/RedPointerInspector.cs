using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RedPointer), true)]
public class RedPointerInspector : Editor
{
    SerializedProperty _idProperty;
    SerializedProperty _typeProperty;
    SerializedProperty _goProperty;
    SerializedProperty _numProperty;

    private RedPointAsset _assetObj;
    private Dictionary<int, RedPointAssetCfg> _nodeDic;
    private string _key;

    private void OnEnable()
    {
        _idProperty = serializedObject.FindProperty("id");
        _typeProperty = serializedObject.FindProperty("type");
        _goProperty = serializedObject.FindProperty("viewGo");
        _numProperty = serializedObject.FindProperty("numTxt");

        if (!Application.isPlaying)
        {
            string path = "Assets/Data/RedPointCfg.asset";
            if (File.Exists(path))
            {
                _assetObj = AssetDatabase.LoadAssetAtPath<RedPointAsset>(path);
                _nodeDic = new Dictionary<int, RedPointAssetCfg>();
                _nodeDic.Add(1, _assetObj.cfg);//添加根节点
                for (int i = 0; i < _assetObj.cfgList.Count; i++)
                {
                    RedPointAssetCfg cfg = _assetObj.cfgList[i];
                    _nodeDic.Add(cfg.id, cfg);
                }
                if (!_nodeDic.ContainsKey(_idProperty.intValue))
                {
                    _idProperty.intValue = 0;
                    _idProperty.serializedObject.ApplyModifiedProperties();
                    _key = "Dynamic";
                }
                else
                {
                    _key = GetKey(_idProperty.intValue);
                }
            }
            else
            {
                _assetObj = null;
                _nodeDic = null;
                _idProperty.intValue = 0;
                _idProperty.serializedObject.ApplyModifiedProperties();
                _key = "Dynamic";
            }
        }
        else //运行时，从RedPointMgr中获取数据
        {
            _key = GetKey(_idProperty.intValue);
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        using(new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Key: ", GUILayout.Width(40));
            if (GUILayout.Button(_key))
            {
                if (!_assetObj)
                    EditorUtility.DisplayDialog("提示", "请先生成红点配置文件！", "确定");
                else
                    ShowList();
            }
        }
        EditorGUILayout.PropertyField(_typeProperty);
        EditorGUILayout.PropertyField(_goProperty);
        if (_typeProperty.enumValueIndex == (int)RedViewType.Num)
        {
            EditorGUILayout.PropertyField(_numProperty);
        }
        else
        {
            _numProperty.objectReferenceValue = null;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private string GetKey(int id, string subfix = null)
    {
        string key;
        int parentId;
        if (Application.isPlaying)
        {
            RedPointNode node = RedPointMgr.Inst.nodeDic[id];
            key = node.key;
            parentId = node.parentId;
        }
        else
        {
            RedPointAssetCfg cfg =  _nodeDic[id];
            key = cfg.key;
            parentId = cfg.parentId;

        }
        if (string.IsNullOrEmpty(subfix))
        {
            subfix = key;
        }
        else
        {
            subfix = key + "." + subfix;
        }
        if(parentId == -1) //已经达到根节点了
        {
            return subfix;
        }
        return GetKey(parentId, subfix);
    }

    private void OnSelectNode(object data)
    {
        int id = (int)data;
        _idProperty.intValue = id;
        _idProperty.serializedObject.ApplyModifiedProperties();
        if (id == 0)
            _key = "Dynamic";
        else
            _key = GetKey(id);
    }

    public void ShowList()
    {
        //显示列表
        GenericMenu menu = new GenericMenu();
        menu.allowDuplicateNames = true;
        RedPointAssetCfg cfg = _assetObj.cfg;
        menu.AddItem(new GUIContent("Dynamic"), false, OnSelectNode, 0);
        menu.AddItem(new GUIContent(cfg.key), false, OnSelectNode, cfg.id);
        menu.AddSeparator("");
        AddCfgItem(menu, cfg, cfg.key);
        menu.ShowAsContext();
    }

    private void AddCfgItem(GenericMenu menu, RedPointAssetCfg cfg, string key)
    {
        string prefixKey = key + "/";
        bool addChildItem = false;
        for (int i = 0; i < cfg.childs.Count; i++)
        {
            RedPointAssetCfg childCfg = _nodeDic[cfg.childs[i]];
            menu.AddItem(new GUIContent(prefixKey + childCfg.key), false, OnSelectNode, childCfg.id);
            addChildItem = true;
        }
        //添加分割线
        if (addChildItem)
            menu.AddSeparator(prefixKey);
        //添加下一层
        for (int i = 0; i < cfg.childs.Count; i++)
        {
            RedPointAssetCfg childCfg = _nodeDic[cfg.childs[i]];
            if (childCfg.childs.Count > 0)
                AddCfgItem(menu, childCfg, prefixKey + childCfg.key);
        }
    }
    
}

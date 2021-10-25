using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RedPointAsset), true)]
public class RedPointAssetInspector : Editor
{
    private RedPointAsset assetObj;
    private Dictionary<int, RedPointAssetCfg> _nodeDic;
    private Dictionary<int, RedPointAssetCfgViewItem> _nodeViewDic;
    private int _index;

    private void OnEnable()
    {
        _index = 1;

        assetObj = (RedPointAsset)target;
        //根节点的id = 1， parentId = -1；
        assetObj.cfg.id = 1;
        assetObj.cfg.parentId = -1;
        assetObj.cfg.key = "Root";

        _nodeDic = new Dictionary<int, RedPointAssetCfg>();
        _nodeViewDic = new Dictionary<int, RedPointAssetCfgViewItem>();
        _nodeDic.Add(1, assetObj.cfg);//添加根节点
        _nodeViewDic.Add(1, new RedPointAssetCfgViewItem(this, assetObj.cfg));
        for (int i = 0; i < assetObj.cfgList.Count; i++)
        {
            RedPointAssetCfg cfg = assetObj.cfgList[i];
            _nodeDic.Add(cfg.id, cfg);
            _nodeViewDic.Add(cfg.id, new RedPointAssetCfgViewItem(this, cfg));
            if (cfg.id > _index) //获取最大的索引值
            {
                _index = cfg.id;
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        _nodeViewDic[1].OnInspectorGUI(0);
        if (GUI.changed)
            EditorUtility.SetDirty(assetObj);
        serializedObject.ApplyModifiedProperties();
    }

    public RedPointAssetCfg GetRootNodeCfg()
    {
        return assetObj.cfg;
    }

    public RedPointAssetCfg GetNodeCfg(int id)
    {
        return _nodeDic[id];
    }

    public RedPointAssetCfgViewItem GetNodeView(int id)
    {
        return _nodeViewDic[id];
    }

    //增加节点
    public RedPointAssetCfg AddNodeCfg()
    {
        _index++;
        RedPointAssetCfg cfg = new RedPointAssetCfg()
        {
            id = _index,
            key = _index.ToString()
        };
        _nodeDic.Add(_index, cfg);
        _nodeViewDic.Add(_index, new RedPointAssetCfgViewItem(this, cfg));
        assetObj.cfgList.Add(cfg);
        return cfg;
    }

    public bool isValid(int id)
    {
        return _nodeDic.ContainsKey(id);
    }

    //删除节点
    public void RemoveNodeCfg(int id)
    {
        RedPointAssetCfg removeCfg = _nodeDic[id];
        _nodeDic.Remove(id);
        _nodeViewDic.Remove(id);
        for(int i = 0; i < removeCfg.childs.Count; i++)
        {
            RemoveNodeCfg(removeCfg.childs[i]);
        }
    }


    //删除数据后，重新构建保存的List
    public void RebuildList()
    {
        assetObj.cfgList.Clear();
        foreach(var pair in _nodeDic)
        {
            if (pair.Key == GetRootNodeCfg().id)
                continue;
            assetObj.cfgList.Add(pair.Value);
        }
    }

    public void ShowParentList(int expectId, GenericMenu.MenuFunction2 callBack)
    {
        //显示列表
        GenericMenu menu = new GenericMenu();
        menu.allowDuplicateNames = true;
        RedPointAssetCfg cfg = GetRootNodeCfg();
        //排除的节点的直接父节点也不显示
        if(GetNodeCfg(expectId).parentId != cfg.id)
        {
            menu.AddItem(new GUIContent(cfg.key), false, callBack, cfg.id);
            menu.AddSeparator("");
        }
        AddCfgItem(expectId, menu, cfg, cfg.key, callBack);
        menu.ShowAsContext();
    }

    private void AddCfgItem(int expectId, GenericMenu menu, RedPointAssetCfg cfg, string key, GenericMenu.MenuFunction2 callBack)
    {
        if (cfg.id == expectId) return; //排除掉不显示的
        RedPointAssetCfg expectCfg = GetNodeCfg(expectId);
        string prefixKey = key + "/";
        bool addChildItem = false;
        for (int i = 0; i < cfg.childs.Count; i++)
        {
            RedPointAssetCfg childCfg = GetNodeCfg(cfg.childs[i]);
            if (childCfg.id == expectId) continue; //排除掉不显示的
            if (expectCfg.parentId == childCfg.id)
                continue;
            menu.AddItem(new GUIContent(prefixKey + childCfg.key), false, callBack, childCfg.id);
            addChildItem = true;
        }
        //添加分割线
        if(addChildItem)
            menu.AddSeparator(prefixKey);
        //添加下一层
        for (int i = 0; i < cfg.childs.Count; i++)
        {
            RedPointAssetCfg childCfg = GetNodeCfg(cfg.childs[i]);
            if (childCfg.childs.Count > 0)
                AddCfgItem(expectId, menu, childCfg, prefixKey + childCfg.key, callBack);
        }
    }
}

public class RedMenuHelper
{
    [MenuItem("Tools/Create RedPoint Asset")]
    public static void CreateAsset()
	{
        ScriptableObject obj = ScriptableObject.CreateInstance<RedPointAsset>();
        string dir = "Assets/Data";
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        string path = dir + "/RedPointCfg.asset";
        if (File.Exists(path))
        {
            Debug.LogErrorFormat("<RedNodeEditor> 红点配置文件已经存在，path = {0}", path);
            return;
        }
        AssetDatabase.CreateAsset(obj, path);
	}
}

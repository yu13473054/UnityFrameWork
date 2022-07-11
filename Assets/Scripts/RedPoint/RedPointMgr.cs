using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 红点管理器，负责根据配置的Tag生成树结构。当红点数量有变化时，使用事件系统进行红点数量变化的通知。
/// Tag可以为固定Tag，例如活动页；也可以为动态Tag，例如道具背包中的每个Item
/// </summary>
public class RedPointMgr : Singleton<RedPointMgr>
{
    public Dictionary<int, RedPointNode> nodeDic { get; private set; }

    private int _index; //当前节点的最大索引

    public void Init()
    {
        nodeDic = new Dictionary<int, RedPointNode>();
        //初始化红点结构
        RedPointAsset pointAsset = ResMgr.Inst.LoadAsset<RedPointAsset>("RedPointCfg", 3, "RedPoint");
        //转换下数据
        AddNode(pointAsset.cfg);
        for (int i = 0; i < pointAsset.cfgList.Count; i++)
        {
            AddNode(pointAsset.cfgList[i]);
        }
        //构建下每个节点的查询表
        foreach(var pair in nodeDic)
        {
            pair.Value.BuildChildDic();
            if (pair.Key > _index)
                _index = pair.Key;
        }

        //释放空间
        ResMgr.Inst.OnModuleDestroy("RedPoint", true);
    }

    private void AddNode(RedPointAssetCfg cfg)
    {
        RedPointNode node = new RedPointNode(cfg);
        nodeDic.Add(cfg.id, node);
    }
    private RedPointNode AddNode(int id, string key, int parentId)
    {
        RedPointNode node = new RedPointNode(id, key, parentId);
        nodeDic.Add(id, node);
        return node;
    }

    public RedPointNode rootNode
    {
        get
        {
            return nodeDic[1];
        }
    }

    /// <summary>
    /// 通过Key获取到对应到节点，必须从根节点开始
    /// </summary>
    /// <param name="fullKey">例如：Root.Main.Mail</param>
    public int GetIdByKey(string fullKey)
    {
        string[] splits = fullKey.Split('.');
        RedPointNode node = rootNode;
        if (!splits[0].Equals(node.key))
        {
            Debug.LogErrorFormat("<RedPointMgr> {0}需要从根节点{1}开始！", fullKey, node.key);
            return -1;
        }
        int id = node.id;
        for (int j = 1; j < splits.Length; j++)
        {
            string key = splits[j];
            if (!node.childDicByKey.TryGetValue(key, out id))
            {
                Debug.LogErrorFormat("<RedPointMgr> 节点{0}不存在！", key);
                return -1;
            }
            //节点存在，继续查找
            node = nodeDic[id];
        }
        return id;
    }

    /// 为parentId所在的节点，添加子节点
    /// <param name="key">叶子节点key</param>
    /// <param name="parentId">父节点id</param>
    public int AddDynamicKey(string key, int parentId)
    {
        RedPointNode parentNode = nodeDic[parentId];
        int id;
        if(parentNode.childDicByKey.TryGetValue(key, out id))
        {
            if (!nodeDic[id].IsLeafNode())
            {
                Debug.LogErrorFormat("<RedPointMgr> {0}不是叶子节点！", key);
                return -1;
            }
            return id;
        }
        id = ++_index;
        RedPointNode childNode = AddNode(id, key, parentNode.id);
        parentNode.AddChild(childNode);
        return id;
    }

    /// <summary>
    /// 动态添加一个有红点功能标签：例如道具背包中的每个item上的New功能。
    /// 此时，每个Item都应该为一个叶子节点，此时只需为每个Item设置一个独有的Key
    /// </summary>
    /// <param name="fullKey">Tag应该具有完整的父子级关系, 需要从根节点开始</param>
    public int AddDynamicFullKey(string fullKey)
    {
        string[] splits = fullKey.Split('.');
        RedPointNode node = rootNode;
        if (!splits[0].Equals(node.key))
        {
            Debug.LogErrorFormat("<RedPointMgr> {0}需要从根节点{1}开始", fullKey, node.key);
            return -1;
        }
        int leafNodeId = 0;
        for (int j = 1; j < splits.Length; j++)
        {
            string key = splits[j];
            //节点存在，继续查找
            if(node.childDicByKey.TryGetValue(key, out leafNodeId))
            {
                node = nodeDic[leafNodeId];
                continue;
            }
            //节点不存在，需要创建一个节点
            leafNodeId = ++_index;
            RedPointNode childNode = AddNode(leafNodeId, key, node.id);
            node.AddChild(childNode);
            node = childNode;
        }
        if (!node.IsLeafNode())
        {
            Debug.LogErrorFormat("<RedPointMgr> {0}不是叶子节点！", fullKey);
            return -1;
        }
        return leafNodeId;
    }

    /// <summary>
    /// 修改指定节点的红点数，默认只能修改叶子节点的数量
    /// </summary>
    /// <param name="checkEndNode">检查节点是否为叶子节点，默认为true.</param>
    public void SetPointNum(int id, int num, bool checkEndNode = true)
    {
        RedPointNode node = nodeDic[id];
        if(checkEndNode && !node.IsLeafNode())
        {
            Debug.LogErrorFormat("<RedPointMgr> {0}节点不是叶子节点，请检查是否要进行此操作！", node.key);
            return;
        }
        node.SetPointNum(num);
    }
    public void SetPointNum(string fullKey, int num, bool checkEndNode = true)
    {
        SetPointNum(GetIdByKey(fullKey), num, checkEndNode);
    }

    public int GetPointNum(int id)
    {
        RedPointNode node = nodeDic[id];
        return node.pointNum;
    }
}
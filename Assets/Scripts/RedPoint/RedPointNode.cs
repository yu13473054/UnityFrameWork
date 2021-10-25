using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 节点对象
/// </summary>
public class RedPointNode
{
    public string key; //节点的名称
    public int id;
    public int parentId;
    public int pointNum; //具体的红点数量
    public Dictionary<string, int> childDicByKey
    {
        get;
        private set;
    }
    public List<int> childs { get; private set; }

    public RedPointNode(int id, string key, int parentId, List<int> childs = null)
    {
        this.key = key;
        this.id = id;
        this.parentId = parentId;
        this.childs = childs == null ? new List<int>() : childs;
        childDicByKey = new Dictionary<string, int>();
    }

    public RedPointNode(RedPointAssetCfg cfg) : this(cfg.id, cfg.key, cfg.parentId)
    {
        this.childs.AddRange(cfg.childs);
    }

    //构建字典表，方便查找
    public void BuildChildDic()
    {
        for (int i = 0; i < childs.Count; i++)
        {
            int childId = childs[i];
            string childKey = RedPointMgr.Inst.nodeDic[childId].key;
            if (childDicByKey.ContainsKey(childKey))
            {
                Debug.LogErrorFormat("<RedPointNode> 节点：{0}中，有相同Key的子节点：{1}", key, childKey);
            }
            else
                childDicByKey.Add(childKey, childId);
        }
    }

    //添加子节点
    public void AddChild(RedPointNode child)
    {
        childs.Add(child.id);
        if (childDicByKey.ContainsKey(child.key))
        {
            Debug.LogErrorFormat("<RedPointNode> 节点：{0}中，有相同Key的子节点：{1}", key, child.key);
        }
        else
            childDicByKey.Add(child.key, child.id);
    }

    /// <summary>
    /// 触发事件，通知修改了数量
    /// </summary>
    /// <param name="recursion">是否递归触发父级的Trigger。默认为true</param>
    public void TriggerEvent(bool recursion = true)
    {
        EventMgr.Inst.RedPointEvt.TriggerEvent<int>(id, pointNum);
        if (parentId > 0 && recursion)
            RedPointMgr.Inst.nodeDic[parentId].TriggerEvent(recursion);
    }

    /// <summary>
    /// 强行设置节点的红点数
    /// </summary>
    /// <param name="isTrigger">是否Trigger事件</param>
    public void SetPointNum(int num, bool isTrigger = true)
    {
        if (pointNum == num) return;
        if (parentId > 0)//不是根节点
        {
            int changeNum = num - pointNum;
            RedPointNode parentNode = RedPointMgr.Inst.nodeDic[parentId];
            parentNode.SetPointNum(parentNode.pointNum + changeNum, isTrigger);
        }
        pointNum = num;
        if (isTrigger)
            TriggerEvent(false);
    }

    //是否为叶子节点
    public bool IsLeafNode()
    {
        return childs.Count == 0;
    }
}
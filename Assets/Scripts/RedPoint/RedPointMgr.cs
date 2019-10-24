using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 红点管理器，负责根据配置的Tag生成树结构。当红点数量有变化时，使用事件系统进行红点数量变化的通知。
/// Tag可以为固定Tag，例如活动页；也可以为动态Tag，例如道具背包中的每个Item
/// </summary>
public class RedPointMgr
{
    /// <summary>
    /// 节点对象
    /// </summary>
    public class Node
    {
        public string name; //节点的名称，也是Tigger的名称
        public int pointNum; //具体的红点数量
        public Node parent;
        private List<Node> _childs = new List<Node>();

        public void AddChild(Node child)
        {
            if (_childs.Contains(child)) return; //不能重复添加
            _childs.Add(child);
        }

        /// <summary>
        /// 触发事件，通知修改了数量
        /// </summary>
        /// <param name="recursion">是否递归触发父级的Trigger。默认为true</param>
        public void TriggerEvent(bool recursion = true)
        {
            EventMgr.Inst.RedPointkEvt.TriggerEvent<int>(name, pointNum); 
            if (parent != null && recursion)
                parent.TriggerEvent(recursion);
        }

        /// <summary>
        /// 强行设置节点的红点数
        /// </summary>
        /// <param name="isTrigger">是否Trigger事件</param>
        public void SetPointNum(int num, bool isTrigger = true)
        {
            if (pointNum == num) return;
            if(parent != null)//为null说明是root节点
            {
                int changeNum = num - pointNum;
                parent.SetPointNum(parent.pointNum + changeNum, isTrigger);
            }
            pointNum = num;
            if (isTrigger)
                TriggerEvent(false);
        }

        //是否为叶子节点
        public bool IsEndNode()
        {
            return _childs.Count == 0;
        }
    }

    //只需要配置每个叶子节点的tag，生成结点时，会自动生成父节点信息
    private readonly string[] PointTags = {
        "Mian.Mail.System",
        "Mian.Mail.Team",
        "Mian.Mail.Notice",
    };

    private static RedPointMgr _inst;
    public static RedPointMgr Inst
    {
        get 
        {
            if (_inst == null) _inst = new RedPointMgr();
            return _inst; 
        }
    }

    private Dictionary<string, Node> _nodeDic;

    public void Init()
    {
        _nodeDic = new Dictionary<string, Node>();
        //初始化红点结构
        for(int i = 0; i<PointTags.Length; i++)
        {
            string tag = PointTags[i];
            ParseTag(tag);
        }
    }

    //解析一个标签
    private void ParseTag(string tag)
    {
        string[] splits = tag.Split('.');
        for (int j = splits.Length - 1; j >= 0; j--)
        {
            string name = splits[j];
            Node node;
            if (!_nodeDic.TryGetValue(name, out node))
            {
                node = new Node()
                {
                    name = name
                };
                _nodeDic.Add(name, node);
            }
            if (node.parent != null) break;//父节点的信息已经设置过，后续的节点直接跳过
            if (j != splits.Length - 1)
            {
                Node childNode = _nodeDic[splits[j + 1]];
                childNode.parent = node;//设置parent
                node.AddChild(childNode);//将自己设置为child节点，方法本身会剔重
            }
            else //叶子节点
            {
                Node parentNode;
                if (_nodeDic.TryGetValue(splits[j - 1], out parentNode))
                {
                    node.parent = parentNode;
                    parentNode.AddChild(node);
                }
            }
        }
    }

    /// <summary>
    /// 动态添加一个有红点功能标签：例如道具背包中的每个item上的New功能。
    /// 此时，每个Item都应该为一个叶子节点，此时只需为每个Item设置一个独有的Name即可
    /// </summary>
    /// <param name="tag">Tag应该具有完整的父子级关系</param>
    public void AddDynamicTag(string tag)
    {
        ParseTag(tag);
    }

    /// <summary>
    /// 修改指定节点的红点数，默认只能修改叶子节点的数量
    /// </summary>
    /// <param name="checkEndNode">检查节点是否为叶子节点，默认为true.</param>
    public void SetPointNum(string name, int num, bool checkEndNode = true)
    {
        Node node = _nodeDic[name];
        if(checkEndNode && !node.IsEndNode())
        {
            Debug.LogErrorFormat("<RedPointMgr> {0}节点不是叶子节点，请检查是否要进行此操作！", name);
            return;
        }
        node.SetPointNum(num);
    }

    public int GetPointNum(string name)
    {
        Node node = _nodeDic[name];
        return node.pointNum;
    }
}

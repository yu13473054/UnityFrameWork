using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RedPointAssetCfgViewItem
{
    private RedPointAssetCfg _node;
    private RedPointAssetInspector _main;

    public RedPointAssetCfgViewItem(RedPointAssetInspector main, RedPointAssetCfg node)
    {
        _main = main;
        _node = node;
    }

    public void OnInspectorGUI(float leftSpace)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Space(leftSpace);

            var vs = _node.isShowChild ? new GUILayout.VerticalScope("box") : new GUILayout.VerticalScope();
            using (vs)
            {
                using (new GUILayout.HorizontalScope())
                {
                    float oldWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(new GUIContent("key:")).x + 3;
                    _node.key = EditorGUILayout.TextField("key:", _node.key, GUILayout.Width(100));
                    EditorGUIUtility.labelWidth = oldWidth;

                    GUILayout.Space(20);

                    if (_node.childs.Count > 0)
                    {
                        bool oldStyle = EditorStyles.foldout.stretchWidth;
                        EditorStyles.foldout.stretchWidth = false;
                        _node.isShowChild = EditorGUILayout.Foldout(_node.isShowChild, "child", true);
                        EditorStyles.foldout.stretchWidth = oldStyle;
                    }

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("+", GUILayout.Width(20)))
                    {
                        AddNode();
                    }
                    if (leftSpace > 0) //根节点不显示删除按钮
                    {
                        if(GUILayout.Button("p", GUILayout.Width(20)))
                        {
                            _main.ShowParentList(_node.id, OnSelectByMenu);
                        }

                        if(GUILayout.Button("-", GUILayout.Width(20)))
                        {
                            string txt = _node.childs.Count > 0 ? "删除节点时，相关子节点也将被删除！" : "是否删除该节点？";
                            if (EditorUtility.DisplayDialog("提示", txt, "确定", "删除"))
                            {
                                DelNode();
                            }
                        }
                    }
                }

                if (!_main.isValid(_node.id)) //删除数据后，数据就无效了，不用在绘制ui了
                {
                    return;
                }

                for (int i = 0; i < _node.childs.Count; i++)
                {
                    int id = _node.childs[i];
                    if (_node.isShowChild)
                    {
                        RedPointAssetCfgViewItem view = _main.GetNodeView(id);
                        view.OnInspectorGUI(leftSpace + 5);
                    }
                }
                // 没有child后，直接隐藏
                if (_node.isShowChild && _node.childs.Count == 0)
                    _node.isShowChild = false;

            }
        }
    }

    private void AddNode()
    {
        RedPointAssetCfg newNode = _main.AddNodeCfg();
        newNode.parentId = _node.id; //记录父节点
        _node.childs.Add(newNode.id);
        _node.isShowChild = true;
    }

    private void DelNode()
    {
        //删除本身数据
        _main.RemoveNodeCfg(_node.id);
        _main.RebuildList();
        //从父节点中删除本身
        DelNodeFromParent();
    }

    private void DelNodeFromParent()
    {
        RedPointAssetCfg parentCfg = _main.GetNodeCfg(_node.parentId);
        for (int i = 0; i < parentCfg.childs.Count; i++)
        {
            int id = parentCfg.childs[i];
            if (_node.id == id)
            {
                parentCfg.childs.RemoveAt(i);
                break;
            }
        }
    }

    //修改节点的父级
    private void ChangeNodeParent(int newParentId)
    {
        DelNodeFromParent();

        //直接修改ParentId
        _node.parentId = newParentId;
        //加入parentId所在node的child中
        RedPointAssetCfg cfg = _main.GetNodeCfg(newParentId);
        cfg.childs.Add(_node.id);
        cfg.isShowChild = true;
    }

    private void OnSelectByMenu(object data)
    {
        int newParentId = (int)data;
        ChangeNodeParent(newParentId);
    }
}

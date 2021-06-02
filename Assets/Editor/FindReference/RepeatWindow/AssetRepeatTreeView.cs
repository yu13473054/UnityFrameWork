using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace AssetDanshari
{
    public class AssetRepeatTreeView : AssetTreeView
    {
        private AssetRepeatModel model { get; set; }

        public AssetRepeatTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, AssetModel model) : base(state, multiColumnHeader, model)
        {
            this.model = model as AssetRepeatModel;
        }

        protected override void CellGUI(Rect cellRect, AssetTreeViewItem<AssetModel.AssetInfo> item, int column, ref RowGUIArgs args)
        {
            var info = item.data;
            switch (column)
            {
                case 0:
                    DrawItemWithIcon(cellRect, item, ref args, info.displayName, info.fileRelativePath, true, false, info.deleted);
                    break;
                case 1:
                    DefaultGUI.Label(cellRect, info.fileRelativePath, args.selected, args.focused);
                    break;
            }
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override void ContextClickedItem(int id)
        {
            var item = FindItem(id, rootItem) as AssetTreeViewItem<AssetModel.AssetInfo>;
            var assetInfo = GetItemAssetInfo(item);
            if (item == null || assetInfo == null || item.data.deleted || item.data.isRst)
            {
                return;
            }

            GenericMenu menu = new GenericMenu();
            menu.AddItem(AssetDanshariStyle.Get().locationContext, false, OnLocate, id);
            menu.AddItem(AssetDanshariStyle.Get().explorerContext, false, OnContextExplorerActiveItem, item);
            menu.AddSeparator(String.Empty);
            foreach (var dir in AssetDanshariUtility.CommonDir)
            {
                menu.AddItem(new GUIContent(AssetDanshariStyle.Get().duplicateContextMoveComm + dir.displayName), false, OnContextMoveItem, dir.fileRelativePath);
            }
            menu.AddSeparator(String.Empty);
            menu.AddItem(AssetDanshariStyle.Get().duplicateContextOnlyUseThis, false, OnContextUseThisItem, item);
            menu.ShowAsContext();
        }

        /// <summary>
        /// 去引用到的目录查找所有用到的guid，批量更改
        /// </summary>
        private void OnContextUseThisItem(object userdata)
        {
            var item = userdata as AssetTreeViewItem<AssetModel.AssetInfo>;
            if (item != null)
            {
                AssetModel.AssetInfo group = ((AssetTreeViewItem<AssetModel.AssetInfo>) item.parent).data;
                AssetModel.AssetInfo useInfo = item.data;
                var style = AssetDanshariStyle.Get();
                if (!EditorUtility.DisplayDialog(String.Empty, style.sureStr + style.duplicateContextOnlyUseThis.text,
                    style.sureStr, style.cancelStr))
                {
                    return;
                }

                Dictionary<string, List<string>> beReplaceDic = new Dictionary<string, List<string>>();
                for (int i = group.children.Count - 1; i >= 0; i--)
                {
                    var info = group.children[i];
                    if (info != useInfo)
                    {
                        //存储依赖info的资源路径
                        if (info.hasChildren)
                        {
                            List<string> depList = new List<string>();
                            for (int j = 0; j < info.children.Count; j++)
                            {
                                depList.Add(info.children[j].fileRelativePath);
                            }
                            beReplaceDic.Add(info.fileRelativePath, depList);
                        }
                        AssetDatabase.DeleteAsset(info.fileRelativePath);
                        info.deleted = true;
                    }
                }
                ((AssetRepeatModel)m_Model).SetUseThis(useInfo.fileRelativePath, beReplaceDic);
                EditorUtility.DisplayDialog(String.Empty, style.progressFinish, style.sureStr);
                AssetDatabase.Refresh();
                Repaint();
            }
        }

        private void OnContextMoveItem(object userdata)
        {
            if (!HasSelection())
            {
                return;
            }

            var selects = GetSelection();
            foreach (var select in selects)
            {
                TreeViewItem item = FindItem(select, rootItem);
                var assetInfo = GetItemAssetInfo(item);
                if (assetInfo == null || assetInfo.deleted)
                {
                    continue;
                }

                var dirPath = userdata as string;

                var style = AssetDanshariStyle.Get();
                string destPath = String.Format("{0}/{1}", dirPath, assetInfo.displayName);
                if (assetInfo.fileRelativePath != destPath)
                {
                    var errorStr = AssetDatabase.MoveAsset(assetInfo.fileRelativePath, destPath);
                    if (!string.IsNullOrEmpty(errorStr))
                    {
                        EditorUtility.DisplayDialog(style.errorTitle, errorStr, style.sureStr);
                    }
                    else
                    {
                        assetInfo.fileRelativePath = destPath;
                    }
                }

                OnContextUseThisItem(item);

            }
        }

    }
}
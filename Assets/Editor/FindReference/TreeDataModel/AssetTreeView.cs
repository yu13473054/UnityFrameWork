using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace AssetDanshari
{
    public class AssetTreeView : TreeView
    {
        protected AssetModel m_Model;
        private List<TreeViewItem> m_WatcherItems = new List<TreeViewItem>();

        public AssetTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, AssetModel model) : base(state, multiColumnHeader)
        {
            m_Model = model;
            rowHeight = 20f;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            multiColumnHeader.height = 23f;
        }

        public virtual void Destroy()
        {
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = -1, depth = -1, displayName = "Root" };

            if (m_Model != null && m_Model.data != null)
            {
                foreach (var info in m_Model.data)
                {
                    BuildDataDir(0, info, root);
                }
            }

            SetupDepthsFromParentsAndChildren(root);
            SortTreeViewNaturalCompare(root);
            return root;
        }

        protected virtual void BuildDataDir(int depth, AssetModel.AssetInfo dirInfo, TreeViewItem parent)
        {
            var dirItem = new AssetTreeViewItem<AssetModel.AssetInfo>(dirInfo.id, depth, dirInfo.displayName, dirInfo);
            parent.AddChild(dirItem);

            if (dirInfo.hasChildren)
            {
                foreach (var childInfo in dirInfo.children)
                {
                    BuildDataDir(depth + 1, childInfo, dirItem);
                }
            }
        }

        protected virtual AssetModel.AssetInfo GetItemAssetInfo(TreeViewItem item)
        {
            var item2 = item as AssetTreeViewItem<AssetModel.AssetInfo>;
            if (item2 != null)
            {
                return item2.data;
            }

            return null;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as AssetTreeViewItem<AssetModel.AssetInfo>;
            if (item != null)
            {
                for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                {
                    CellGUI(args.GetCellRect(i), item, args.GetColumn(i), ref args);
                }
                return;
            }

            base.RowGUI(args);
        }

        protected virtual void CellGUI(Rect cellRect, AssetTreeViewItem<AssetModel.AssetInfo> item, int column,
            ref RowGUIArgs args)
        {
        }

        protected void DrawItemWithIcon(Rect cellRect, TreeViewItem item, ref RowGUIArgs args,
            string displayName, string fileRelativePath, bool contentIndent = true, bool foldoutIndent = false, bool deleted = false)
        {
            if (contentIndent)
            {
                float num = GetContentIndent(item);
                cellRect.xMin += num;
            }

            if (foldoutIndent)
            {
                float num = GetFoldoutIndent(item);
                cellRect.xMin += num;
            }

            Rect position = cellRect;
            position.width = 16f;
            position.height = 16f;
            position.y += 2f;
            Texture iconForItem = item.icon;
            if (iconForItem == null)
            {
                iconForItem = AssetDatabase.GetCachedIcon(fileRelativePath);
                if (iconForItem)
                {
                    item.icon = iconForItem as Texture2D;
                }
            }
            if (iconForItem)
            {
                GUI.DrawTexture(position, iconForItem, ScaleMode.ScaleToFit);
                item.icon = iconForItem as Texture2D;
            }

            cellRect.xMin += 18f;
            DefaultGUI.Label(cellRect, displayName, args.selected, args.focused);

            if (deleted)
            {
                position.x = cellRect.xMax - 40f;
                position.y += 3f;
                position.height = 9f;
                position.width = 40f;
                GUI.DrawTexture(position, AssetDanshariStyle.Get().iconDelete.image, ScaleMode.ScaleToFit);
            }
        }

        protected override void DoubleClickedItem(int id)
        {
            var assetInfo = GetItemAssetInfo(FindItem(id, rootItem));
            if (assetInfo == null)
            {
                return;
            }
            string fileRelativePath = assetInfo.fileRelativePath;
            if (string.IsNullOrEmpty(fileRelativePath))
            {
                return;
            }
            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(fileRelativePath);
            if (obj)
            {
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            }
        }

        protected void OnLocate(object userdata)
        {
            DoubleClickedItem((int)userdata);
        }

        protected void OnContextExplorerActiveItem(object userdata)
        {
            var assetInfo = GetItemAssetInfo((TreeViewItem)userdata);
            if (assetInfo == null || string.IsNullOrEmpty(assetInfo.fileRelativePath))
            {
                return;
            }

            EditorUtility.RevealInFinder(assetInfo.fileRelativePath);
        }

        public bool IsExtraItem(int id)
        {
            var assetInfo = GetItemAssetInfo(FindItem(id, rootItem));
            if (assetInfo != null)
            {
                return assetInfo.isRst;
            }
            return false;
        }

        /// <summary>
        /// 选中包括了结果显示项
        /// </summary>
        /// <returns></returns>
        public bool IsSelectionContainsRstItem()
        {
            var selects = GetSelection();
            foreach (var select in selects)
            {
                if (IsExtraItem(select))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 是否选中了多个
        /// </summary>
        /// <returns></returns>
        public bool IsSelectionMulti()
        {
            var selects = GetSelection();
            return selects.Count > 1;
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="sortFromThisItem"></param>
        public void SortTreeViewNaturalCompare(TreeViewItem sortFromThisItem)
        {
            if (sortFromThisItem == null)
            {
                return;
            }

            if (sortFromThisItem.hasChildren)
            {
                sortFromThisItem.children.Sort((a, b) => EditorUtility.NaturalCompare(a.displayName, b.displayName));
                foreach (var child in sortFromThisItem.children)
                {
                    SortTreeViewNaturalCompare(child);
                }
            }
        }
    }
}
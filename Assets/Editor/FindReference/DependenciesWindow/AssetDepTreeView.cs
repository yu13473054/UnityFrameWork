using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace AssetDanshari
{
    public class AssetDepTreeView : AssetTreeView
    {
        private AssetDepModel model { get; set; }

        public AssetDepTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, AssetModel model) : base(state, multiColumnHeader, model)
        {
            this.model = model as AssetDepModel;
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        protected override void CellGUI(Rect cellRect, AssetTreeViewItem<AssetModel.AssetInfo> item, int column, ref RowGUIArgs args)
        {
            var info = item.data;

            switch (column)
            {
                case 0:
                    if (!info.isRst)
                    {
                        DrawItemWithIcon(cellRect, item, ref args, info.displayName, info.fileRelativePath);
                    }
                    break;
                case 1:
                    if (info.isRst)
                    {
                        DrawItemWithIcon(cellRect, item, ref args, info.displayName, info.fileRelativePath, false);
                    }
                    else
                    {
                        if (info.hasChildren && info.children.Count > 0)
                        {
                            DefaultGUI.Label(cellRect, info.children.Count.ToString(), args.selected, args.focused);
                        }
                    }
                    break;
            }
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return true;
        }

        protected override void ContextClickedItem(int id)
        {
            var item = FindItem(id, rootItem);
            var assetInfo = GetItemAssetInfo(item);
            if (item == null || assetInfo == null)
            {
                return;
            }

            GenericMenu menu = new GenericMenu();
            if (!IsSelectionMulti())
            {
                menu.AddItem(AssetDanshariStyle.Get().locationContext, false, OnLocate, id);
                menu.AddItem(AssetDanshariStyle.Get().explorerContext, false, OnContextExplorerActiveItem, item);
            }

            if (!IsSelectionContainsRstItem())
            {
                menu.AddSeparator(String.Empty);
                menu.AddSeparator(String.Empty);
                menu.AddItem(AssetDanshariStyle.Get().dependenciesDelete, false, OnContextDeleteThisItem);
            }

            if (menu.GetItemCount() > 0)
            {
                menu.ShowAsContext();
            }
        }

        private void OnContextDeleteThisItem()
        {
            if (!HasSelection())
            {
                return;
            }

            var style = AssetDanshariStyle.Get();
            if (!EditorUtility.DisplayDialog(String.Empty, style.sureStr + style.dependenciesDelete.text,
                style.sureStr, style.cancelStr))
            {
                return;
            }

            var selects = GetSelection();
            foreach (var select in selects)
            {
                var assetInfo = GetItemAssetInfo(FindItem(select, rootItem));
                if (assetInfo == null)
                {
                    continue;
                }

                if (AssetDatabase.DeleteAsset(assetInfo.fileRelativePath))
                {
                    m_Model.data.Remove(assetInfo);
                }
            }
            Reload();
            SetExpanded(rootItem.id, false);
            SetExpanded(rootItem.id, true);
            Repaint();
        }

        public void SetFilterEmpty(bool filterEmpty)
        {
            Reload();
            if (filterEmpty)
            {
                // 剔除有被引用的
                if (rootItem.hasChildren)
                {
                    for (var i = rootItem.children.Count - 1; i >= 0; i--)
                    {
                        var child = rootItem.children[i];
                        var assetInfo = GetItemAssetInfo(child);
                        if (assetInfo.hasChildren || assetInfo.displayName.EndsWith(".spriteatlas"))
                        {
                            rootItem.children.RemoveAt(i);
                        }
                    }
                }
            }
            SetExpanded(rootItem.id, false);
            SetExpanded(rootItem.id, true);
            Repaint();
        }
    }
}
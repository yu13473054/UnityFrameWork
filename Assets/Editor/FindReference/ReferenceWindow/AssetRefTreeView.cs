using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace AssetDanshari
{
    public class AssetRefTreeView : AssetTreeView
    {
        private AssetRefModel model { get; set; }

        public AssetRefTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, AssetModel model) : base(state, multiColumnHeader, model)
        {
            this.model = model as AssetRefModel;
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
                case 2:
                    if (info.isRst)
                    {
                        DefaultGUI.Label(cellRect, info.bindObj as string, args.selected, args.focused);
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

            if (menu.GetItemCount() > 0)
            {
                menu.ShowAsContext();
            }
        }
    }
}
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

namespace AssetDanshari
{
    public class AssetRefWindow : AssetBaseWindow
    {
         //查找选中资源引用的对象
        [MenuItem("Assets/查找依赖哪些资源", false, 201)]
        static void FindReference()
        {
            List<string> queryFileList = AssetDanshariUtility.GetSelectAssets();
            AssetBaseWindow.CheckPaths<AssetRefWindow>(queryFileList);
        }

        private void Awake()
        {
            titleContent = AssetDanshariStyle.Get().referenceTitle;
            minSize = new Vector2(727f, 331f);
            _treeModel = new AssetRefModel();
            _treeModel.SetDataPaths(_queryList);
            _queryList = null;
        }

        protected override void InitTree(MultiColumnHeader multiColumnHeader)
        {
            m_AssetTreeView = new AssetRefTreeView(_treeViewState, multiColumnHeader, _treeModel);
        }

        protected override void DrawGUI(GUIContent waiting, bool expandCollapseComplex)
        {
            base.DrawGUI(AssetDanshariStyle.Get().referenceWaiting, true);
        }

        protected override MultiColumnHeaderState CreateMultiColumnHeader()
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = AssetDanshariStyle.Get().nameHeaderContent,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 280,
                    minWidth = 150,
                    autoResize = false,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = AssetDanshariStyle.Get().referenceHeaderContent2,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    width = 300,
                    minWidth = 100,
                    autoResize = false,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = AssetDanshariStyle.Get().referenceHeaderContent3,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    width = 100,
                    minWidth = 100,
                    autoResize = false,
                    allowToggleVisibility = true
                }
            };

            return new MultiColumnHeaderState(columns);
        }
    }
}
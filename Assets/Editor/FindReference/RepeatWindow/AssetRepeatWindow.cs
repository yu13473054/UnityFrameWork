using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

namespace AssetDanshari
{
    public class AssetRepeatWindow : AssetBaseWindow
    {
         //查找选中资源引用的对象
        [MenuItem("Assets/查找重复资源", false, 203)]
        static void FindReference()
        {
            List<string> queryFileList = AssetDanshariUtility.GetSelectAssets();
            AssetBaseWindow.CheckPaths<AssetRepeatWindow>(queryFileList);
        }

        private void Awake()
        {
            titleContent = AssetDanshariStyle.Get().duplicateTitle;
            minSize = new Vector2(727f, 331f);
            _treeModel = new AssetRepeatModel();
            _treeModel.SetDataPaths(_queryList);
            _queryList = null;
        }

        protected override void InitTree(MultiColumnHeader multiColumnHeader)
        {
            m_AssetTreeView = new AssetRepeatTreeView(_treeViewState, multiColumnHeader, _treeModel);
        }

        protected override void DrawGUI(GUIContent waiting, bool expandCollapseComplex)
        {
            base.DrawGUI(AssetDanshariStyle.Get().duplicateWaiting, true);
        }

        protected override MultiColumnHeaderState CreateMultiColumnHeader()
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = AssetDanshariStyle.Get().duplicateHeaderContent,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 300,
                    minWidth = 200,
                    autoResize = false,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = AssetDanshariStyle.Get().duplicateHeaderContent2,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    width = 400,
                    minWidth = 100,
                    autoResize = false,
                    allowToggleVisibility = true
                }
            };

            return new MultiColumnHeaderState(columns);
        }
    }
}
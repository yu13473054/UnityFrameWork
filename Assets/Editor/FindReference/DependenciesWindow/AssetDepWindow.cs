using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AssetDanshari
{
    public class AssetDepWindow : AssetBaseWindow
    {
        private bool m_FilterEmpty;

        //查找选中资源依赖的对象
        [MenuItem("Assets/查找被哪些资源依赖", false, 202)]
        static void FindDependence()
        {
            List<string> queryFileList = AssetDanshariUtility.GetSelectAssets();
            CheckPaths<AssetDepWindow>(queryFileList);
        }

        private void Awake()
        {
            titleContent = AssetDanshariStyle.Get().dependenciesTitle;
            minSize = new Vector2(727f, 331f);
            _treeModel = new AssetDepModel();
            _treeModel.SetDataPaths(_queryList);
            _queryList = null;
        }

        protected override void InitTree(MultiColumnHeader multiColumnHeader)
        {
            m_FilterEmpty = false;
            m_AssetTreeView = new AssetDepTreeView(_treeViewState, multiColumnHeader, _treeModel);
        }

        protected override void DrawGUI(GUIContent waiting, bool expandCollapseComplex)
        {
            base.DrawGUI(AssetDanshariStyle.Get().dependenciesWaiting, true);
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
                    headerContent = AssetDanshariStyle.Get().dependenciesHeaderContent2,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    width = 350,
                    minWidth = 100,
                    autoResize = false,
                    allowToggleVisibility = true
                }
            };

            return new MultiColumnHeaderState(columns);
        }

        protected override void DrawToolbarMore()
        {
            EditorGUI.BeginChangeCheck();
            m_FilterEmpty = GUILayout.Toggle(m_FilterEmpty, AssetDanshariStyle.Get().dependenciesFilter, EditorStyles.toolbarButton,
                GUILayout.Width(70f));
            if (EditorGUI.EndChangeCheck() && m_AssetTreeView != null)
            {
                (m_AssetTreeView as AssetDepTreeView).SetFilterEmpty(m_FilterEmpty);
            }
        }
    }
}
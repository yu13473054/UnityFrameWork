using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;

namespace AssetDanshari
{
    public abstract class AssetBaseWindow : EditorWindow
    {
        private bool _isInit;
        protected AssetModel _treeModel;
        protected TreeViewState _treeViewState;
        private MultiColumnHeaderState _multiColumnHeaderState;
        private SearchField m_SearchField;
        protected AssetTreeView m_AssetTreeView;
        private Action _onShow;

        public static List<string> _queryList;
        public static void CheckPaths<T>(List<string> queryFileList) where T : AssetBaseWindow
        {
            _queryList = queryFileList;
            var window = GetWindow<T>();
            window.Show();
        }

        void OnEnable()
        {
            _onShow = () => { m_AssetTreeView.ExpandAll(); };
        }


        private void OnDisable()
        {
            if (m_AssetTreeView != null)
            {
                m_AssetTreeView.Destroy();
            }
        }

        private void OnGUI()
        {
            Init();
            if (_treeModel.HasData())
            {
                DrawGUI(GUIContent.none, false);
                if (_onShow != null)
                {
                    _onShow();
                    _onShow = null;
                }
            }
        }

        private void Init()
        {
            if (_isInit) return;
            _isInit = true;

            _treeViewState = new TreeViewState();

            bool firstInit = _multiColumnHeaderState == null;
            var headerState = CreateMultiColumnHeader();
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(_multiColumnHeaderState, headerState))
                MultiColumnHeaderState.OverwriteSerializedFields(_multiColumnHeaderState, headerState);
            _multiColumnHeaderState = headerState;

            var multiColumnHeader = new MultiColumnHeader(headerState);
            if (firstInit)
            {
                multiColumnHeader.ResizeToFit();
            }

            m_SearchField = new SearchField();

            InitTree(multiColumnHeader);

            if (_treeModel.HasData())
            {
                m_AssetTreeView.Reload();
            }
        }

        protected abstract void InitTree(MultiColumnHeader multiColumnHeader);


        protected virtual void DrawGUI(GUIContent waiting, bool expandCollapseComplex)
        {
            var style = AssetDanshariStyle.Get();
            style.InitGUI();
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button(AssetDanshariStyle.Get().expandAll2, EditorStyles.toolbarButton, GUILayout.Width(70f)))
            {
                m_AssetTreeView.ExpandAll();
            }
            if (GUILayout.Button(AssetDanshariStyle.Get().collapseAll2, EditorStyles.toolbarButton, GUILayout.Width(70f)))
            {
                m_AssetTreeView.CollapseAll();
            }
            EditorGUI.BeginChangeCheck();   
            m_AssetTreeView.searchString = m_SearchField.OnToolbarGUI(m_AssetTreeView.searchString);
            if (EditorGUI.EndChangeCheck() && GUIUtility.keyboardControl == 0)
            {
                m_AssetTreeView.SetFocusAndEnsureSelectedItem();
            }
            DrawToolbarMore();
            EditorGUILayout.EndHorizontal();
            m_AssetTreeView.OnGUI(GUILayoutUtility.GetRect(0, 100000, 0, 100000));
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        protected abstract MultiColumnHeaderState CreateMultiColumnHeader();

        protected virtual void DrawToolbarMore()
        {
        }
    }
}

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

        public static List<string> _queryList;
        public static void CheckPaths<T>(List<string> queryFileList) where T : AssetBaseWindow
        {
            _queryList = queryFileList;
            var window = GetWindow<T>();
            window.Show();
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
            DrawGUI(GUIContent.none, false);
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

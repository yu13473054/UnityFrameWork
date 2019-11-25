using System;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEditor;
using System.Collections.Generic;

namespace AssetDanshari
{
    public static class AssetDanshariUtility
    {
        public const string Res_Sprite = "Assets/Data/resmap_sprite.txt";
        public const string Res_Pref = "Assets/Data/resmap_prefab.txt";
        public const string Res_Obj = "Assets/Data/resmap_obj.txt";

        //符合compareCB方法获得文件夹下所有文件 
        public static List<string> GetFileList(string dirPath, Func<string, bool> compareCB)
        {
            List<string> fileList = new List<string>();
            if (!Directory.Exists(dirPath)) return fileList;
            var allFiles = Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories);

            for (var i = 0; i < allFiles.Length; i++)
            {
                var file = allFiles[i];
                if (compareCB(file))
                {
                    fileList.Add(file.Replace('\\', '/'));
                }
            }
            return fileList;
        }

        public static bool ValidFile(string filePath)
        {
            if (!filePath.EndsWith(".meta") && !filePath.EndsWith(".DS_Store"))
                return true;
            return false;
        }

        /// <summary>
        /// 获取文件列表的GUID
        /// </summary>
        public static List<string> GuidFromFileList(List<string> fileList)
        {
            List<string> guidList = new List<string>();
            foreach (var file in fileList)
            {
                guidList.Add(AssetDatabase.AssetPathToGUID(file));
            }
            return guidList;
        }

        //生成非等长而为数组
        public static bool[][] ResultList(int fileCount, int guidCount)
        {
            var ret = new bool[fileCount][];
            for (int i = 0; i < fileCount; i++)
            {
                ret[i] = new bool[guidCount];
            }

            return ret;
        }

        public static List<string> GetSelectAssets()
        {
            UnityEngine.Object[] selecObjs = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets);
            List<string> tmpList = new List<string>();
            foreach (UnityEngine.Object obj in selecObjs)
            {
                string objPath = AssetDatabase.GetAssetPath(obj);
                if (AssetDatabase.IsValidFolder(objPath))
                {
                    tmpList.AddRange(AssetDanshariUtility.GetFileList(objPath, AssetDanshariUtility.ValidFile));
                }
                else
                {
                    tmpList.Add(objPath);
                }
            }
            return tmpList;
        }

        public static void DisplayThreadProgressBar(int totalFiles, int filesFinished)
        {
            string msg = String.Format(@"{0} ({1}/{2})", AssetDanshariStyle.Get().progressTitle,
                (filesFinished + 1).ToString(), totalFiles.ToString());
            EditorUtility.DisplayProgressBar(AssetDanshariStyle.Get().progressTitle, msg, (filesFinished + 1) * 1f / totalFiles);
        }
    }
}
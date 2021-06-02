using System;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace AssetDanshari
{
    public static class AssetDanshariUtility
    {
        public const string Res_Sprite = "Assets/Data/Data/resmap_sprite.txt";
        public const string Res_Pref = "Assets/Data/Data/resmap_prefab.txt";
        public const string Res_Obj = "Assets/Data/Data/resmap_obj.txt";

        public static int m_DataPathLen = Application.dataPath.Length - 6;

        public static string[] ResDir = {
            "Assets",
        };

        public static string[] ResPrefebDir = {
            "Assets/Res/Prefab",
        };

        public static AssetModel.AssetInfo[] CommonDir = {
            new AssetModel.AssetInfo(1, "Assets/Res/Atlas/Common/Bottom", "Bottom"), 
            new AssetModel.AssetInfo(1, "Assets/Res/Atlas/Common/Front", "Front"), 
        };

        //符合compareCB方法获得文件夹下所有文件 
        public static List<string> GetFileList(string dirPath, Func<string, bool> compareCB)
        {
            List<string> fileList = new List<string>();
            if (!Directory.Exists(dirPath)) return fileList;
            var allFiles = Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories).Where(compareCB).ToArray();

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

        /// <summary>
        /// 获取Asset下所有的文件
        /// </summary>
        public static List<string> GetAllFile(Func<string, bool> compareCB)
        {
            List<string> fileList = new List<string>();
            for (int i = 0; i < AssetDanshariUtility.ResDir.Length; i++)
            {
                fileList.AddRange(AssetDanshariUtility.GetFileList(AssetDanshariUtility.ResDir[i], compareCB));
            }
            return fileList;
        }

        /// <summary>
        /// 获取Asset下所有的文件
        /// </summary>
        public static List<string> GetAllPrefebFile( Func<string, bool> compareCB )
        {
            List<string> fileList = new List<string>();
            for ( int i = 0; i < AssetDanshariUtility.ResDir.Length; i++ )
            {
                fileList.AddRange( AssetDanshariUtility.GetFileList( AssetDanshariUtility.ResPrefebDir[i], compareCB ) );
            }
            return fileList;
        }

        //会引用的其他资源的Asset
        public static bool ValidFileHasRef(string filePath)
        {
            if (filePath.EndsWith(".txt") || filePath.EndsWith(".png") || filePath.EndsWith(".anim")
                || filePath.EndsWith(".mp3") || filePath.EndsWith(".wav")
                || filePath.EndsWith(".tga") || filePath.EndsWith(".jpg") || filePath.EndsWith(".lua")
                || filePath.EndsWith(".meta") || filePath.EndsWith(".DS_Store") || filePath.Contains(".svn")
                || filePath.EndsWith(".dll"))
                return false;
            return true;
        }

        //不会引用的其他资源的Asset：查找重复资源时
        public static bool ValidFileRepeat(string filePath)
        {
            if (filePath.EndsWith(".spriteatlas"))
                return false;
            return ValidFileHasRef(filePath);
        }

        public static bool ValidFile(string filePath)
        {
            if (filePath.EndsWith(".meta") || filePath.EndsWith(".DS_Store") || filePath.Contains(".svn"))
                return false;
            return true;
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
/// <summary>
/// 多语言资源引用打断
/// </summary>
public class MultiLngResDetach {
    const string LocalizationDir = "/Localization";
    const string TmpLocalizationDir = "/TmpLocalization";

    public static void StartDeal()
    {
        string srcPath = Application.dataPath + LocalizationDir;
        string destPath = System.Environment.CurrentDirectory + TmpLocalizationDir;
        if (Directory.Exists(destPath))
            UnityEditor.FileUtil.DeleteFileOrDirectory(destPath);

        ConfigHandler handler = ConfigHandler.Open(Application.streamingAssetsPath + "/config.txt");
        string buildLng = handler.ReadValue("BuildLng", "0");
        if (buildLng != "0")//指定语言
        {
            //将多语言资源保存一份
            UnityEditor.FileUtil.CopyFileOrDirectory(srcPath, destPath);

            //遍历出需要打入包的多语言类型
            string[] splits = buildLng.Split(',');
            Dictionary<int, int> lngDic = new Dictionary<int, int>();
            for (int i = 0; i < splits.Length; i++)
            {
                int lngType = Int32.Parse(splits[i]);
                if (lngType == 0 || lngType > ResmapUtility.LocalTypeList.Length) continue;
                lngDic.Add(lngType, lngType);
            }
            //剔除不需要的多语言
            for (int i = 0; i < ResmapUtility.LocalTypeList.Length; i++)
                if (!lngDic.ContainsKey(i + 1))
                    UnityEditor.FileUtil.DeleteFileOrDirectory(srcPath + "/" + ResmapUtility.LocalTypeList[i]);

            AssetDatabase.Refresh();
        }

    }

    public static void Revert()
    {
        ConfigHandler handler = ConfigHandler.Open(Application.streamingAssetsPath + "/config.txt");
        string buildLng = handler.ReadValue("BuildLng", "0");
        if (buildLng != "0") //非全语言
        {
            string srcPath = Application.dataPath + LocalizationDir;
            string destPath = System.Environment.CurrentDirectory + TmpLocalizationDir;
            UnityEditor.FileUtil.DeleteFileOrDirectory(srcPath);
            UnityEditor.FileUtil.MoveFileOrDirectory(destPath, srcPath);
            AssetDatabase.Refresh();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

public class FileEncrypt
{
    private static List<string> fileList = new List<string>();
    private static bool isInit = false;
    static void Init()
    {
        if (isInit) return;
        isInit = true;
        fileList.Clear();
        List<string> dirs = new List<string>
        {
            "Assets/Data"
        };
        //多语言资源
        string[] folders = AssetDatabase.GetSubFolders("Assets/Localization");
        for (int i = 0; i < folders.Length; i++)
        {
            string[] subFolders = AssetDatabase.GetSubFolders(folders[i]);
            for (int j = 0; j < subFolders.Length; j++)
            {
                string folderPath = subFolders[j];
                if (folderPath.Contains("/Data"))
                    dirs.Add(folderPath);
            }
        }
        for (int i = 0; i < dirs.Count; i++)
        {
            string[] resultFiles = Directory.GetFiles(dirs[i], "*.txt", SearchOption.AllDirectories);
            if (resultFiles.Length > 0)
            {
                fileList.AddRange(resultFiles);
            }
        }

        //固定路径的文件
        //        fileList.Add("Assets/Data/Chapter/view10003.txt");
    }

    public static void Encrypt()
    {
        Init();
        for (int i = 0; i < fileList.Count; i++)
        {
            string content = File.ReadAllText(fileList[i]);
            content = CommonUtils.EncryptTxt(content);
            File.WriteAllText(fileList[i], content);
        }
    }
    public static void Decrypt()
    {
        Encrypt();
    }
}

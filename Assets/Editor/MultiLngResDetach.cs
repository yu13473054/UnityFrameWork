using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
/// <summary>
/// 多语言资源引用打断
/// </summary>
public class MultiLngResDetach {
    private static DirInfo[] Sample =
    {
        new DirInfo("Assets/Localization/{0}/Audio"),
        new DirInfo("Assets/Localization/{0}/Font",null,new []{ "fontsettings", "TTF" }),
        new DirInfo("Assets/Localization/{0}/prefab"),
        new DirInfo("Assets/Localization/{0}/Sprites"),
    };

    [MenuItem("工具/多语言资源引用打断")]
    public static void Done()
    {
        List<DirInfo> configs = new List<DirInfo>();
        //创建规则列表
        string rootDir = "Assets/Localization";
        string[] subFolders = AssetDatabase.GetSubFolders(rootDir);
        for (int i = 0; i < subFolders.Length; i++)
        {
            string foldName = subFolders[i].Replace(rootDir + "/", "");
            for (int j = 0; j < Sample.Length; j++)
            {
                DirInfo sampleDirInfo = Sample[j];
                configs.Add(new DirInfo(string.Format(sampleDirInfo.dir,foldName), sampleDirInfo.fileters, sampleDirInfo.exts));
            }
        }
        //筛选出符合要求的文件
        List<string> fileList = new List<string>();
        for (int i = 0; i < configs.Count; i++)
        {
            GetFilesRecursion(fileList, configs[i]);
        }
        //复制生成新文件，删除旧文件
        List<string> newFileList = new List<string>();
        for (int i = 0; i < fileList.Count; i++)
        {
            string filePath = fileList[i];
            string dir = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string ext = Path.GetExtension(filePath);

            string newFilePath = dir + "/New__"+fileName+ext;
            File.Copy(filePath, newFilePath);
            File.Delete(filePath); //必须在创建新文件后，马上删除老文件，不然动态字体会有很多References
            newFileList.Add(newFilePath);
        }
        AssetDatabase.Refresh();
        //将文件改回原名称
        for (int i = 0; i < fileList.Count; i++)
        {
            string filePath = fileList[i];
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            //必须使用该接口，不然guid不会保存
            string result = AssetDatabase.RenameAsset(newFileList[i], fileName);
            if (!string.IsNullOrEmpty(result))
            {
                Debug.LogError(result);
            }
        }
    }


    /// 递归获取dirPath文件夹下所有文件，包括子文件夹中的文件
    static void GetFilesRecursion(List<string> resultList, DirInfo dirInfo)
    {
        if (!Directory.Exists(dirInfo.dir))
        {
            Debug.LogError("请检查路径是不是文件夹。" + dirInfo.dir);
            return;
        }

        string[] files = Directory.GetFiles(dirInfo.dir);
        for (int i = 0; i < files.Length; i++)
        {
            string filePath = files[i];
            //通用剔除
            if (filePath.EndsWith(".meta") || filePath.Contains(".svn")
                || filePath.Contains(".DS_Store"))
                continue;
            //按文件拓展名剔除
            if (dirInfo.exts != null)
            {
                bool isValid = false;
                for (int j = 0; j < dirInfo.exts.Length; j++)
                {
                    if (filePath.EndsWith(dirInfo.exts[j]))
                    {
                        isValid = true;
                        break;
                    }
                }
                if(!isValid) continue; //不满足文件类型
            }
            //按关键字剔除
            if (dirInfo.fileters != null)
            {
                bool isValid = false;
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                for (int k = 0; k < dirInfo.fileters.Length; k++)
                {
                    if (fileName.Contains(dirInfo.fileters[k]))
                    {
                        isValid = true;
                        break;
                    }
                }
                if (!isValid) continue; //不满足文件关键字
            }
            resultList.Add(filePath);
        }

        string[] childDirPaths = Directory.GetDirectories(dirInfo.dir);
        for (int i = 0; i < childDirPaths.Length; i++)
        {
            GetFilesRecursion(resultList, new DirInfo(childDirPaths[i], dirInfo.fileters, dirInfo.exts));
        }
    }

    static string NormalizePath(string sourcePath)
    {
        return sourcePath.Replace("\\", "/");
    }
}

public class DirInfo
{
    public string dir;//生效的文件夹
    public string[] fileters;//需要填入的标志字段
    public string[] exts;//需要填入的文件类型

    public DirInfo(string dir, string[] fileters = null, string[] exts = null)
    {
        this.dir = dir;
        this.fileters = fileters;
        this.exts = exts;
    }
}

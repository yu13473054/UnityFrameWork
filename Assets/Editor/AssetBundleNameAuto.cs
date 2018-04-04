using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
/// <summary>
/// 处理AB包的命名问题，默认处理Res文件夹下的内容
/// 1，默认打包策略使用Folder_All方式，支持到Res的一级子目录。
///    即Res/Sprites文件夹中，则按照以下规则：
///     2.1，如果二级是文件夹，则二级文件夹中的所有文件都打进一个AB包中，AB包的名称为sprites_文件夹名
///     2.2，如果二级是单个文件，则此文件单独生成一个AB包，AB包的名称为sprites_文件名
/// 3，可以在_AssetBundlePathList中定义具体的打包形式和路径，具体的策略参考BundleType
/// 4，Data和Lua文件的打包策略不在此范围内。
/// </summary>
public class AssetBundleNameAuto
{
    enum BundleType
    {
        Default,         // 将当前目录中每一个文件打成一个AB包，每一个文件夹打成一个ab包
        Folder,             // 将当前目录中的所有的内容打进一个AB包中
    }

    struct AssetBundlePath
    {
        public BundleType type;
        public string path;
        public string prefix;

        public AssetBundlePath( BundleType type, string path, string prefix = "" )
        {
            this.type = type;
            this.path = path;
            this.prefix = prefix;
        }
    }

    private static AssetBundlePath[] _specialPathList = new AssetBundlePath[]
    {
        new AssetBundlePath(BundleType.Default, "Assets/Res/Prefab/UI"),
    };

    private static Dictionary<string,List<string>> _abNameDic = new Dictionary<string, List<string>>();

    [MenuItem( "Builder/设置所有AssetBundleName", false, 201 )]
	public static void AssetBundleSetNames()
    {
        //先清理下所有的ABName
        ClearAllNames();

        string rootDir = "Assets/Res";
        //先将默认的Res按照策略处理一遍
        string[] childDirs = Directory.GetDirectories(rootDir);
        for (int i = 0; i < childDirs.Length; i++)
        {
            string childDirPath = childDirs[i];
            if (childDirPath.Equals(rootDir)) continue;

            string[] strs = childDirPath.Split('/','\\');
            string name = strs[strs.Length - 1];//将文件夹的名称作为前缀
            DefaultPolicy(childDirPath, name + "_");
        }

        //将自定义的文件处理一遍
        for (int i = 0; i < _specialPathList.Length; i++)
        {
            AssetBundlePath abPath = _specialPathList[i];
            switch (abPath.type)
            {
                case BundleType.Folder:
                    FolderPolicy(abPath.path, abPath.prefix);
                    break;
                case BundleType.Default:
                    DefaultPolicy(abPath.path, abPath.prefix);
                    break;
            }
        }

        //遍历是否有重复的abName
        foreach (var pairs in _abNameDic)
        {
            //有重名
            if (pairs.Value.Count>1)
            {
                string log = pairs.Key + "：\n";
                for (int i = 0; i < pairs.Value.Count; i++)
                {
                    log += pairs.Value[i] + "----\n";
                }
                log = log.TrimEnd('\n');
                Debug.LogWarning("AB的名称有重复，请检查"+log);
            }
        }
        _abNameDic.Clear();

        // 清理无用名称
        AssetDatabase.RemoveUnusedAssetBundleNames();
    }

    // type == Folder
    static void FolderPolicy(string path, string ABPrefix)
    {
        string[] strs = path.Split('/', '\\');
        string name = strs[ strs.Length - 1 ];
        SetABName(path,ABPrefix+name);
    }

    // type == Default
    static void DefaultPolicy(string path, string ABPrefix)
    {
        //处理文件夹
        string[] childDirs = Directory.GetDirectories(path);
        for (int j = 0; j < childDirs.Length; j++)
        {
            string thirdChildDirPath = childDirs[j];
            if(childDirs.Equals(thirdChildDirPath)) continue;

            FolderPolicy(thirdChildDirPath, ABPrefix);
        }

        //处理文件
        string[] childFiles = Directory.GetFiles(path);
        for (int j = 0; j < childFiles.Length; j++)
        {
            string file = childFiles[j];
            FileInfo fileInfo = new FileInfo(file);
            string name = fileInfo.Name.Replace(fileInfo.Extension,"");
            SetABName(file, ABPrefix+name);
        }
    }

    /// <summary>
    /// 将指定文件或者文件夹下的所有文件的AssetBundleName设置为指定名称
    /// </summary>
    static void SetABName(string path, string abName)
    {
        bool setNameSucc = false;
        abName = abName.ToLower();
        List<string> filesName = new List<string>();
        if (File.Exists(path))
        {
            filesName.Add(path);
        }
        else if (Directory.Exists(path))
        {
            GetFilesRecursion(filesName, path);
        }
        for (int i = 0; i < filesName.Count; i++)
        {
            FileInfo fileInfo = new FileInfo(filesName[i]);
            string extension = fileInfo.Extension;
            if (extension == ".meta" || extension == ".cs" || extension == ".js")
                continue;
            string assetPath = fileInfo.FullName.Substring(fileInfo.FullName.IndexOf("Assets")).Replace("\\", "/");
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer == null)
            {
                Debug.LogError("No Importer:" + assetPath);
                continue;
            }
            setNameSucc = true;
            importer.assetBundleName = abName;
        }

        if (setNameSucc)
        {
            //保存ABName，用于检查是否有重复的AB命名
            if (!_abNameDic.ContainsKey(abName))
            {
                _abNameDic.Add(abName,new List<string>());
            }
            _abNameDic[abName].Add(path);
        }
    }

    /// <summary>
    /// 递归获取dirPath文件夹下所有文件，包括子文件夹中的文件
    /// </summary>
    static void GetFilesRecursion(List<string> resultList, string dirPath)
    {
        if (!Directory.Exists(dirPath))
        {
            Debug.LogError("请检查路径是不是文件夹。"+dirPath);
            return;
        }
        resultList.AddRange(Directory.GetFiles(dirPath));
        string[] childDirPaths = Directory.GetDirectories(dirPath);
        for (int i = 0; i < childDirPaths.Length; i++)
        {
            GetFilesRecursion(resultList, childDirPaths[i]);
        }
    }

    // 清除所有名字
    [MenuItem("Builder/清除AssetBundleName", false, 202)]
    static void ClearAllNames()
    {
        List<string> filePathList = new List<string>();
        GetFilesRecursion(filePathList, "Assets");
        for (int i = 0; i < filePathList.Count; i++)
        {
            FileInfo file = new FileInfo(filePathList[i]);
			// 文本文件跳过
            if( file.Extension == ".meta" || file.Extension == ".cs" || file.Extension == ".js" )
                continue;

            string path = file.FullName.Substring( file.FullName.IndexOf( "Assets" ) ).Replace( "\\", "/" );
            
			AssetImporter importer = AssetImporter.GetAtPath ( path );
            if( importer != null )
            {
                importer.assetBundleName = "";
            }
            
        }
    }
}

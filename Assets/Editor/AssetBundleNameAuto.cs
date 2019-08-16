using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class AssetBundleNameAuto
{
    enum BundleType
    {
        Default,         // 将当前目录中每一个文件打成一个AB包，每一个文件夹中所有文件（包括子文件夹中的文件）打成一个AB包：Res文件夹的默认策略
        Folder,          // 将当前目录中的所有文件打进一个AB包中,子文件夹中的所有文件（包括子文件夹中的文件）打进另外一个ab包中：Data文件夹的默认策略
        FolderAll,       // 将当前目录中的所有文件（包括子文件夹中的文件）打进一个AB包中
    }

    struct AssetBundlePath
    {
        public BundleType type;
        public string path;

        public AssetBundlePath(BundleType type, string path)
        {
            this.type = type;
            this.path = path;
        }
    }

    private static List<AssetBundlePath> ruleList = new List<AssetBundlePath>();
    private static bool isInit = false;

    private static void ReInit()
    {
        isInit = false;
        Init();
    }
    private static void Init()
    {
        if(isInit) return;
        isInit = true;
        ruleList.Clear();
        //初始化默认资源的规则
        string[] folders = AssetDatabase.GetSubFolders("Assets/Res");
        for (int i = 0; i < folders.Length; i++)
        {
            ruleList.Add(new AssetBundlePath(BundleType.Default, folders[i]));
        }
        //多语言资源规则
        folders = AssetDatabase.GetSubFolders("Assets/Localization");
        for (int i = 0; i < folders.Length; i++)
        {
            string[] subFolders = AssetDatabase.GetSubFolders(folders[i]);
            for (int j = 0; j < subFolders.Length; j++)
            {
                string folderPath = subFolders[j];
                if(folderPath.Contains("/Data"))
                    ruleList.Add(new AssetBundlePath(BundleType.Folder, folderPath));
                else
                    ruleList.Add(new AssetBundlePath(BundleType.Default, folderPath));
            }
        }

        //越靠后，规则会覆盖前一个的
        ruleList.Add(new AssetBundlePath(BundleType.Folder, "Assets/Data"));
        ruleList.Add(new AssetBundlePath(BundleType.Folder, "Assets/Lua"));
        ruleList.Add( new AssetBundlePath(BundleType.Folder, "Assets/Res/Shader") );
        ruleList.Add(new AssetBundlePath(BundleType.Default, "Assets/Res/Prefab/UI"));
        ruleList.Add(new AssetBundlePath(BundleType.Default, "Assets/Res/Atlas/Common"));
        ruleList.Add(new AssetBundlePath(BundleType.FolderAll, "Assets/Res/AssetsUpdate"));
        ruleList.Add(new AssetBundlePath(BundleType.Default, "Assets/Res/Spine/Char"));
        ruleList.Add(new AssetBundlePath(BundleType.Folder, "Assets/Res/Char"));
        ruleList.Add(new AssetBundlePath(BundleType.Folder, "Assets/Res/FightTimeline"));
        ruleList.Add(new AssetBundlePath(BundleType.Default, "Assets/Res/FX/_Common"));
    }

    [MenuItem("打包/设置所有AssetBundleName", false, 201)]
    public static void SetAllABNames()
    {
        ReInit();
        for (int i = 0; i < ruleList.Count; i++)
        {
            AssetBundlePath abPath = ruleList[i];
            CustomDeal(abPath);
        }

        // 清理无用名称
        AssetDatabase.RemoveUnusedAssetBundleNames();
    }

    [MenuItem("打包/设置资源(Res)的AssetBundleName", false, 202)]
    public static void SetResABNames()
    {
        Init();
        for (int i = 0; i < ruleList.Count; i++)
        {
            AssetBundlePath abPath = ruleList[i];
            if (!abPath.path.Contains("/Data") && !abPath.path.Contains("/Lua"))
                CustomDeal(abPath);
        }
        DealLocalization();
        Debug.Log("设置资源(Res)的AssetBundleName");
    }

    [MenuItem("打包/设置数据(Data)的AssetBundleName", false, 203)]
    public static void SetDataABNames()
    {
        Init();
        for (int i = 0; i < ruleList.Count; i++)
        {
            AssetBundlePath abPath = ruleList[i];
            if (abPath.path.Contains("/Data"))
            {
                CustomDeal(abPath);
            }
        }
        DealLocalization();        
        Debug.Log("设置数据(Data)的AssetBundleName");
    }

    [MenuItem("打包/设置脚本(Lua)的AssetBundleName", false, 204)]
    public static void SetLuaABNames()
    {
        Init();
        for (int i = 0; i < ruleList.Count; i++)
        {
            AssetBundlePath abPath = ruleList[i];
            if (abPath.path.Contains("/Lua"))
            {
                CustomDeal(abPath);
            }
        }
        Debug.Log("设置脚本(Lua)的AssetBundleName");
    }

    [MenuItem( "打包/设置Shader的AssetBundleName", false, 204 )]
    public static void SetShaderABNames()
    {
        Init();
        for( int i = 0; i < ruleList.Count; i++ )
        {
            AssetBundlePath abPath = ruleList[i];
            if( abPath.path.Contains( "/Shader" ) )
            {
                CustomDeal( abPath );
            }
        }
        Debug.Log( "设置Shader的AssetBundleName" );
    }

    static void DealLocalization()
    {
        ConfigHandlerEditor handler = ConfigHandlerEditor.Open(Application.streamingAssetsPath + "/app.txt");
        string buildLng = handler.ReadValue("BuildLng", "0");
        if (buildLng != "0")//指定语言
        {
            //遍历出需要打入包的多语言类型
            string[] splits = buildLng.Split(',');
            Dictionary<int, int> lngDic = new Dictionary<int, int>();
            for (int i = 0; i < splits.Length; i++)
            {
                int lngType = Int32.Parse(splits[i]);
                if (lngType == 0 || lngType >= AutoResMap.LocalTypeList.Length) continue;
                lngDic.Add(lngType, lngType);
            }
            //剔除不需要的多语言
            for (int i = 0; i < AutoResMap.LocalTypeList.Length; i++)
            {
                if (!lngDic.ContainsKey(i + 1))
                {
                    ClearABNames("Assets/Localization/" + AutoResMap.LocalTypeList[i]);
                }
            }
        }
    }

    //自定义的处理策略
    private static void CustomDeal(AssetBundlePath abPath)
    {
        string prefix = abPath.path.Replace("Assets/", "").Replace("/", "_");
        switch (abPath.type)
        {
            case BundleType.FolderAll:
                FolderAllPolicy(abPath.path, prefix);
                break;
            case BundleType.Default:
                DefaultPolicy(abPath.path, prefix);
                break;
            case BundleType.Folder:
                FolderPolicy(abPath.path, prefix);
                break;
        }
    }

    [MenuItem("打包/清除所有AssetBundleName", false, 301)]
    public static void ClearAllABNames()
    {
        ReInit();
        for (int i = 0; i < ruleList.Count; i++)
        {
            AssetBundlePath abPath = ruleList[i];
            ClearABNames(abPath.path);
        }

        // 清理无用名称
        AssetDatabase.RemoveUnusedAssetBundleNames();
    }

    [MenuItem("打包/清除资源(Res)的AssetBundleName", false, 302)]
    public static void ClearResABNames()
    {
        Init();
        for (int i = 0; i < ruleList.Count; i++)
        {
            AssetBundlePath abPath = ruleList[i];
            if (!abPath.path.Contains("/Data") && !abPath.path.Contains("/Lua"))
                ClearABNames(abPath.path);
        }
        Debug.Log("清除资源(Res)的AssetBundleName");
    }
    [MenuItem("打包/清除数据(Data)的AssetBundleName", false, 303)]
    public static void ClearDataABNames()
    {
        Init();
        for (int i = 0; i < ruleList.Count; i++)
        {
            AssetBundlePath abPath = ruleList[i];
            if (abPath.path.Contains("/Data"))
                ClearABNames(abPath.path);
        }
        Debug.Log("清除数据(Data)的AssetBundleName");
    }
    [MenuItem("打包/清除脚本(Lua)的AssetBundleName", false, 304)]
    public static void ClearLuaABNames()
    {
        Init();
        for (int i = 0; i < ruleList.Count; i++)
        {
            AssetBundlePath abPath = ruleList[i];
            if (abPath.path.Contains("/Lua"))
                ClearABNames(abPath.path);
        }
        Debug.Log("清除脚本(Lua)的AssetBundleName");
    }
    private static void ClearABNames(string rootDir)
    {
        if (!Directory.Exists(rootDir)) return;
        //清理文件夹上的abName
        List<string> dirPathList = new List<string>();
        dirPathList.Add(rootDir);
        GetDirRecursion(dirPathList, rootDir);
        for (int i = 0; i < dirPathList.Count; i++)
        {
            string path = dirPathList[i];
            AssetImporter importer = AssetImporter.GetAtPath(path);
            if (importer != null)
            {
                importer.assetBundleName = "";
            }
        }
        //清理文件上的abName
        List<string> filePathList = new List<string>();
        GetFilesRecursion(filePathList, rootDir);
        for (int i = 0; i < filePathList.Count; i++)
        {
            string file = filePathList[i];
            AssetImporter importer = AssetImporter.GetAtPath(file);
            if (importer != null)
            {
                importer.assetBundleName = "";
            }
        }
    }

    // type == Folder
    static void FolderPolicy(string dirPath, string ABPrefix)
    {
        //根目录中的文件直接打入该目录的ab包中
        string[] files = GetFiles(dirPath);
        foreach (string filePath in files)
        {
            SetABName(filePath, ABPrefix);
        }

        string[] childDirs = AssetDatabase.GetSubFolders(dirPath);
        for (int j = 0; j < childDirs.Length; j++)
        {
            string thirdChildDirPath = childDirs[j];
            string[] strs = thirdChildDirPath.Split('/', '\\');
            string name = strs[strs.Length - 1];
            FolderAllPolicy(thirdChildDirPath, ABPrefix + "_" + name);
        }
    }

    // type == FolderAll：此时传递的名称前缀就是ab包的名称
    static void FolderAllPolicy(string dirPath, string ABPrefix)
    {
        SetABName(dirPath, ABPrefix);
    }

    // type == Default
    static void DefaultPolicy(string dirPath, string ABPrefix)
    {
        //处理文件夹
        string[] childDirs = AssetDatabase.GetSubFolders(dirPath);
        for (int j = 0; j < childDirs.Length; j++)
        {
            string thirdChildDirPath = childDirs[j];
            string[] strs = thirdChildDirPath.Split('/', '\\');
            string name = strs[strs.Length - 1];
            FolderAllPolicy(thirdChildDirPath, ABPrefix + "_" + name);
        }

        //处理文件
        string[] childFiles = GetFiles(dirPath);
        for (int j = 0; j < childFiles.Length; j++)
        {
            string file = childFiles[j];
            FileInfo fileInfo = new FileInfo(file);
            string name = fileInfo.Name.Replace(fileInfo.Extension, "");
            SetABName(file, ABPrefix + "_" + name);
        }
    }

    /// 将指定文件或者文件夹下的所有文件的AssetBundleName设置为指定名称
    static void SetABName(string path, string abName)
    {
        //先清理一遍，防止默认策略已经对其中的文件进行过命名
        if (Directory.Exists(path))
        {
            ClearABNames(path);
        }

        abName = abName.ToLower();
        AssetImporter importer = AssetImporter.GetAtPath(path);
        if (importer == null)
        {
            Debug.LogError("No Importer:" + path);
        }
        importer.assetBundleName = abName;
    }

    /// 递归获取dirPath文件夹下所有文件，包括子文件夹中的文件
    static void GetFilesRecursion(List<string> resultList, string dirPath)
    {
        if (!Directory.Exists(dirPath))
        {
            Debug.LogError("请检查路径是不是文件夹。" + dirPath);
            return;
        }

        string[] files = GetFiles(dirPath);
        resultList.AddRange(files);
        string[] childDirPaths = Directory.GetDirectories(dirPath);
        for (int i = 0; i < childDirPaths.Length; i++)
        {
            GetFilesRecursion(resultList, childDirPaths[i]);
        }
    }

    /// 递归获取dirPath文件夹下所有文件夹，包括子文件夹中的文件夹
    static void GetDirRecursion(List<string> resultList, string dirPath)
    {
        if (!Directory.Exists(dirPath))
        {
            Debug.LogError("请检查路径是不是文件夹。" + dirPath);
            return;
        }
        string[] childDirPaths = AssetDatabase.GetSubFolders(dirPath);
        resultList.AddRange(childDirPaths);
        for (int i = 0; i < childDirPaths.Length; i++)
        {
            GetDirRecursion(resultList, childDirPaths[i]);
        }
    }

    static List<string> tmpList = new List<string>();
    static string[] GetFiles(string dirPath)
    {
        if(!Directory.Exists(dirPath)) return new string[0];
        string[] files = Directory.GetFiles(dirPath);
        for (int i = 0; i < files.Length; i++)
        {
            string filePath = files[i];
            if (filePath.EndsWith(".meta") || filePath.EndsWith(".cs") 
                || filePath.EndsWith(".js") || filePath.Contains(".svn")
                || filePath.Contains(".DS_Store"))
                continue;
            tmpList.Add(filePath);
        }
        files = tmpList.ToArray();
        tmpList.Clear();
        return files;
    }

    //获取指定资源的ab名称
    public static string GetResABName(string assetPath)
    {
        Init();
        BundleType type = BundleType.Default;//默认打包策略
        string validatePath = null;

        //自定义中，最后一个出现的一定是最终的ab名称
        for (int i = ruleList.Count-1; i >= 0; i--)
        {
            AssetBundlePath abPath = ruleList[i];
            if (assetPath.StartsWith(abPath.path+"/"))
            {
                type = abPath.type;
                validatePath = abPath.path;
                break;
            }
        }
        //没有定制该资源的ab策略
        if (validatePath == null)
        {
            Debug.LogError("<AssetBundleNameAuto> 该资源所在的位置不正确，无法获取ab包名称，路径 = "+assetPath);
            return "";
        }
        switch (type)
        {
            case BundleType.FolderAll:
                //就直接到定制的文件夹
                return validatePath.Replace("Assets/", "").Replace("/", "_").ToLower();
            case BundleType.Default:
                string leftPath = assetPath.Replace(validatePath + "/", "");
                int index = leftPath.IndexOf("/", StringComparison.Ordinal);
                if (index > 0)
                {//还有文件夹，就命名到该文件夹
                    return (validatePath.Replace("Assets/", "").Replace("/", "_") + "_" + leftPath.Substring(0, index)).ToLower();
                }
                else
                {//没有文件夹，就需要加上文件名称
                    return (validatePath.Replace("Assets/", "").Replace("/", "_") + "_" + leftPath.Split('.')[0]).ToLower();
                }
            case BundleType.Folder:
                leftPath = assetPath.Replace(validatePath + "/", "");
                index = leftPath.IndexOf("/", StringComparison.Ordinal);
                if (index > 0)
                {//还有文件夹，就命名到该文件夹
                    return (validatePath.Replace("Assets/", "").Replace("/", "_") + "_" + leftPath.Substring(0, index)).ToLower();
                }
                else
                {//没有文件夹，就直接到定制的文件夹
                    return validatePath.Replace("Assets/", "").Replace("/", "_").ToLower();
                }
        }
        return "";
    }
}

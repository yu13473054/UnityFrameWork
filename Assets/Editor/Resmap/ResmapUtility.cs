using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public class ResmapUtility
{
    class ResmapInfo
    {
        public string key;
        public string editorPath;
        public string abName;
    }

    //导入文件的规则：如果指定值不为空，增必须同时满足
    struct FileRule
    {
        public string filter;
        public string ext;

        public FileRule(string filter, string ext)
        {
            this.filter = filter;
            this.ext = ext;
        }
    }

    class DirConfig
    {
        public DirConfig(bool allFileInto, params FileRule[] rules)
        {
            this.rules = rules;
            this.allFileInto = allFileInto;
        }

        public FileRule[] rules;//文件规则，只要满足其中一个规则，则能导入到resmap中
        public bool allFileInto;//该文件夹下的所有文件是否都能导入
    }

    const string DataDir = "Assets/Data/";
    const string MultiLngFile = "multiLngConfig.txt";

    private static bool _isInit = false;
    public static bool canAuto2Resmap = true;

    public static string[] LocalTypeList =
    {
        "CN", "EN", "JP"
    };

    private static Dictionary<string, DirConfig> _dirDic;

    //类型收集器
    static List<string> imgList = new List<string>();
    static List<string> prefList = new List<string>();
    static List<string> objList = new List<string>();

    //多语言名称配置
    static Dictionary<string, List<string>> localResNameConfig;
    private static bool localIsModify;

    static void InitData()
    {
        if (_isInit) return;
        _isInit = true;

        _dirDic = new Dictionary<string, DirConfig>();
        _dirDic.Add("Assets/Res/Icon", new DirConfig(true));
        _dirDic.Add("Assets/Res/Prefab", new DirConfig(true, new FileRule("", "prefab")));
        _dirDic.Add("Assets/Res/UI", new DirConfig(false, new FileRule("_dynamic", ""), new FileRule("", "prefab")));
        _dirDic.Add("Assets/Res/Font", new DirConfig(true, new FileRule("", "fontsettings"), new FileRule("", "TTF"), new FileRule("", "TTc")));
        for (int i = 0; i < LocalTypeList.Length; i++)
        {
            string key = LocalTypeList[i];
            _dirDic.Add(string.Format("Assets/Localization/{0}/Audio", key), new DirConfig(true));
            _dirDic.Add(string.Format("Assets/Localization/{0}/Data", key), new DirConfig(true));
            _dirDic.Add(string.Format("Assets/Localization/{0}/Sprites", key), new DirConfig(true));
            _dirDic.Add(string.Format("Assets/Localization/{0}/Font", key), new DirConfig(true, new FileRule("", "fontsettings"), new FileRule("", "TTF"), new FileRule("", "TTc")));
            _dirDic.Add(string.Format("Assets/Localization/{0}/FX", key), new DirConfig(true, new FileRule("", "prefab")));
            _dirDic.Add(string.Format("Assets/Localization/{0}/Prefab", key), new DirConfig(true, new FileRule("", "prefab")));


        }
        localResNameConfig = InitLocalResConfig();
    }

    [MenuItem("Assets/Add To Resmap %#=", false, 61)]
    static void AddtoResmap()
    {
        InitData();
        List<string> assets = GetSelectAssets();
        assets = CategorizeInfo(assets);
        StartDeal(true);
        Debug.Log("Add To Resmap Done!");
    }

    [MenuItem("Assets/Delete From Resmap %#-", false, 62)]
    static void DelFromResmap()
    {
        InitData();
        List<string> assets = GetSelectAssets();
        CategorizeInfo(assets);
        StartDeal(false);

        Debug.Log("Delete From Resmap Done!");
    }

    [MenuItem("打包/重新生成所有的Resmap", false, 21)]
    public static void AddAllAsset()
    {
        InitData();

        //根据之前的resmap，收集信息
        List<string> fileList = new List<string>();
        fileList.AddRange(CollectFileByResmap("resmap_obj.txt"));
        fileList.AddRange(CollectFileByResmap("resmap_prefab.txt"));
        fileList.AddRange(CollectFileByResmap("resmap_sprite.txt"));
        foreach(var pair in _dirDic)
        {
            string dirPath = pair.Key;
            if (Directory.Exists(dirPath) && pair.Value.allFileInto)
            {
                GetFilesRecursion(fileList, dirPath);
            }
        }

        AssetDatabase.DeleteAsset(DataDir + "resmap_obj.txt");
        AssetDatabase.DeleteAsset(DataDir + "resmap_prefab.txt");
        AssetDatabase.DeleteAsset(DataDir + "resmap_sprite.txt");
        AssetDatabase.DeleteAsset(DataDir + MultiLngFile);
        AssetDatabase.Refresh();

        CategorizeInfo(fileList);
        StartDeal(true);
        Debug.Log("重新生成resmap成功！");
    }

    [MenuItem("打包/自动导入resmap(开)", true)]
    static  bool _IsAutoOn()
    {
        return !canAuto2Resmap;
    }
    [MenuItem("打包/自动导入resmap(开)", false, 22)]
    static void IsAutoOn()
    {
        canAuto2Resmap = true;
    }

    [MenuItem("打包/自动导入resmap(关)", true)]
    static bool _IsAutoOff()
    {
        return canAuto2Resmap;
    }
    [MenuItem("打包/自动导入resmap(关)", false, 23)]
    static void IsAutoOff()
    {
        canAuto2Resmap = false;
    }

    static bool CheckValidByProcess(string file)
    {
        if (!CanResFile(file)) return false;

        foreach (var pair in _dirDic)
        {
            if(pair.Value.allFileInto && file.StartsWith(pair.Key))
            {
                return true;
            }
        }
        return false;
    }

    static void OnPostprocessAllAssets(string[] imports, string[] dels, string[] moves, string[] moveFroms)
    {
        if (!canAuto2Resmap) return;
        InitData();
        List<string> fileList = new List<string>();
        for (int i = 0; i < imports.Length; i++)
        {
            if (CheckValidByProcess(imports[i]))
                fileList.Add(imports[i]);
        }
        for (int i = 0; i < moves.Length; i++)
        {
            if (CheckValidByProcess(moves[i]))
                fileList.Add(imports[i]);
        }
        ToResmapExternal(fileList);
    }

    //同步资源后，动态修改resmap
    public static void ToResmapExternal(List<string> addFile)
    {
        if (addFile.Count == 0) return;
        for (int i = 0; i < addFile.Count; i++)
        {
            string path = addFile[i];
            int startIndex = path.IndexOf("Assets");
            if (startIndex == 0) continue;
            addFile[i] = path.Substring(startIndex);
        }
        InitData();
        CategorizeInfo(addFile);
        StartDeal(true);
    }

    private static void StartDeal(bool isAdd)
    {
        //处理图片
        Deal(isAdd, "resmap_sprite.txt", imgList);
        //处理预制体
        Deal(isAdd, "resmap_prefab.txt", prefList);
        //处理其他类型
        Deal(isAdd, "resmap_obj.txt", objList);

        GenerateLocalConfig();

        AssetDatabase.Refresh();
    }

    //处理多语言配置文件
    private static void DealLocalConfig(bool isAdd, string fileName, int index, string subfix)
    {
        if (index >= 0)
        {
            List<string> nameList;
            localResNameConfig.TryGetValue(fileName, out nameList);
            if (isAdd)
            {
                if (nameList != null)
                    //已经存在，就直接替换名称
                    nameList[index] = fileName + subfix;
                else
                {
                    //第一次添加
                    nameList = new List<string>();
                    for (int j = 0; j < LocalTypeList.Length; j++)
                    {
                        if (j == index)
                            nameList.Add(fileName + subfix);
                        else
                            nameList.Add(fileName);
                    }
                    localResNameConfig.Add(fileName, nameList);
                }
                localIsModify = true;
            }
            else
            {
                if (nameList != null)
                {
                    nameList[index] = fileName;
                    localIsModify = true;
                }
            }
        }
    }

    // 修改完之后，自动完成读写操作
    static void Deal(bool isAdd, string mapTxtName, List<string> assetList)
    {
        Dictionary<string, ResmapInfo> dic = InitResmap(mapTxtName);
        bool isModify = false;
        for (int i = 0; i < assetList.Count; i++)
        {
            string filePath = NormalizePath(assetList[i]);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            //多语言文件需要拼接出文件名
            int index;
            string subfix = GetLocalResSubfix(filePath, out index);
            if (index >= 0) fileName = fileName + subfix;
            //重新设置信息
            ResmapInfo info = null;
            dic.TryGetValue(fileName, out info);
            if (isAdd) //增加
            {
                if (info == null)
                {
                    info = new ResmapInfo();
                    info.key = fileName;
                    info.editorPath = filePath;
                    info.abName = ABNameUtility.GetResABName(filePath);
                    if (!string.IsNullOrEmpty(info.abName))
                    {
                        dic.Add(fileName, info);
                        isModify = true;
                    }
                }
                else
                {
                    if (info.editorPath.Equals(filePath)) continue;

                    if (File.Exists(info.editorPath))
                    {
                        Debug.LogErrorFormat("添加的资源已经有重名的键名{0}，请检查！已存在路径{1}，添加的路径{2}", fileName, info.editorPath, filePath);
                        continue;
                    }
                    string abName = ABNameUtility.GetResABName(filePath);
                    if (string.IsNullOrEmpty(abName)) continue;
                    info.abName = abName;
                    info.editorPath = filePath;
                    isModify = true;
                }
                DealLocalConfig(true, Path.GetFileNameWithoutExtension(filePath), index, subfix);
            }
            else //删除操作
            {
                if (info != null)
                {
                    if (info.editorPath.Equals(filePath)) //保证是同一条
                    {
                        dic.Remove(fileName);
                        isModify = true;
                        DealLocalConfig(false, Path.GetFileNameWithoutExtension(filePath), index, subfix);
                    }
                }
            }
        }

        //对不存在的文件，再一次删除
        List<string> keyList = new List<string>(dic.Keys);
        for (int i = 0; i < keyList.Count; i++)
        {
            ResmapInfo info = dic[keyList[i]];
            string filePath = info.editorPath;
            if (!File.Exists(filePath))
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                int index;
                string subfix = GetLocalResSubfix(filePath, out index);
                DealLocalConfig(false, fileName, index, subfix);
                //从字典中删除该条信息
                dic.Remove(keyList[i]);
                isModify = true;
            }
        }

        if (isModify)
        {
            //对资源进行名称排序
            dic = dic.OrderBy(pair => pair.Key).ToDictionary(k => k.Key, v => v.Value);

            //重新生成文件
            GenerateTxt(mapTxtName, dic);
        }

        if (!File.Exists(DataDir + mapTxtName))
        {
            GenerateTxt(mapTxtName, null);
        }
    }

    static Dictionary<string, ResmapInfo> InitResmap(string resmapName)
    {
        Dictionary<string, ResmapInfo> dic = new Dictionary<string, ResmapInfo>();
        if (!File.Exists(DataDir + resmapName)) return dic;
        TableHandlerEditor handler = TableHandlerEditor.Open(DataDir, resmapName);
        for (int row = 0; row < handler.GetRecordsNum(); row++)
        {
            string key = handler.GetValue(row, 0);
            if (dic.ContainsKey(key))
            {
                Debug.LogErrorFormat("{0}中有重名的键，键名{1}", resmapName, key);
                continue;
            }
            ResmapInfo info = new ResmapInfo()
            {
                key = key,
                editorPath = handler.GetValue(row, 1),
                abName = handler.GetValue(row, 2),

            };
            dic.Add(key, info);
        }
        return dic;
    }

    static List<string> CollectFileByResmap(string path)
    {
        List<string> result = new List<string>();
        Dictionary<string, ResmapInfo> dic = InitResmap(path);
        foreach (var info in dic)
        {
            if (File.Exists(info.Value.editorPath))
                result.Add(info.Value.editorPath);
        }
        return result;
    }

    static Dictionary<string, List<string>> InitLocalResConfig()
    {
        Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();
        if (!File.Exists(DataDir + MultiLngFile)) return dic;
        TableHandlerEditor handler = TableHandlerEditor.Open(DataDir, MultiLngFile);
        for (int row = 0; row < handler.GetRecordsNum(); row++)
        {
            string key = handler.GetValue(row, 0);
            if (dic.ContainsKey(key))
            {
                Debug.LogErrorFormat("{0}中有重名的键，键名{1}", MultiLngFile, key);
                continue;
            }
            List<string> resNameList = new List<string>();
            for (int i = 1; i < handler.GetFieldsNum(); i++)
            {
                resNameList.Add(handler.GetValue(row, i));
            }
            dic.Add(key, resNameList);
        }
        return dic;
    }

    /// <param name="sourceAssets">如果是自动操作的，需要指定paths</param>
    static List<string> CategorizeInfo(List<string> sourceAssets)
    {
        imgList.Clear();
        prefList.Clear();
        objList.Clear();

        //查找出有效的资源路径
        if (sourceAssets.Count == 0) return null;
        List<string> fileList = new List<string>();
        foreach (string objPath in sourceAssets)
        {
            if (!objPath.StartsWith("Assets/Res") && !objPath.StartsWith("Assets/Localization") && !objPath.StartsWith("Assets/Data")) continue;
            //选中文件夹：收集该文件夹中所有的文件
            if (Directory.Exists(objPath))
                GetFilesRecursion(fileList, objPath);
            else if (CanResFile(objPath))
                fileList.Add(objPath);
        }

        sourceAssets.Clear(); //存储有效的资源路径
        //分类
        for (int i = 0; i < fileList.Count; i++)
        {
            string file = fileList[i];
            if (!ValidateAsset(file)) continue;
            if (file.EndsWith(".png") || file.EndsWith(".jpg"))
            {
                imgList.Add(file);
                sourceAssets.Add(file);
            }
            else if (file.EndsWith(".prefab"))
            {
                prefList.Add(file);
                sourceAssets.Add(file);
            }
            else if (!file.EndsWith(".meta"))
            {
                objList.Add(file);
                sourceAssets.Add(file);
            }
        }
        return sourceAssets;
    }

    //根据路径获取多语言的文件名
    static string GetLocalResSubfix(string filePath, out int index)
    {
        index = -1;
        if (!filePath.StartsWith("Assets/Localization")) return null;
        for (int i = 0; i < LocalTypeList.Length; i++)
        {
            if (filePath.Contains("/" + LocalTypeList[i] + "/"))
            {
                index = i;
                return "_" + LocalTypeList[i].ToLower();
            }
        }
        return null;
    }

    public static string GetResSubfixByIndex(int index)
    {
        return "_" + LocalTypeList[index - 1].ToLower();
    }

    private static List<string> GetSelectAssets()
    {
        Object[] selecObjs = Selection.GetFiltered<Object>(SelectionMode.Assets);
        List<string> tmpList = new List<string>();
        foreach (Object obj in selecObjs)
        {
            string objPath = AssetDatabase.GetAssetPath(obj);
            tmpList.Add(objPath);
        }
        return tmpList;
    }

    static void GenerateTxt(string txtName, Dictionary<string, ResmapInfo> dic)
    {
        StringBuilder sb = new StringBuilder();
        //第一行
        sb.Append("资源键名").Append("\t").Append("资源路径").Append("\t").Append("所在assetBundle名称").Append("\n");
        if (dic != null)
        {
            foreach (ResmapInfo info in dic.Values)
            {
                sb.Append(info.key).Append("\t").Append(info.editorPath).Append("\t").Append(info.abName).Append("\n");
            }
        }
        //去掉最后一个换行符
        File.WriteAllText(DataDir + txtName, sb.ToString().TrimEnd());
    }

    static void GenerateLocalConfig()
    {
        StringBuilder sb = new StringBuilder();
        //第一行
        sb.Append("资源名").Append("\t");
        for (int i = 0; i < LocalTypeList.Length; i++)
        {
            sb.Append(LocalTypeList[i]);
            sb.Append(i == LocalTypeList.Length - 1 ? "\n" : "\t");
        }

        if (localIsModify)
        {
            localIsModify = false;
            localResNameConfig = localResNameConfig.OrderBy(pair => pair.Key).ToDictionary(k => k.Key, v => v.Value);

            foreach (KeyValuePair<string, List<string>> pair in localResNameConfig)
            {
                sb.Append(pair.Key).Append("\t");
                int count = pair.Value.Count;
                for (int i = 0; i < count; i++)
                {
                    sb.Append(pair.Value[i]);
                    sb.Append(i == count - 1 ? "\n" : "\t");
                }
            }
            //去掉最后一个换行符
            File.WriteAllText(DataDir + MultiLngFile, sb.ToString().TrimEnd());
        }
        if (!File.Exists(DataDir + MultiLngFile))
        {
            File.WriteAllText(DataDir + MultiLngFile, sb.ToString().TrimEnd());
        }
    }

    //选中的物体为有效可填入资源
    static bool ValidateAsset(string filePath)
    {
        foreach(var pair in _dirDic)
        {
            string dir = pair.Key;
            DirConfig config = pair.Value;
            //判读文件是否有效
            if (filePath.StartsWith(dir) && config.rules.Length > 0)
            {
                string fileName = Path.GetFileName(filePath);
                for (int i = 0; i < config.rules.Length; i++)
                {
                    FileRule rule = config.rules[i];
                    bool result = true;
                    if (!string.IsNullOrEmpty(rule.filter))
                    {
                        result &= fileName.Contains(rule.filter);
                    }
                    if (!string.IsNullOrEmpty(rule.ext))
                    {
                        result &= fileName.EndsWith(rule.ext);
                    }
                    if (result) return true; //满足文件规则
                }
                return false; //所有的文件规则都不满足
            }
        }
        return true;//至此，说明文件是不受规则影响的文件

    }

    /// 递归获取dirPath文件夹下所有文件，包括子文件夹中的文件
    static void GetFilesRecursion(List<string> resultList, string dirPath)
    {
        if (!Directory.Exists(dirPath))
        {
            Debug.LogError("请检查路径是不是文件夹。" + dirPath);
            return;
        }

        string[] files = Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            //文件剔除
            if (!CanResFile(file))
                continue;
            string newFile = NormalizePath(file);
            resultList.Add(newFile);
        }
        string[] childDirPaths = Directory.GetDirectories(dirPath);
        for (int i = 0; i < childDirPaths.Length; i++)
        {
            GetFilesRecursion(resultList, childDirPaths[i]);
        }
    }

    static string NormalizePath(string sourcePath)
    {
        return sourcePath.Replace("\\", "/");
    }

    static bool CanResFile(string file)
    {
        return !file.EndsWith(".meta") && !file.EndsWith(".cginc") && !file.EndsWith(".cs") && !file.EndsWith(".spriteatlas")
            && !file.Contains(".svn") && !file.Contains(".DS_Store");
    }
}

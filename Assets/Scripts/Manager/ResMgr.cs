using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using Object = UnityEngine.Object;
using System.Collections;

// 添加引用计数的结构
public class AssetBundleInfo
{
    public AssetBundle ab;
    public string[] dependencies;
    public List<string> moduleList;//记录模块：被依赖进内存的不记录

    public AssetBundleInfo()
    {
        moduleList = new List<string>();
    }

    /// <summary>
    /// 记录，如果已经记录，则返回false
    /// </summary>
    public bool RecordModule(string moduleName)
    {
        bool contains = moduleList.Contains(moduleName);
        if (!contains)
        {
            moduleList.Add(moduleName);
        }
        return !contains;
    }
}

public struct AssetInfo
{
    public string assetName;
    public string abName;
    public string path;
    public int type; //1：Sprite   2：Prefab    3,obj

    public AssetInfo(string assetName, string abName, string path, int type)
    {
        this.assetName = assetName;
        this.abName = abName;
        this.path = path;
        this.type = type;
    }
}

public class ResMgr : MonoBehaviour
{
    #region 初始化
    private static ResMgr _inst;
    public static ResMgr Inst
    {
        get { return _inst; }
    }
    #endregion

    public static string[] ignoreArray = { };

    private AssetBundleManifest _rootManifest;
    private Dictionary<string, AssetBundleInfo> _bundleDic;
    private Dictionary<string, List<AssetBundleInfo>> _moduleDic;
    Dictionary<string, string[]> _multiLngNameDic;     // 多语言名称映射表
    Dictionary<string, ResmapInfo> _resmap_sprite;    // 图片映射表
    Dictionary<string, ResmapInfo> _resmap_prefab;    // Prefab映射表
    Dictionary<string, ResmapInfo> _resmap_obj;    // 其他资源映射表

    private Dictionary<string, int> _refCountDic = new Dictionary<string, int>();
    //预加载的资源
    private List<AssetInfo> _preLoadAssets = new List<AssetInfo>();
    private List<string> _preLoadABs = new List<string>();

    void Awake()
    {
        _inst = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Init()
    {
        _bundleDic = new Dictionary<string, AssetBundleInfo>();
        _moduleDic = new Dictionary<string, List<AssetBundleInfo>>();
        _multiLngNameDic = new Dictionary<string, string[]>();

        //加载ab依赖关系文件
        if (GameMain.Inst.ResourceMode != 0)
        {
            _rootManifest = LoadAsset<AssetBundleManifest>("assetbundle", "AssetBundleManifest", "manifest");
        }

        _resmap_sprite = InitResmap("resmap_sprite");
        _resmap_prefab = InitResmap("resmap_prefab");
        _resmap_obj = InitResmap("resmap_obj");

        //多语言资源名表
        TableHandler mutilLngTable = TableHandler.OpenFromData("multiLngConfig");
        for (int row = 0; row < mutilLngTable.GetRecordsNum(); row++)
        {
            string[] s = new string[mutilLngTable.GetFieldsNum() - 1];
            for (int i = 1; i < mutilLngTable.GetFieldsNum(); i++)
            {
                s[i - 1] = mutilLngTable.GetValue(row, i);
            }
            _multiLngNameDic.Add(mutilLngTable.GetValue(row, 0), s);
        }

        LoadAB("res_shader", "shader"); //预加载shader，永久不卸载
    }

    /// 初始化Resmap
    Dictionary<string, ResmapInfo> InitResmap(string resmapName)
    {
        Dictionary<string, ResmapInfo> dic = new Dictionary<string, ResmapInfo>();
        TableHandler handler = TableHandler.OpenFromData(resmapName);
        for (int row = 0; row < handler.GetRecordsNum(); row++)
        {
            ResmapInfo info = new ResmapInfo()
            {
                key = handler.GetValue(row, 0),
                editorPath = handler.GetValue(row, 1),
                abName = handler.GetValue(row, 2),

            };
            dic.Add(info.key, info);
        }
        return dic;
    }

    /// 销毁资源
    void OnDestroy()
    {
        foreach (var bundle in _bundleDic)
        {
            bundle.Value.ab.Unload(false);
        }
        _bundleDic.Clear();
        _moduleDic.Clear();
        _refCountDic.Clear();
        Resources.UnloadUnusedAssets();
        Debug.Log("<ResMgr> OnDestroy!");
    }

    /// 当使用者被销毁时，清除其引用
    public void OnModuleDestroy(string moduleName, bool unloadAll = true)
    {
        // 开发者模式，没有记录ab的使用者
        if (GameMain.Inst == null || GameMain.Inst.ResourceMode == 0) return;
        //防止先调用了ResMgr的OnDestroy方法
        if (_moduleDic == null) return;
        //遍历所有的AB包，清除使用者
        List<AssetBundleInfo> abInfoList = null;
        if (_moduleDic.TryGetValue(moduleName, out abInfoList))
        {
            for (int i = abInfoList.Count - 1; i >= 0; i--)
            {
                AssetBundleInfo abInfo = abInfoList[i];
                //移除使用者
                if (abInfo.moduleList.Remove(moduleName))
                {
                    if (abInfo.moduleList.Count == 0 && !_preLoadABs.Contains(abInfo.ab.name))
                    {
                        //卸载ab包的逻辑
                        _bundleDic.Remove(abInfo.ab.name);
                        abInfo.ab.Unload(unloadAll);
                    }
                }
            }
            _moduleDic.Remove(moduleName);
        }
    }

    /// 直接清除指定AB的相关信息，慎用！！！
    public void UnloadAB(string abName, bool unloadAll = false)
    {
        if (_bundleDic.ContainsKey(abName))
        {
            AssetBundleInfo abInfo = _bundleDic[abName];
            //移除所有ab包中的相关使用者
            List<string> moduleList = abInfo.moduleList;
            for (int i = moduleList.Count - 1; i >= 0; i--)
            {
                string moduleName = moduleList[i];
                List<AssetBundleInfo> moudleABList = _moduleDic[moduleName];
                moudleABList.Remove(abInfo);
                if (moudleABList.Count == 0)
                    _moduleDic.Remove(moduleName);
            }

            //卸载ab包
            if (!_preLoadABs.Contains(abInfo.ab.name))
            {
                _bundleDic.Remove(abInfo.ab.name);
                abInfo.ab.Unload(unloadAll);
            }
        }
        else
        {
            Debug.LogErrorFormat("<ResMgr> 卸载{0}失败，该ab包未加载！", abName);
        }
    }

    //卸载加载的且没有使用过的模块
    public void UnloadUnusedModule()
    {
        List<string> keyList = new List<string>(_refCountDic.Keys);
        for (int i = 0; i < keyList.Count; i++)
        {
            string moduleName = keyList[i];
            if (_refCountDic[moduleName] == 0)
            {
                if(moduleName.Equals("manifest") || moduleName.Equals("data") || moduleName.Equals("lua")
                   || moduleName.Equals("shader"))
                    continue;
                _refCountDic.Remove(moduleName);
                OnModuleDestroy(moduleName);
            }
        }
    }

    public IEnumerator CoUnloadUnused(AsyncOperation operation, Action<float> progressCB, Action finish)
    {
        yield return null;
        while (!operation.isDone)
        {
            if (progressCB != null)
                progressCB(operation.progress);
            yield return null;
        }
        if (finish != null)
            finish();
    }
    //卸载未使用的资源
    public void UnloadUnused(Action<float> progressCB = null, Action finish = null)
    {
        AsyncOperation opt = Resources.UnloadUnusedAssets();
        if (progressCB != null)
        {
            StartCoroutine(CoUnloadUnused(opt, progressCB, finish));
        }
    }

    #region 模块名计数

    //预加载逻辑
    private void AddUnusedModule(string moduleName)
    {
        if (!_refCountDic.ContainsKey(moduleName))
        {
            _refCountDic.Add(moduleName, 0);
        }
    }

    public void AddModuleRef(string moduleName)
    {
        int count;
        if (_refCountDic.TryGetValue(moduleName, out count))
        {
            _refCountDic[moduleName] = count + 1;
        }
        else
        {
            _refCountDic.Add(moduleName, 1);
        }
    }

    /// <returns>是否减少成功</returns>
    public bool DelModuleRef(string moduleName)
    {
        int count;
        if (_refCountDic.TryGetValue(moduleName, out count))
        {
            _refCountDic[moduleName] = count - 1;
            if (count - 1 == 0)
            {
                _refCountDic.Remove(moduleName);
                OnModuleDestroy(moduleName);
            }
            return true;
        }
        return false;
    }

    #endregion

    public bool ContainModule(string moduleName)
    {
        return _refCountDic.ContainsKey(moduleName);
    }

    #region 同步资源加载
    public T LoadAsset<T>(string abName, string assetName, string module, string editorPath = "") where T : UnityEngine.Object
    {
        // 加载AssetBundle
        AssetBundle ab = LoadAB(abName, module);
        if (ab == null)
        {
            return default(T);
        }

        // 读取assetbundle里的这个文件
        T asset = ab.LoadAsset<T>(assetName);
        if (asset == null)
        {
            Debug.LogError("<ResMgr> 找不到Asset :" + assetName);
            return default(T);
        }

        return asset;
    }

    /// 载入AssetBundle
    public AssetBundle LoadAB(string abName, string modlue)
    {
        if (GameMain.Inst.ResourceMode == 0)
            return null;

        abName = abName.ToLower();
        AssetBundleInfo abInfo = null;
        if (!_bundleDic.TryGetValue(abName, out abInfo))
        {
            abInfo = new AssetBundleInfo();
            _bundleDic.Add(abName, abInfo);

            AssetBundle ab_tmp;
            if (IsEncrypt(abName))
                ab_tmp = AssetBundle.LoadFromFile(CommonUtils.GetABPath(abName.GetHashCode().ToString()), 0, 18);
            else
                ab_tmp = AssetBundle.LoadFromFile(CommonUtils.GetABPath(abName.GetHashCode().ToString()));


            if (ab_tmp == null)
            {
                Debug.LogErrorFormat("<ResMgr> 加载{0}失败！", abName);
                return null;
            }
            abInfo.ab = ab_tmp;
            abInfo.ab.name = abName; //手动赋值，打包自动生成的根AB包的Name为空

        }

        //记录使用者
        if (RecordModule(abInfo, modlue))
            LoadDependencies(modlue, abName);//加载该ab包的依赖包

        return abInfo.ab;
    }

    //按照使用者将AssetBundleInfo分类
    private bool RecordModule(AssetBundleInfo abInfo, string module)
    {
        if (!_moduleDic.ContainsKey(module))
        {
            _moduleDic.Add(module, new List<AssetBundleInfo>());
            AddUnusedModule(module); //尝试预保存模块
        }
        if (abInfo.RecordModule(module))
        {
            _moduleDic[module].Add(abInfo);
            return true;
        }
        return false;
    }

    //携程用记录依赖
    private void OnRecordModule(AssetBundleInfo abInfo, string moduleName)
    {
        if (RecordModule(abInfo, moduleName))
        {
            string[] dependencies = abInfo.dependencies;
            if (dependencies == null) return;
            for (int i = 0; i < dependencies.Length; i++)
            {
                AssetBundleInfo depAbInfo = null;
                if (_bundleDic.TryGetValue(dependencies[i], out depAbInfo))
                {
                    OnRecordModule(depAbInfo, moduleName);
                }
            }
        }
    }
    void LoadDependencies(string module, string abName)
    {
        if (_rootManifest == null)
            return;
        AssetBundleInfo abInfo = _bundleDic[abName];
        if (abInfo == null)
        {
            Debug.LogErrorFormat("<ResMgr> 查询{0}依赖失败！", abName);
            return;
        }

        //获取所有依赖
        string[] dependencies = abInfo.dependencies;
        if (abInfo.dependencies == null)
        {
            dependencies = _rootManifest.GetAllDependencies(abName);
            abInfo.dependencies = dependencies;
        }

        //增加新加载的ab的引用计数
        for (int i = 0; i < dependencies.Length; i++)
        {
            string depName = dependencies[i];
            LoadAB(depName, module);
        }
    }

    //1：Sprite   2：Prefab    3：obj
    public T LoadAsset<T>(string assetName, int type, string moduleName, bool isTry = false) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(moduleName))
        {
            Debugger.LogError("<ResourceManager> 模块名不能为空，请检查！资源名：" + assetName);
            return null;
        }
        ResmapInfo info = GetMapInfo(assetName, type, isTry);
        if (info == null) return null;
        if (GameMain.Inst.ResourceMode == 0)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(info.editorPath);
#else
            Debugger.LogError("<ResourceManager> 错误的运行环境，应该是编辑器模式");
            return null;
#endif
        }
        else
            return LoadAsset<T>(info.abName, assetName, moduleName);
    }

    #endregion

    //获取多语言资源名称
    private string GetResName(string reskeyname)
    {
        string[] nameArray;
        if (_multiLngNameDic.TryGetValue(reskeyname, out nameArray))
        {
            return nameArray[(int)GameMain.Inst.lngType - 1];
        }
        return reskeyname;
    }

    public ResmapInfo GetMapInfo(string assetName, int resType, bool isTry = false)
    {
        assetName = GetResName(assetName);
        ResmapInfo info = null;
        switch (resType)
        {
            case 1:
                if (_resmap_sprite == null || !_resmap_sprite.TryGetValue(assetName, out info))
                {
                    if (!isTry)
                        Debug.LogError("<ResMgr> 映射表中没有这个资源: " + assetName);
                    return null;
                }
                break;
            case 2:
                if (_resmap_prefab == null || !_resmap_prefab.TryGetValue(assetName, out info))
                {
                    if (!isTry)
                        Debug.LogError("<ResMgr> 映射表中没有这个资源: " + assetName);
                    return null;
                }
                break;
            case 3:
                if (_resmap_obj == null || !_resmap_obj.TryGetValue(assetName, out info))
                {
                    if (!isTry)
                        Debug.LogError("<ResMgr> 映射表中没有这个资源: " + assetName);
                    return null;
                }
                break;
        }

        return info;
    }

    public static bool IsEncrypt(string abName)
    {
        for (int i = 0; i < ignoreArray.Length; i++)
        {
            if (abName.StartsWith(ignoreArray[i]))
                return false;
        }
        return true;
    }

    #region 异步加载
    // 异步载入
    void LoadAssetAsyn<T>(string assetBundleName, string assetNames, Action<string, T> action, string moduleName) where T : UnityEngine.Object
    {
        StartCoroutine(OnLoadAsset<T>(assetBundleName, assetNames, action, moduleName));
    }
    // 异步载入协程
    IEnumerator OnLoadAsset<T>(string abName, string assetName, Action<string, T> action, string moduleName) where T : UnityEngine.Object
    {
        // 强制小写
        abName = abName.ToLower();

        // 如果缓存里有先拿缓存
        AssetBundleInfo bundle;
        if (!_bundleDic.TryGetValue(abName, out bundle))
        {
            bundle = new AssetBundleInfo();
            _bundleDic.Add(abName, bundle);
            yield return OnLoadDependencies(abName, moduleName);
            yield return OnLoadAB(abName, moduleName);
        }

        //记录使用者
        if (!string.IsNullOrEmpty(moduleName))
        {
            OnRecordModule(bundle, moduleName);
        }

        AssetBundleRequest request = bundle.ab.LoadAssetAsync<T>(assetName);
        yield return request;

        // 如果没找到
        if (request.asset == null)
        {
            Debug.LogError("<ResMgr> 找不到Asset :" + assetName);
            yield break;
        }

        // 执行回调
        if (action != null)
            action(assetName, (T)request.asset);
    }

    // 异步读AB
    IEnumerator OnLoadAB(string abName, string moduleName)
    {
        AssetBundleCreateRequest request;
        if (IsEncrypt(abName))
            request = AssetBundle.LoadFromFileAsync(CommonUtils.GetABPath(abName.GetHashCode().ToString()), 0, 18);
        else
            request = AssetBundle.LoadFromFileAsync(CommonUtils.GetABPath(abName.GetHashCode().ToString()));
        yield return request;

        if (request.assetBundle == null)
        {
            Debug.LogError("<ResMgr> 没有此AssetBundle："+abName);
            yield break;
        }


        AssetBundleInfo abInfo = _bundleDic[abName];
        abInfo.ab = request.assetBundle;
        abInfo.ab.name = abName;//手动赋值，assetbundle的name属性为空
    }

    IEnumerator OnLoadDependencies(string abName, string moduleName)
    {
        //获取所有依赖
        string[] dependencies = _rootManifest.GetAllDependencies(abName);
        if (dependencies.Length == 0) yield break;
        //记录依赖
        if (_bundleDic.ContainsKey(abName))
        {
            _bundleDic[abName].dependencies = dependencies;
        }
        else
        {
            Debugger.LogError("没有ab包使用该依赖！");
            yield break;
        }

        //增加新加载的ab的引用计数
        for (int i = 0; i < dependencies.Length; i++)
        {
            string depName = dependencies[i];
            AssetBundleInfo bundle;
            if (!_bundleDic.TryGetValue(depName, out bundle))
            {
                bundle = new AssetBundleInfo();
                _bundleDic.Add(depName, bundle);
                yield return OnLoadAB(depName, moduleName);
            }
        }
    }
    #endregion

    #region 预加载

    public int PreLoadCount()
    {
        if (_preLoadAssets == null) return 0;
        return _preLoadAssets.Count;
    }

    //增加预加载资源
    public void AddPreLoadAsset(string reskeyname, int resType)
    {
        ResmapInfo info = GetMapInfo(reskeyname, resType);
        _preLoadAssets.Add(new AssetInfo(reskeyname, info.abName, info.editorPath, resType));
        if (GameMain.Inst.ResourceMode == 0) return;
        AddPreLoadAB(info.abName);
    }

    private void AddPreLoadAB(string abName)
    {
        //保存ab信息
        if (_preLoadABs.Contains(abName)) return;

        _preLoadABs.Add(abName);
        //获取所有依赖
        string[] dependencies = _rootManifest.GetAllDependencies(abName);
        for (int j = 0; j < dependencies.Length; j++)
        {
            string depName = dependencies[j];
            AddPreLoadAB(depName);
        }
    }

    /// <summary>
    /// 开始预加载流程
    /// </summary>
    /// <param name="refreshDone"></param> 更新的回调，每加载一个对象，就调用一次
    /// <param name="finishDone"></param> 加载完成的回调
    /// <param name="autoClearPreList"></param> 是否在加载完成后，自动清除加载队列
    /// <returns></returns>
    public int StartPreLoad(Action<int> refreshDone, Action finishDone, bool autoClearPreList = true)
    {
        StartCoroutine(CoStartPreLoad(refreshDone, finishDone, autoClearPreList));
        return _preLoadAssets.Count;
    }

    IEnumerator CoStartPreLoad(Action<int> refreshDone, Action finishDone, bool autoClearPreList)
    {
        yield return null;
        for (int i = 0; i < _preLoadAssets.Count; i++)
        {
            if (refreshDone != null) refreshDone((i + 1));
            AssetInfo info = _preLoadAssets[i];
            // 0：从本地读，1：从AB读
            if (GameMain.Inst.ResourceMode == 0)
            {
#if UNITY_EDITOR
                switch (info.type)
                {
                    case 1:
                        UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(info.path);
                        yield return null;
                        break;
                    case 2:
                        UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(info.path);
                        yield return null;
                        break;
                    case 3:
                        UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(info.path);
                        yield return null;
                        break;
                }
#endif
            }
            else
            {
                switch (info.type)
                {
                    case 1:
                        yield return OnLoadAsset<Sprite>(info.abName, info.assetName, null, null);
                        break;
                    case 2:
                        yield return OnLoadAsset<GameObject>(info.abName, info.assetName, null, null);
                        break;
                    case 3:
                        yield return OnLoadAsset<UnityEngine.Object>(info.abName, info.assetName, null, null);
                        break;
                }
            }

        }
        yield return null;
        if (finishDone != null) finishDone();
        finishDone = null;
        if (autoClearPreList)
        {
            ClearPreList();
        }
    }

    //清理预加载逻辑
    public void ClearPreList()
    {
        _preLoadAssets.Clear();
        _preLoadABs.Clear();
    }
    #endregion

}

public class ResmapInfo
{
    public string key;
    public string editorPath;
    public string abName;
}

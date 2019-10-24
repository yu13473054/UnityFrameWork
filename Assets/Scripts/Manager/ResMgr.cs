using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using Object = UnityEngine.Object;
using System.Collections;

/// <summary>
/// 策略：
/// 1，直接使用非依赖的ab包需要记录使用者，当同一个对象多次使用时不会重复记录
/// 2，直接使用已经靠依赖读进来的ab包，需要记录哪些对象使用这个ab包
/// 3，靠依赖读进来的ab包，记录这些包的使用者为主包的使用者
/// 4，使用UI作为资源的索引，读取资源时，传入UI名称，当UI界面销毁时，进行资源的释放，需保证所有的UI界面都是唯一实例，不会出现多实例
/// </summary>
public class ResMgr : MonoBehaviour
{
    #region 初始化
    private static ResMgr _inst;
    public static ResMgr Inst
    {
        get { return _inst; }
    }
    #endregion

    private AssetBundleManifest _rootManifest;
    private AssetBundle _rootAB;
    private Dictionary<string, AssetBundleInfo> _bundleDic;
    private Dictionary<string, List<AssetBundleInfo>> _moduleDic;
    Dictionary<string, string[]> _multiLngNameDic;     // 多语言名称映射表
    Dictionary<string, ResmapInfo> _resmap_sprite;    // 图片映射表
    Dictionary<string, ResmapInfo> _resmap_prefab;    // Prefab映射表
    Dictionary<string, ResmapInfo> _resmap_obj;    // 其他资源映射表

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
            _rootAB = InitAssetBundle("assetbundle");
            _rootManifest = _rootAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
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

    public AssetBundle InitAssetBundle(string abName)
    {
        return AssetBundle.LoadFromFile(CommonUtils.GetABPath(abName),0,10);
    }

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

    public ResmapInfo GetResinfoInObj(string key)
    {
        key = GetResName(key);
        ResmapInfo info;
        if (_resmap_obj == null || !_resmap_obj.TryGetValue(key, out info))
        {
            Debug.LogError("<ResMgr> 映射表中没有这个资源: " + key);
            return null;
        }
        return info;
    }
    public ResmapInfo GetResinfoInPrefab(string reskeyname)
    {
        reskeyname = GetResName(reskeyname);
        ResmapInfo info;
        if (_resmap_prefab == null || !_resmap_prefab.TryGetValue(reskeyname, out info))
        {
            Debug.LogError("<ResMgr> 映射表中没有这个资源: " + reskeyname);
            return null;
        }
        return info;
    }
    public ResmapInfo GetResinfoInSprite(string reskeyname)
    {
        reskeyname = GetResName(reskeyname);

        ResmapInfo info;
        if (_resmap_sprite == null || !_resmap_sprite.TryGetValue(reskeyname, out info))
        {
            Debug.LogError("<ResMgr> 映射表中没有这个资源: " + reskeyname);
            return null;
        }
        return info;
    }

    //加载Prefab，该Prefab需要填写在resmap_prefab.txt文件中
    public GameObject LoadPrefab(string reskeyname, string module)
    {
        string assetName = reskeyname;
        ResmapInfo info = GetResinfoInPrefab(reskeyname);
        return LoadAsset<GameObject>(module, assetName, info.abName,info.editorPath);
    }

    //Sprite，该Sprite需要填写在resmap_sprite.txt文件中
    public Sprite LoadSprite(string reskeyname, string module)
    {
        string assetName = reskeyname;
        ResmapInfo info = GetResinfoInSprite(reskeyname);
        return LoadAsset<Sprite>(module, assetName, info.abName, info.editorPath);
    }

    public Texture LoadTexture(string reskeyname, string module)
    {
        string assetName = reskeyname;
        ResmapInfo info = GetResinfoInSprite(reskeyname);
        return LoadAsset<Texture>(module, assetName, info.abName, info.editorPath);
    }

    //Object，该Object需要填写在resmap_obj.txt文件中
    public Object LoadObj(string reskeyname, string module)
    {
        string assetName = reskeyname;
        ResmapInfo info = GetResinfoInObj(reskeyname);
        return LoadAsset<Object>(module, assetName, info.abName, info.editorPath);
    }

    public T LoadObj<T>(string reskeyname, string module) where T : UnityEngine.Object
    {
        string assetName = reskeyname;
        ResmapInfo info = GetResinfoInObj(reskeyname);
        return LoadAsset<T>(module, assetName, info.abName, info.editorPath);
    }

    public Material LoadMaterial(string reskeyname, string module)
    {
        string assetName = reskeyname;
        ResmapInfo info = GetResinfoInObj(reskeyname);
        return LoadAsset<Material>(module, assetName, info.abName, info.editorPath);
    }

    public T LoadAsset<T>(string module, string assetName, string abName, string editorPath = "") where T : UnityEngine.Object
    {
        if (GameMain.Inst.ResourceMode == 0)
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(editorPath);
#else
            return null;
#endif
        else
        {
            if(string.IsNullOrEmpty(module))
            {
                Debug.LogError("<ResMgr> 模块名不能为空！");
                return default(T);
            }

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
    }

    /// 载入AssetBundle
    public AssetBundle LoadAB(string abName, string modlue)
    {
        abName = abName.ToLower();
        AssetBundleInfo abInfo = null;
        if (!_bundleDic.ContainsKey(abName))
        {
            abInfo = new AssetBundleInfo();
            _bundleDic.Add(abName, abInfo);

            AssetBundle ab_tmp = InitAssetBundle(abName);//关联数据的素材绑定
            abInfo.ab = ab_tmp;
            abInfo.ab.name = abName; //手动赋值，打包自动生成的根AB包的Name为空
            abInfo.dependencies = _rootManifest.GetAllDependencies(abName);
        }
        else
        {
            _bundleDic.TryGetValue(abName, out abInfo);
        }
        if (abInfo == null || abInfo.ab == null)
        {
            _bundleDic.Remove(abName);//移除无效的ab信息
            Debug.LogErrorFormat("<ResMgr> 加载{0}失败！", abName);
            return null;
        }

        //记录使用者
        if(RecordModule(abInfo, modlue))
            LoadDependencies(modlue, abName);//加载该ab包的依赖包

        return abInfo.ab;
    }

    //按照使用者将AssetBundleInfo分类
    private bool RecordModule(AssetBundleInfo abInfo, string module)
    {
        if (!_moduleDic.ContainsKey(module))
        {
            _moduleDic.Add(module,new List<AssetBundleInfo>());
        }
        if (abInfo.RecordMoudle(module))
        {
            _moduleDic[module].Add(abInfo);
            return true;
        }
        return false;
    }

    /// 载入依赖
    void LoadDependencies(string module, string abName)
    {
        AssetBundleInfo abInfo = _bundleDic[abName];
        if (abInfo == null)
        {
            Debug.LogErrorFormat("<ResMgr> 查询{0}依赖失败！", abName);
            return;
        }

        //获取所有依赖
        string[] dependencies = abInfo.dependencies;
        if (dependencies.Length == 0) return;

        //增加新加载的ab的引用计数
        for (int i = 0; i < dependencies.Length; i++)
        {
            string depName = dependencies[i];
            LoadAB(depName, module);
        }
    }

    /// 销毁资源
    void OnDestroy()
    {
        if (_rootManifest != null)
        {
            _rootManifest = null;
            _rootAB.Unload(true);
        }

        foreach (var bundle in _bundleDic)
        {
            bundle.Value.ab.Unload(true);
        }
        _bundleDic.Clear();
        _bundleDic = null;

        _moduleDic.Clear();
        _moduleDic = null;
        Resources.UnloadUnusedAssets();

        _inst = null;
        Debug.Log("<ResMgr> OnDestroy!");
    }

    /// 直接清除指定AB的相关信息，慎用！！！
    public void UnloadAB(string abName, bool unloadAll = false)
    {
        if (_bundleDic.ContainsKey(abName))
        {
            AssetBundleInfo abInfo = _bundleDic[abName];
            //移除所有ab包中的相关使用者
            List<string> moudleList = abInfo.moudleList;
            for (int i = moudleList.Count - 1; i >= 0; i--)
            {
                string moduleName = moudleList[i];
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

    /// 当使用者被销毁时，清除其引用
    public void OnMoudleDestroy(string moduleName, bool unloadAll = true)
    {
        // 开发者模式，没有记录ab的使用者
        if (GameMain.Inst == null || GameMain.Inst.ResourceMode == 0) return;
        //防止先调用了ResMgr的OnDestroy方法
        if(_moduleDic == null) return;

        //遍历所有的AB包，清除使用者
        List<AssetBundleInfo> abInfoList = null;
        _moduleDic.TryGetValue(moduleName, out abInfoList);
        if (abInfoList==null)
        {
            Debug.LogError("<ResMgr> 竟然没有该UI相关的ab包，不可能出现，请检查传递的使用者名称，当前UI名称："+moduleName);
        }
        else
        {
            for (int i = abInfoList.Count-1; i >=0; i--)
            {
                AssetBundleInfo abInfo = abInfoList[i];
                //移除使用者
                if (abInfo.moudleList.Remove(moduleName))
                {
                    if (abInfo.moudleList.Count == 0 && !_preLoadABs.Contains(abInfo.ab.name))
                    {
                        //卸载ab包的逻辑
                        abInfo.ab.Unload(unloadAll);
                        _bundleDic.Remove(abInfo.ab.name);
                    }
                }
            }
            _moduleDic.Remove(moduleName);
        }
    }

    public IEnumerator CoUnloadUnused(AsyncOperation operation, Action<float> progressCB, Action finish)
    {
        yield return null;
        while (!operation.isDone)
        {
            if(progressCB!=null)
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
        if(progressCB != null)
        {
            StartCoroutine(CoUnloadUnused(opt, progressCB, finish));
        }
    }

    #region 异步加载
    // 异步载入协程
    IEnumerator OnLoadAsset<T>(string abName, string assetName, Action<string, T> action, string moduleName) where T : UnityEngine.Object
    {
        // 如果缓存里有先拿缓存
        AssetBundleInfo bundleInfo;
        if (!_bundleDic.TryGetValue(abName, out bundleInfo))
        {
            yield return OnLoadAB(abName, moduleName);
            bundleInfo = _bundleDic[abName];
        }

        AssetBundleRequest request = bundleInfo.ab.LoadAssetAsync<T>(assetName);
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
        AssetBundleInfo abInfo;
        if (!_bundleDic.TryGetValue(abName, out abInfo))
        {
            abInfo = new AssetBundleInfo();
            _bundleDic.Add(abName, abInfo);

            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(CommonUtils.GetABPath(abName), 0, 10);
            yield return request;
            if (request.assetBundle == null)
            {
                Debug.LogError("<ResourceManager> 没有此AssetBundle："+abName);
                yield break;
            }
            
            abInfo.ab = request.assetBundle;
            abInfo.ab.name = abName;//手动赋值，assetbundle的name属性为空
            abInfo.dependencies = _rootManifest.GetAllDependencies(abName);
        }

        //记录使用者
        if (RecordModule(abInfo, moduleName))
        {
            yield return OnLoadDependencies(abName, moduleName);
        }
    }

    IEnumerator OnLoadDependencies(string abName, string moduleName)
    {
        //获取所有依赖
        string[] dependencies = _bundleDic[abName].dependencies;
        if (dependencies.Length == 0) yield break;

        //增加新加载的ab的引用计数
        for (int i = 0; i < dependencies.Length; i++)
        {
            string depName = dependencies[i];
            yield return OnLoadAB(depName, moduleName);
        }
    }
    #endregion

    #region 预加载
    //增加预加载资源
    public void AddPreLoadAsset(string reskeyname, int resType)
    {
        ResmapInfo info = null;
        switch (resType)
        {
            case 1://图片
                info = GetResinfoInSprite(reskeyname);
                break;
            case 2://预制体
                info = GetResinfoInPrefab(reskeyname);
                break;
            case 3://其他
                info = GetResinfoInObj(reskeyname);
                break;
        }
        //保存ab信息
        if (!_preLoadABs.Contains(info.abName))
        {
            _preLoadABs.Add(info.abName);
        }
        _preLoadAssets.Add(new AssetInfo(info, resType));
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
                        UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(info.resInfo.editorPath);
                        yield return null;
                        break;
                    case 2:
                        UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(info.resInfo.editorPath);
                        yield return null;
                        break;
                    case 3:
                        UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(info.resInfo.editorPath);
                        yield return null;
                        break;
                }
#endif
            }
            else
            {
                //获取所有依赖
                string[] dependencies = _rootManifest.GetAllDependencies(info.resInfo.abName);
                for (int j = 0; j < dependencies.Length; j++)
                {
                    string depName = dependencies[j];
                    if (!_preLoadABs.Contains(depName))
                    {
                        _preLoadABs.Add(depName);
                    }
                }
                switch (info.type)
                {
                    case 1:
                        yield return OnLoadAsset<Sprite>(info.resInfo.abName, info.resInfo.key, null, null);
                        break;
                    case 2:
                        yield return OnLoadAsset<GameObject>(info.resInfo.abName, info.resInfo.key, null, null);
                        break;
                    case 3:
                        yield return OnLoadAsset<UnityEngine.Object>(info.resInfo.abName, info.resInfo.key, null, null);
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

public class AssetBundleInfo
{
    public string[] dependencies;//该ab包的依赖包
    public AssetBundle ab;
    public List<string> moudleList;//记录使用者：被依赖进内存的不记录使用者

    public AssetBundleInfo()
    {
        moudleList = new List<string>();
    }

    /// <summary>
    /// 记录使用者，如果已经记录，则返回false
    /// </summary>
    public bool RecordMoudle(string moudle)
    {
        bool contains = moudleList.Contains(moudle);
        if (!contains)
        {
            moudleList.Add(moudle);
        }
        return !contains;
    }

}

public struct AssetInfo
{
    public ResmapInfo resInfo;
    public int type; //1：Sprite   2：Prefab    3,obj

    public AssetInfo(ResmapInfo resmapInfo, int type)
    {
        this.resInfo = resmapInfo;
        this.type = type;
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LuaInterface;
using UObject = UnityEngine.Object;

public class ResMgr : MonoBehaviour
{
    #region 初始化
    private static ResMgr _inst;
    public static ResMgr Inst
    {
        get { return _inst; }
    }
    public static void Init()
    {
        if (_inst)
        {
            return;
        }
        GameObject go = new GameObject("ResMgr");
        go.AddComponent<ResMgr>();
    }
    #endregion

    private AssetBundleManifest _rootManifest;
    private AssetBundle _rootAB;
    private Dictionary<string, AssetBundleInfo> _bundles;

    void Awake()
    {
        _inst = this;
        DontDestroyOnLoad(gameObject);

        _bundles = new Dictionary<string, AssetBundleInfo>();
        //加载ab依赖关系文件
        if (GameMain.Inst.ResourceMode!=0)
        {

        }
        else
        {
            _rootAB = AssetBundle.LoadFromFile(CommonUtils.ResFullPath(AppConst.AssetDir, ""));
            _rootManifest = _rootAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
    }

    public T LoadAsset<T>(string assetName, string abName, string editorPath) where T : UnityEngine.Object
    {
        if (GameMain.Inst.ResourceMode == 0)
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(editorPath);
#else
            return null;
#endif
        else
            return LoadAsset<T>(abName, assetName);
    }

    public T LoadAsset<T>(string ABName, string assetName) where T : UnityEngine.Object
    {
        // 加载AssetBundle
        AssetBundle ab = LoadAB(ABName);
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

    public T GetData<T>() where T : UnityEngine.Object
    {
        if (GameMain.Inst.ResourceMode!=0)
        {
#if UNITY_EDITOR
            T data = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(CommonUtils.ResFullPath(AppConst.DataDir + typeof(T).Name, ".asset"));
            if (!data)
            {
                Debug.LogErrorFormat("表格不存在，表格名称：{0}", typeof(T).Name);
            }
            return data;
#else
                return default(T);
#endif
        }
        else
        {
            return LoadAssetFromAB<T>(AppConst.DataABName, typeof(T).Name);
        }
    }

    public GameObject GetPrefab(string name)
    {
        UIResPathProperty property = DatabaseMgr.Inst.GetUIResPathProperty(name);
        if (GameMain.Inst.ResourceMode!=0)
        {
#if UNITY_EDITOR
            GameObject  go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(CommonUtils.ResFullPath(property._res_develop));
            if (!go)
            {
                Debug.LogErrorFormat("获取prefab失败：{0}, 资源路径：{1}", name, CommonUtils.ResFullPath(property._res_develop));
            }

            return go;
#else
                return null;
#endif
        }
        else
        {
            return LoadAssetFromAB<GameObject>(property._abName, name);
        }
    }

    /// <summary>
    /// 从ab包载入素材
    /// </summary>
    T LoadAssetFromAB<T>(string abName, string assetName) where T : UnityEngine.Object
    {
        abName = abName.ToLower();
        AssetBundle ab_Tmp = LoadAB(abName);
        return ab_Tmp.LoadAsset<T>(assetName);
    }
    /// <summary>
    /// 载入AssetBundle
    /// </summary>
    /// <param name="abName"></param>
    /// <returns></returns>
    public AssetBundle LoadAB(string abName)
    {
        abName = CommonUtils.ConstraintABName(abName);
        AssetBundleInfo abInfo = null;
        if (!_bundles.ContainsKey(abName))
        {
            abInfo = new AssetBundleInfo();
            _bundles.Add(abName, abInfo);

            LoadDependencies(abName);//加载该ab包的依赖包
            string resPath = CommonUtils.ResFullPath(abName);
            Debug.LogFormat("正在加载ab包：{0}", resPath);
            AssetBundle ab_tmp = AssetBundle.LoadFromFile(resPath); //关联数据的素材绑定
            abInfo.AB = ab_tmp;
        }
        else
        {
            _bundles.TryGetValue(abName, out abInfo);
        }
        if (abInfo == null || abInfo.AB == null)
        {
            _bundles.Remove(abName);//移除无效的ab信息
            Debug.LogErrorFormat("加载{0}.ab失败！", abName);
            return null;
        }
        return abInfo.AB;
    }

    /// <summary>
    /// 载入依赖
    /// </summary>
    void LoadDependencies(string abName)
    {
        if (_rootManifest == null)
        {
            Debug.LogError("请先加载_rootManifest!!");
            return;
        }
        //获取所有依赖
        string[] dependencies = _rootManifest.GetAllDependencies(abName);
        if (dependencies.Length == 0) return;
        //记录依赖
        if (_bundles.ContainsKey(abName))
        {
            _bundles[abName].Dependencies = dependencies;
        }
        else
        {
            Debug.LogError("没有ab包使用该依赖！");
            return;
        }

        //增加新加载的ab的引用计数
        for (int i = 0; i < dependencies.Length; i++)
        {
            string depName = dependencies[i];
            //第一次load的依赖包，不需要增加引用计数
            if (_bundles.ContainsKey(depName))
            {
                _bundles[depName].ReferenceChange();//增加一引用计数
            }
            else
            {
                LoadAB(depName);
            }
        }
    }

    /// <summary>
    /// 销毁资源
    /// </summary>
    void OnDestroy()
    {
        if (_rootManifest != null)
        {
            _rootManifest = null;
            _rootAB.Unload(true);
        }

        foreach (var bundle in _bundles)
        {
            bundle.Value.AB.Unload(true);
        }
        _bundles.Clear();
        Resources.UnloadUnusedAssets();
        Debug.Log("~ResMgr was destroy!");
    }

    /// <summary>
    /// 此函数交给外部卸载专用，自己调整是否需要彻底清除AB
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="unloadAll"></param>
    public void UnloadAB(string abName, bool unloadAll = false)
    {
        abName = CommonUtils.ConstraintABName(abName);
        if (_bundles.ContainsKey(abName))
        {
            //卸载指定ab包
            AssetBundleInfo abInfo = _bundles[abName];
            abInfo.AB.Unload(unloadAll);
            _bundles.Remove(abName);
            //处理依赖包
            string[] dependencies = abInfo.Dependencies;
            if (dependencies == null || dependencies.Length == 0)
            {
                return;
            }
            for (int i = 0; i < dependencies.Length; i++)
            {
                string depAbName = dependencies[i];
                AssetBundleInfo depAbInfo = _bundles[depAbName];
                depAbInfo.ReferenceChange(-1);
                if (depAbInfo.GetReference() <= 0)//没有引用后，也需要卸载
                {
                    UnloadAB(depAbName);
                }
            }
        }
        else
        {
            Debug.LogErrorFormat("卸载{0}失败，该ab包未加载！", abName);
        }

    }

    public GameObject LoadPrefab(string uiName)
    {
        return null;
    }

    public void LoadPrefab(string abName, string[] assetNames, LuaFunction func)
    {
        abName = abName.ToLower();
        List<UObject> result = new List<UObject>();
        for (int i = 0; i < assetNames.Length; i++)
        {
            UObject go = LoadAssetFromAB<UObject>(abName, assetNames[i]);
            if (go != null) result.Add(go);
        }
        if (func != null) func.Call((object)result.ToArray());
    }
}
public class AssetBundleInfo
{
    private int _referencedCount;//被多少个AB引用了
    public string[] Dependencies { get; set; }//该ab包的依赖包
    public AssetBundle AB { get; set; }

    public AssetBundleInfo()
    {
        _referencedCount = 1;
    }

    public void ReferenceChange(int value = 1)
    {
        _referencedCount += value;
    }

    public int GetReference()
    {
        return _referencedCount;
    }
}
#region 异步获取资源
//#if ASYNC_MODE
//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using System;
//using System.IO;
//using LuaInterface;
//using UObject = UnityEngine.Object;
//
//
//namespace LuaFramework {
//
//    public class ResMgr : MonoBehaviour
//    {
//        private static ResMgr _inst;
//        public static ResMgr Inst 
//        {
//            get { return _inst; }
//        }
//
//        string _baseDownloadingURL = "";
//        string[] _allManifest = null;
//        AssetBundleManifest _assetBundleManifest = null;
//        Dictionary<string, string[]> _dependencies = new Dictionary<string, string[]>();
//        Dictionary<string, AssetBundleInfo> _loadedAssetBundles = new Dictionary<string, AssetBundleInfo>();
//        Dictionary<string, List<LoadAssetRequest>> _loadRequests = new Dictionary<string, List<LoadAssetRequest>>();
//
////        Dictionary<string, AssetBundle> _loadABDic = new Dictionary<string, AssetBundle>();
//
//        class LoadAssetRequest {
//            public Type _assetType;
//            public string[] _assetNames;
//            public LuaFunction _luaFunc;
//            public Action<UObject[]> _sharpFunc;
//        }
//
//        void Awake()
//        {
//            _inst = this;
//            DontDestroyOnLoad(gameObject);
//        }
//
//        // Load AssetBundleManifest.
//        public void Initialize(string manifestName, Action initOK) {
//            _baseDownloadingURL = Util.GetRelativePath();
//            LoadAsset<AssetBundleManifest>(manifestName, new string[] { "AssetBundleManifest" }, delegate(UObject[] objs) {
//                if (objs.Length > 0) {
//                    _assetBundleManifest = objs[0] as AssetBundleManifest;
//                    _allManifest = _assetBundleManifest.GetAllAssetBundles();
//                }
//                if (initOK != null) initOK();
//            });
//        }
//
//        public T GetData<T>(string tableName) where T : UnityEngine.Object
//        {
//#if UNITY_EDITOR
//            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(Util.ResPath(tableName, ".asset"));
//#else
//            AssetBundle ab_Tmp = AssetBundle.LoadFromFile(Util.ResPath("cfgdata.ab"));
//            if (ab_Tmp == null)
//            {
//                Debug.LogError("加载cfgdata.ab失败！");
//            }
//            else
//            {
//                return ab_Tmp.LoadAsset<T>(tableName);
//            }
//            return null;
//#endif
//        }
//
//        public void LoadPrefab(string abName, string assetName, Action<UObject[]> func) {
//            LoadAsset<GameObject>(abName, new string[] { assetName }, func);
//        }
//
//        public void LoadPrefab(string abName, string[] assetNames, Action<UObject[]> func) {
//            LoadAsset<GameObject>(abName, assetNames, func);
//        }
//
//        public void LoadPrefab(string abName, string[] assetNames, LuaFunction func) {
//            LoadAsset<GameObject>(abName, assetNames, null, func);
//        }
//
//        string GetRealAssetPath(string abName) {
//            if (abName.Equals(AppConst.AssetDir)) {
//                return abName;
//            }
//            abName = abName.ToLower();
//            if (!abName.EndsWith(AppConst.ExtName)) {
//                abName += AppConst.ExtName;
//            }
//            if (abName.Contains("/")) {
//                return abName;
//            }
//            //string[] paths = m_AssetBundleManifest.GetAllAssetBundles();  产生GC，需要缓存结果
//            for (int i = 0; i < _allManifest.Length; i++) {
//                int index = _allManifest[i].LastIndexOf('/');  
//                string path = _allManifest[i].Remove(0, index + 1);    //字符串操作函数都会产生GC
//                if (path.Equals(abName)) {
//                    return _allManifest[i];
//                }
//            }
//            Debug.LogError("GetRealAssetPath Error:>>" + abName);
//            return null;
//        }
//
//        /// <summary>
//        /// 载入素材
//        /// </summary>
//        void LoadAsset<T>(string abName, string[] assetNames, Action<UObject[]> action = null, LuaFunction func = null) where T : UObject {
//            abName = GetRealAssetPath(abName);
//
//            LoadAssetRequest request = new LoadAssetRequest();
//            request._assetType = typeof(T);
//            request._assetNames = assetNames;
//            request._luaFunc = func;
//            request._sharpFunc = action;
//
//            List<LoadAssetRequest> requests = null;
//            if (!_loadRequests.TryGetValue(abName, out requests)) {
//                requests = new List<LoadAssetRequest>();
//                requests.Add(request);
//                _loadRequests.Add(abName, requests);
//                StartCoroutine(OnLoadAsset<T>(abName));
//            } else {
//                requests.Add(request);
//            }
//        }
//
//        IEnumerator OnLoadAsset<T>(string abName) where T : UObject {
//            AssetBundleInfo bundleInfo = GetLoadedAssetBundle(abName);
//            if (bundleInfo == null) {
//                yield return StartCoroutine(OnLoadAssetBundle(abName, typeof(T)));
//
//                bundleInfo = GetLoadedAssetBundle(abName);
//                if (bundleInfo == null) {
//                    _loadRequests.Remove(abName);
//                    Debug.LogError("OnLoadAsset--->>>" + abName);
//                    yield break;
//                }
//            }
//            List<LoadAssetRequest> list = null;
//            if (!_loadRequests.TryGetValue(abName, out list)) {
//                _loadRequests.Remove(abName);
//                yield break;
//            }
//            for (int i = 0; i < list.Count; i++) {
//                string[] assetNames = list[i]._assetNames;
//                List<UObject> result = new List<UObject>();
//
//                AssetBundle ab = bundleInfo.m_AssetBundle;
//                for (int j = 0; j < assetNames.Length; j++) {
//                    string assetPath = assetNames[j];
//                    AssetBundleRequest request = ab.LoadAssetAsync(assetPath, list[i]._assetType);
//                    yield return request;
//                    result.Add(request.asset);
//
//                    //T assetObj = ab.LoadAsset<T>(assetPath);
//                    //result.Add(assetObj);
//                }
//                if (list[i]._sharpFunc != null) {
//                    list[i]._sharpFunc(result.ToArray());
//                    list[i]._sharpFunc = null;
//                }
//                if (list[i]._luaFunc != null) {
//                    list[i]._luaFunc.Call((object)result.ToArray());
//                    list[i]._luaFunc.Dispose();
//                    list[i]._luaFunc = null;
//                }
//                bundleInfo.m_ReferencedCount++;
//            }
//            _loadRequests.Remove(abName);
//        }
//
//        IEnumerator OnLoadAssetBundle(string abName, Type type) {
//            string url = _baseDownloadingURL + abName;
//
//            WWW download = null;
//            if (type == typeof(AssetBundleManifest))
//                download = new WWW(url);
//            else {
//                string[] dependencies = _assetBundleManifest.GetAllDependencies(abName);
//                if (dependencies.Length > 0) {
//                    _dependencies.Add(abName, dependencies);
//                    for (int i = 0; i < dependencies.Length; i++) {
//                        string depName = dependencies[i];
//                        AssetBundleInfo bundleInfo = null;
//                        if (_loadedAssetBundles.TryGetValue(depName, out bundleInfo)) {
//                            bundleInfo.m_ReferencedCount++;
//                        } else if (!_loadRequests.ContainsKey(depName)) {
//                            yield return StartCoroutine(OnLoadAssetBundle(depName, type));
//                        }
//                    }
//                }
//                download = WWW.LoadFromCacheOrDownload(url, _assetBundleManifest.GetAssetBundleHash(abName), 0);
//            }
//            yield return download;
//
//            AssetBundle assetObj = download.assetBundle;
//            if (assetObj != null) {
//                _loadedAssetBundles.Add(abName, new AssetBundleInfo(assetObj));
//            }
//        }
//
//        AssetBundleInfo GetLoadedAssetBundle(string abName) {
//            AssetBundleInfo bundle = null;
//            _loadedAssetBundles.TryGetValue(abName, out bundle);
//            if (bundle == null) return null;
//
//            // No dependencies are recorded, only the bundle itself is required.
//            string[] dependencies = null;
//            if (!_dependencies.TryGetValue(abName, out dependencies))
//                return bundle;
//
//            // Make sure all dependencies are loaded
//            foreach (var dependency in dependencies) {
//                AssetBundleInfo dependentBundle;
//                _loadedAssetBundles.TryGetValue(dependency, out dependentBundle);
//                if (dependentBundle == null) return null;
//            }
//            return bundle;
//        }
//
//        /// <summary>
//        /// 此函数交给外部卸载专用，自己调整是否需要彻底清除AB
//        /// </summary>
//        /// <param name="abName"></param>
//        /// <param name="isThorough"></param>
//        public void UnloadAssetBundle(string abName, bool isThorough = false) {
//            abName = GetRealAssetPath(abName);
//            Debug.Log(_loadedAssetBundles.Count + " assetbundle(s) in memory before unloading " + abName);
//            UnloadAssetBundleInternal(abName, isThorough);
//            UnloadDependencies(abName, isThorough);
//            Debug.Log(_loadedAssetBundles.Count + " assetbundle(s) in memory after unloading " + abName);
//        }
//
//        void UnloadDependencies(string abName, bool isThorough) {
//            string[] dependencies = null;
//            if (!_dependencies.TryGetValue(abName, out dependencies))
//                return;
//
//            // Loop dependencies.
//            foreach (var dependency in dependencies) {
//                UnloadAssetBundleInternal(dependency, isThorough);
//            }
//            _dependencies.Remove(abName);
//        }
//
//        void UnloadAssetBundleInternal(string abName, bool isThorough) {
//            AssetBundleInfo bundle = GetLoadedAssetBundle(abName);
//            if (bundle == null) return;
//
//            if (--bundle.m_ReferencedCount <= 0) {
//                if (_loadRequests.ContainsKey(abName)) {
//                    return;     //如果当前AB处于Async Loading过程中，卸载会崩溃，只减去引用计数即可
//                }
//                bundle.m_AssetBundle.Unload(isThorough);
//                _loadedAssetBundles.Remove(abName);
//                Debug.Log(abName + " has been unloaded successfully");
//            }
//        }
//    }
//}
#endregion

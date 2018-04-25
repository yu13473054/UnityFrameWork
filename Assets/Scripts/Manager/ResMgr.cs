using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using Object = UnityEngine.Object;

/// <summary>
/// 策略：
/// 1，直接使用非依赖的ab包需要记录使用者，当同一个对象多次使用时不会重复记录
/// 2，直接使用已经靠依赖读进来的ab包，需要记录哪些对象使用这个ab包
/// 3，靠依赖读进来的ab包，记录这些包的使用者为主包的使用者
/// 4，使用UI作为资源的索引，读取资源时，传入UI名称，当UI界面销毁时，进行资源的释放，需保证所有的UI界面都是唯一实例，不会出现多实例
/// 5，数据Data文件，获取后，就解析保存了一个数据备份，不需要管理
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
    private Dictionary<string, List<AssetBundleInfo>> _userDic;

    Dictionary<string, ResmapInfo> _resmap_sprite;    // 图片映射表
    Dictionary<string, ResmapInfo> _resmap_prefab;    // Prefab映射表
    Dictionary<string, ResmapInfo> _resmap_obj;    // 其他资源映射表

    void Awake()
    {
        _inst = this;
        DontDestroyOnLoad(gameObject);

        _bundleDic = new Dictionary<string, AssetBundleInfo>();
        _userDic = new Dictionary<string, List<AssetBundleInfo>>();

        //加载ab依赖关系文件
        if (GameMain.Inst.ResourceMode==0)
        {

        }
        else
        {
            _rootAB = InitAssetBundle("assetbundle");
            _rootManifest = _rootAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
    }

    //加载Prefab，该Prefab需要填写在resmap_prefab.txt文件中
    public GameObject LoadPrefab(string userName, string reskeyname)
    {
        if (_resmap_prefab == null) _resmap_prefab = InitResmap("resmap_prefab.txt");

        ResmapInfo info;
        if (_resmap_prefab == null || !_resmap_prefab.TryGetValue(reskeyname,out info))
        {
            Debug.LogError("<ResMgr> 映射表中没有这个预制: " + reskeyname);
            return null;
        }
        return LoadAsset<GameObject>(userName,reskeyname,info.abName,info.editorPath);
    }

    //Sprite，该Sprite需要填写在resmap_sprite.txt文件中
    public Sprite LoadSprite(string userName, string reskeyname)
    {
        if (_resmap_sprite == null) _resmap_sprite = InitResmap("resmap_sprite.txt");

        ResmapInfo info;
        if (_resmap_sprite == null || !_resmap_sprite.TryGetValue(reskeyname, out info))
        {
            Debug.LogError("<ResMgr> 映射表中没有这个预制: " + reskeyname);
            return null;
        }
        return LoadAsset<Sprite>(userName, reskeyname, info.abName, info.editorPath);
    }

    //Object，该Object需要填写在resmap_obj.txt文件中
    public Object LoadObj(string userName, string reskeyname)
    {
        if (_resmap_obj == null) _resmap_obj = InitResmap("resmap_obj.txt");

        ResmapInfo info;
        if (_resmap_obj == null || !_resmap_obj.TryGetValue(reskeyname, out info))
        {
            Debug.LogError("<ResMgr> 映射表中没有这个预制: " + reskeyname);
            return null;
        }
        return LoadAsset<Object>(userName, reskeyname, info.abName, info.editorPath);
    }

    /// 初始化Resmap
    Dictionary<string, ResmapInfo> InitResmap(string resmapName)
    {
        Dictionary<string, ResmapInfo> dic= new Dictionary<string, ResmapInfo>();
        TableHandler handler = TableHandler.Open(resmapName, "data");
        for (int row = 0; row < handler.GetRecordsNum(); row++)
        {
            ResmapInfo info = new ResmapInfo()
            {
                key = handler.GetValue(row,0),
                editorPath = handler.GetValue(row,1),
                abName = handler.GetValue(row,2),

            };
            dic.Add(info.key, info);
        }
        return dic;
    }

    public AssetBundle InitAssetBundle(string abName)
    {
        if (AppConst.encrypt)
        {
            byte[] abContent = CommonUtils.ReadFileBytes(CommonUtils.GetABPath(abName));
            byte[] abDeCrypt = DecipherUtility.DecryptBytes(abContent);
            return AssetBundle.LoadFromMemory(abDeCrypt);
        }
        else
        {
            return AssetBundle.LoadFromFile(CommonUtils.GetABPath(abName));
        }
    }

    public static string ByteText(byte[] bytes)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
            sb.Append(bytes[i]).Append("  ");
        }
        return sb.ToString();
    }

    public T LoadAsset<T>(string userName, string assetName, string abName, string editorPath) where T : UnityEngine.Object
    {
        if (GameMain.Inst.ResourceMode == 0)
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(editorPath);
#else
            return null;
#endif
        else
            return LoadAsset<T>(userName, assetName, abName);
    }

    public T LoadAsset<T>(string userName, string assetName, string ABName) where T : UnityEngine.Object
    {
        // 加载AssetBundle
        AssetBundle ab = LoadAB(userName, ABName);
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
    public AssetBundle LoadAB(string userName, string abName)
    {
        abName = abName.ToLower();
        AssetBundleInfo abInfo = null;
        if (!_bundleDic.ContainsKey(abName))
        {
            abInfo = new AssetBundleInfo();
            _bundleDic.Add(abName, abInfo);

            LoadDependencies(userName, abName);//加载该ab包的依赖包
            AssetBundle ab_tmp = InitAssetBundle(abName);//关联数据的素材绑定
            abInfo.ab = ab_tmp;
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
        RecordUser(abInfo, userName);

        return abInfo.ab;
    }

    //按照使用者将AssetBundleInfo分类
    private void RecordUser(AssetBundleInfo abInfo, string userName)
    {
        if (!_userDic.ContainsKey(userName))
        {
            _userDic.Add(userName,new List<AssetBundleInfo>());
        }
        if (abInfo.RecordUser(userName))
        {
            _userDic[userName].Add(abInfo);
        }
    }

    /// 载入依赖
    void LoadDependencies(string userName, string abName)
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
        if (_bundleDic.ContainsKey(abName))
        {
            _bundleDic[abName].dependencies = dependencies;
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
            LoadAB(userName,depName);
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

        _userDic.Clear();
        _userDic = null;
        Resources.UnloadUnusedAssets();

        _inst = null;
    }

    /// 直接清除指定AB的相关信息，慎用！！！
    public void UnloadAB(string abName, bool unloadAll = false)
    {
        if (_bundleDic.ContainsKey(abName))
        {
            AssetBundleInfo abInfo = _bundleDic[abName];
            //移除所有ab包中的相关使用者
            List<string> userList = abInfo.userList;
            for (int i = userList.Count - 1; i >= 0; i--)
            {
                OnUserDestroy(userList[i], unloadAll);
            }
        }
        else
        {
            Debug.LogErrorFormat("<ResMgr> 卸载{0}失败，该ab包未加载！", abName);
        }

    }

    /// 当使用者被销毁时，清除其引用
    public void OnUserDestroy(string userName, bool unloadAll = true)
    {
        // 开发者模式，没有记录ab的使用者
        if (GameMain.Inst == null || GameMain.Inst.ResourceMode == 0) return;
        //防止先调用了ResMgr的OnDestroy方法
        if(_userDic == null) return;

        //遍历所有的AB包，清除使用者
        List<AssetBundleInfo> abInfoList = null;
        _userDic.TryGetValue(userName, out abInfoList);
        if (abInfoList==null)
        {
            Debug.LogError("<ResMgr> 竟然没有该UI相关的ab包，不可能出现，请检查传递的使用者名称，当前UI名称："+userName);
        }
        else
        {
            for (int i = abInfoList.Count-1; i >=0; i--)
            {
                AssetBundleInfo abInfo = abInfoList[i];
                //移除使用者
                if (abInfo.userList.Remove(userName))
                {
                    if (abInfo.userList.Count == 0)
                    {
                        //卸载ab包的逻辑
                        abInfo.ab.Unload(unloadAll);
                        _bundleDic.Remove(abInfo.ab.name);
                    }
                }
            }
            _userDic.Remove(userName);
        }
    }
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
    public List<string> userList;//记录使用者：被依赖进内存的不记录使用者

    public AssetBundleInfo()
    {
        userList = new List<string>();
    }

    /// <summary>
    /// 记录使用者，如果已经记录，则返回false
    /// </summary>
    public bool RecordUser(string userName)
    {
        bool contains = userList.Contains(userName);
        if (!contains)
        {
            userList.Add(userName);
        }
        return !contains;
    }

}

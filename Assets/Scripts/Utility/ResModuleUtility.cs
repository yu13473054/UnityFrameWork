using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ResModuleUtility : MonoBehaviour
{
    [HideInInspector]
    public string moduleName;
    void Awake()
    {
        moduleName = name.Replace( "(Clone)", "" ).TrimEnd( ' ' );
        if(ResMgr.Inst)
            ResMgr.Inst.AddModuleRef(moduleName);//模块计数加一
    }
    void OnDestroy()
    {
        if (ResMgr.Inst)
            ResMgr.Inst.DelModuleRef(moduleName);//模块计数减一
    }

    #region 资源获取接口
    // 读取Texture资源
    public Texture LoadTexture(string reskeyname, bool isTry = false)
    {
        return ResMgr.Inst.LoadAsset<Texture>(reskeyname, 1, moduleName, isTry);
    }
    // 读取Sprite资源
    public Sprite LoadSprite(string reskeyname, bool isTry = false)
    {
        return ResMgr.Inst.LoadAsset<Sprite>(reskeyname, 1, moduleName, isTry);
    }
    // 读取Prefab资源
    public GameObject LoadPrefab(string reskeyname, bool instantiate = false, bool isTry = false)
    {
        GameObject go = ResMgr.Inst.LoadAsset<GameObject>(reskeyname, 2, moduleName, isTry);
        if( go == null )
            return null;
        if (instantiate)
        {
            go = Instantiate(go);
            go.name = reskeyname;
        }
        return go;
    }
    // 读取Object
    public UnityEngine.Object LoadObject(string reskeyname, bool isTry = false)
    {
        return ResMgr.Inst.LoadAsset<UnityEngine.Object>(reskeyname, 3, moduleName, isTry);
    }
    public Material LoadMaterial(string reskeyname, bool isTry = false)
    {
        return ResMgr.Inst.LoadAsset<Material>(reskeyname, 3, moduleName, isTry);
    }
    public Shader LoadShader( string reskeyname, bool isTry = false )
    {
        return ResMgr.Inst.LoadAsset<Shader>( reskeyname, 3, moduleName, isTry );
    }
    public Font LoadFont( string reskeyname, bool isTry = false )
    {
        return ResMgr.Inst.LoadAsset<Font>( reskeyname, 3, moduleName, isTry );
    }
    #endregion

    /// <summary>
    /// 该prefab资源需要有资源管理脚本ResModuleUtility
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="instantiate">是否产生实例，默认生成</param>
    /// <param name="isTry">尝试加载，此时不会输出Error日志</param>
    /// <returns></returns>
    public static GameObject LoadPrefabWithRMU(string assetName, bool instantiate = false, bool isTry = false)
    {
        GameObject go = ResMgr.Inst.LoadAsset<GameObject>(assetName, 2, assetName, isTry);
        if (instantiate)
        {
            go = Instantiate(go);
            go.name = assetName;
            if (!go.GetComponent<ResModuleUtility>())
                go.AddComponent<ResModuleUtility>();
            return go;
        }
        if (!go.GetComponent<ResModuleUtility>() && !isTry)
        {
            Debugger.LogError("<ResourceManager> 需要在预制体{0}上挂载ResModuleUtility脚本！", assetName);
            return null;
        }
            
        return go;
    }

    //Lua用的Unused资源卸载接口
    public static void UnloadUnused(Action<float> callBack = null, Action finishDone = null)
    {
        ResMgr.Inst.UnloadUnused(callBack, finishDone);
        LuaMgr.Inst.LuaGC();
    }


    //Lua用的Unused资源卸载接口
    public static void UnloadUnusedAssets()
    {
        ResMgr.Inst.UnloadUnused();
    }

    #region 预加载
    public static int PreLoadCount()
    {
        return ResMgr.Inst.PreLoadCount();
    }

    //增加预加载资源
    public static void AddPreLoadAsset(string reskeyname, int resType)
    {
        ResMgr.Inst.AddPreLoadAsset(reskeyname, resType);
    }

    /// <summary>
    /// 开始预加载流程
    /// </summary>
    /// <param name="refreshDone"></param> 更新的回调，每加载一个对象，就调用一次
    /// <param name="finishDone"></param> 加载完成的回调
    /// <param name="autoClearPreList"></param> 是否在加载完成后，自动清除加载队列
    /// <returns></returns>
    public static int StartPreLoad(Action<int> refreshDone, Action finishDone, bool autoClearPreList = true)
    {
        return ResMgr.Inst.StartPreLoad(refreshDone, finishDone, autoClearPreList);
    }

    //清理预加载逻辑
    public static void ClearPreList()
    {
        ResMgr.Inst.ClearPreList();
    }
    #endregion
}

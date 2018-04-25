using UnityEngine;
using System.IO;
using LuaInterface;

/// <summary>
/// 集成自LuaFileUtils，重写里面的ReadFile，
/// </summary>
public class LuaLoader : LuaFileUtils
{
    // Use this for initialization
    public LuaLoader()
    {
        instance = this;
    }

    /// <summary>
    /// 添加打入Lua代码的AssetBundle
    /// </summary>
    /// <param name="bundle"></param>
    public void AddBundle(string bundleName)
    {
        AssetBundle bundle = ResMgr.Inst.InitAssetBundle(bundleName);
        if (bundle != null)
        {
            base.AddSearchBundle(bundleName.ToLower(), bundle);
        }
    }
}

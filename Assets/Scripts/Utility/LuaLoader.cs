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

        string url = CommonUtils.GetABPath(bundleName.ToLower());
        var bytes = File.ReadAllBytes(url);
        AssetBundle bundle = AssetBundle.LoadFromMemory(bytes);
        if (bundle != null)
        {
            base.AddSearchBundle(bundleName.ToLower(), bundle);
        }
    }
}

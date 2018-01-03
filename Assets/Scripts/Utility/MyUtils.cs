using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using LuaInterface;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class MyUtils
{
    #region 添加Child
    public static GameObject AddChild(this GameObject parent) { return parent.AddChild(-1); }
    public static GameObject AddChild(this GameObject parent, int layer)
    {
        GameObject go = new GameObject();
        if (parent != null)
        {
            Transform t = go.transform;
            t.parent = parent.transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            if (layer == -1) go.layer = parent.layer;
            else if (layer > -1 && layer < 32) go.layer = layer;
        }
        return go;
    }
    #endregion

    #region 路径管理
    /// 获取资源的路径
    public static string ResFullPath(string fileName, string suffix = "")
    {
        if (!string.IsNullOrEmpty(suffix))
        {
            fileName += suffix;
        }
        return ResDir() + fileName;
    }

    /// 取得数据存放目录
    public static string ResDir()
    {
        if (GameMain.Inst._developMode)
        {
            return "Assets/Res/";
        }
        else
        {
#if UNITY_EDITOR
            return Application.streamingAssetsPath + "/";
#else
                return Application.persistentDataPath +"/";
#endif
        }
    }

    /// 应用程序内容路径：打包的时候，资源都先打到该位置，第一次运行游戏需要解压到ResPath
    public static string AppReserveResDir()
    {
        string path = Application.streamingAssetsPath + "/";
        return path;
    }

    #endregion

    public static string ConstraintABName(string abName)
    {
        if (!abName.EndsWith(AppConst.ExtName))
        {
            abName += AppConst.ExtName;
        }
        return abName;
    }

    public static string Uid(string uid)
    {
        int position = uid.LastIndexOf('_');
        return uid.Remove(0, position + 1);
    }

    public static long GetTime()
    {
        TimeSpan ts = new TimeSpan(DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1, 0, 0, 0).Ticks);
        return (long)ts.TotalMilliseconds;
    }

    /// <summary>
    /// 计算字符串的MD5值
    /// </summary>
    public static string Md5(string source)
    {
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        byte[] data = System.Text.Encoding.UTF8.GetBytes(source);
        byte[] md5Data = md5.ComputeHash(data, 0, data.Length);
        md5.Clear();

        string destString = "";
        for (int i = 0; i < md5Data.Length; i++)
        {
            destString += System.Convert.ToString(md5Data[i], 16).PadLeft(2, '0');
        }
        destString = destString.PadLeft(32, '0');
        return destString;
    }

    /// <summary>
    /// 计算文件的MD5值
    /// </summary>
    public static string Md5file(string filePath)
    {
        try
        {
            FileStream fs = new FileStream(filePath, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(fs);
            fs.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("md5file() fail, error:" + ex.Message);
        }
    }

    /// <summary>
    /// 清除所有子节点
    /// </summary>
    public static void ClearChild(Transform go)
    {
        if (go == null) return;
        for (int i = go.childCount - 1; i >= 0; i--)
        {
            GameObject.Destroy(go.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// 清理内存
    /// </summary>
    public static void ClearMemory()
    {
        GC.Collect(); Resources.UnloadUnusedAssets();
        LuaMgr.Inst.LuaGC();
    }

    /// <summary>
    /// 取得行文本
    /// </summary>
    public static string GetFileText(string path)
    {
        return File.ReadAllText(path);
    }

    /// <summary>
    /// 网络可用
    /// </summary>
    public static bool NetAvailable
    {
        get
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
    }

    /// <summary>
    /// 是否是无线
    /// </summary>
    public static bool IsWifi
    {
        get
        {
            return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
        }
    }




    /// <summary>
    /// 防止初学者不按步骤来操作
    /// </summary>
    /// <returns></returns>
    public static int CheckRuntimeFile()
    {
        if (!Application.isEditor) return 0;
        string streamDir = Application.dataPath + "/StreamingAssets/";
        if (!Directory.Exists(streamDir))
        {
            return -1;
        }
        else
        {
            string[] files = Directory.GetFiles(streamDir);
            if (files.Length == 0) return -1;

            if (!File.Exists(streamDir + "files.txt"))
            {
                return -1;
            }
        }
        string sourceDir = Application.dataPath + "/ToLua/Source/Generate/";
        if (!Directory.Exists(sourceDir))
        {
            return -2;
        }
        else
        {
            string[] files = Directory.GetFiles(sourceDir);
            if (files.Length == 0) return -2;
        }
        return 0;
    }

    /// <summary>
    /// 执行Lua方法
    /// </summary>
    public static object[] CallMethod(string module, string func, params object[] args)
    {
        return LuaMgr.Inst.CallFunction(module + "." + func, args);
    }

    /// <summary>
    /// 检查运行环境
    /// </summary>
    public static bool CheckEnvironment()
    {
#if UNITY_EDITOR
        int resultId = CheckRuntimeFile();
        if (resultId == -1)
        {
            Debug.LogError("没有找到框架所需要的资源，单击Game菜单下Build xxx Resource生成！！");
            EditorApplication.isPlaying = false;
            return false;
        }
        else if (resultId == -2)
        {
            Debug.LogError("没有找到Wrap脚本缓存，单击Lua菜单下Gen Lua Wrap Files生成脚本！！");
            EditorApplication.isPlaying = false;
            return false;
        }
        if (Application.loadedLevelName == "Test" && !GameMain.Inst._developMode)
        {
            Debug.LogError("测试场景，必须打开调试模式，GameMain.Inst._developMode = true！！");
            EditorApplication.isPlaying = false;
            return false;
        }
#endif
        return true;
    }
}

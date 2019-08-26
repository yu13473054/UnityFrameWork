using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using Object = UnityEngine.Object;
using System.Text.RegularExpressions;

public static class CommonUtils
{
    #region Child
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
    /// <summary>
    /// 清除所有子节点
    /// </summary>
    public static void ClearChild(Transform go)
    {
        if (go == null) return;
        for (int i = go.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(go.GetChild(i).gameObject);
        }
    }
    #endregion

    #region 路径管理
    /// <summary>
    /// 获取ab所在的位置
    /// </summary>
    /// <param name="abName"></param>
    /// <returns></returns>
    public static string GetABPath(string abName)
    {
        string abPath = "";
#if UNITY_EDITOR
        abPath = GameMain.Inst.appABPath + abName;
#else
        abPath = AppConst.localABPath + abName;
#endif
        //先在本地缓存中查找文件，如果没有，就从app中直接获取
        if (!File.Exists(abPath))
        {
            abPath = GameMain.Inst.appABPath + abName;
        }
        return abPath;
    }
    #endregion

    #region 文件读取
    /// 直接从txt中获取对应的内容
    public static string ReadFileText(string filepath)
    {
        if (!File.Exists(filepath))
        {
            Debug.LogErrorFormat("<CommonUtils> 读取文件失败：{0}", filepath);
            return null;
        }
        using (StreamReader reader = new StreamReader(filepath))
        {
            return reader.ReadToEnd();
        }
    }
    public static byte[] ReadFileBytes(string filepath)
    {
        string text = ReadFileText(filepath);
        return Encoding.UTF8.GetBytes(text);
    }
    #endregion

    public static long GetTime()
    {
        TimeSpan ts = new TimeSpan(DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1, 0, 0, 0).Ticks);
        return (long)ts.TotalMilliseconds;
    }

    #region MD5
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

    #endregion

    /// <summary>
    /// 清理内存
    /// </summary>
    public static void ClearMemory()
    {
        GC.Collect(); 
        Resources.UnloadUnusedAssets();
        LuaMgr.Inst.LuaGC();
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
    /// 判断是否是汉字
    /// </summary>
    public static bool IsChinese(char c)
    {
        if (c >= 0x4e00 && c <= 0x9fbb)
            return true;
        return false;
    }

    /// <summary>
    /// 返回字符串的宽度：汉字为两个字符，英文为一个字符
    /// </summary>
    public static int StrLenWidth(string str)
    {
        int len = 0;
        char[] chars = str.ToCharArray();
        for(int i =0; i<chars.Length; i++)
        {
            if (IsChinese(chars[i]))
                len += 2;
            else
                len++;
        }
        return len;
    }

    /// <summary>
    /// 检查名称是否合法：只有中文数字字母
    /// </summary>
    public static bool NameCheck(string str)
    {
        return Regex.IsMatch(str, @"[^a-zA-Z0-9\u4E00-\u9Fbb]");
    }
}

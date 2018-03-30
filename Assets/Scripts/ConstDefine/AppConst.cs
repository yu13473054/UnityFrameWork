using UnityEngine;

public class AppConst
{
    // 初始化基础配置
    public static void Init()
    {
        // app配置
        ConfigHandler handler = ConfigHandler.Open(appABPath + "app.txt");
        updateHost = handler.ReadValue("UpdateHost");
        loginHost = handler.ReadValue("LoginHost");
    }

    public static string updateHost = "";
    public static string loginHost = "";

    // 包内Steaming地址
#if UNITY_ANDROID && !UNITY_EDITOR //Android上直接取得到的路径有“jar:file///”前缀，不正确
    public static string appABPath = Application.dataPath + "!assets/";
#else
    public static string appABPath = Application.streamingAssetsPath + "/";
#endif

    //本地资源路径
    public static string localABPath = Application.persistentDataPath + "/AB/";

    // 平台名称
#if UNITY_STANDALONE_WIN
    public static string platformName = "win";
#elif UNITY_STANDALONE_OSX
    public static string platformName = "osx";
#elif UNITY_ANDROID
    public static string platformName = "android";
#elif UNITY_IPHONE
    public static string platformName = "ios";
#endif





    public const string ExtName = ".ab";                   //素材包扩展名
    public const string AssetDir = "StreamingAssets";           //素材包打包目录 

#region 资源路径管理
    public const string DataDir = "assetdata/";            //数据文件夹
    public const string DataABName = "assetdata.ab";         //数据ab包
#endregion

    public const bool LuaBundleMode = false;                    //Lua代码AssetBundle模式

    /// <summary>
    /// 如果开启更新模式，前提必须启动框架自带服务器端。
    /// 否则就需要自己将StreamingAssets里面的所有内容
    /// 复制到自己的Webserver上面，并修改下面的WebUrl。
    /// </summary>
    public const bool UpdateMode = false;                       //更新模式-默认关闭 
    public const string WebUrl = "http://localhost:6688/";      //测试更新地址





    public const bool LuaByteMode = false;                       //Lua字节码模式-默认关闭 

    public const int TimerInterval = 1;

    public const string LuaTempDir = "Lua/";                    //临时目录

    public static string UserId = string.Empty;                 //用户ID
    public static int SocketPort = 0;                           //Socket服务器端口
    public static string SocketAddress = string.Empty;          //Socket服务器地址
}

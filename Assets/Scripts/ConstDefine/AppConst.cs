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
    public static string appABPath = Application.streamingAssetsPath + "/assetbundle/";
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
}

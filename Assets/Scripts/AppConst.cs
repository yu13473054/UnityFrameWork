using UnityEngine;

public class AppConst
{
    // 初始化基础配置
    public static void Init()
    {
        // app配置
        ConfigHandler handler = ConfigHandler.Open(appABPath.Replace("/assetbundle","") + "app.txt");
        updateHost = handler.ReadValue("UpdateHost");
        loginHost = handler.ReadValue("LoginHost");
        platID = int.Parse(handler.ReadValue("PlatID"));
        encrypt = handler.ReadValue("Encrypt").Equals("1");
    }

    public static string updateHost = "";
    public static string loginHost = "";
    public static int platID;
    public static bool encrypt;
    // 包内Steaming地址
    public static string appABPath = Application.streamingAssetsPath + "/assetbundle/";

    //本地资源路径
    public static string localABPath = Application.persistentDataPath + "/";

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

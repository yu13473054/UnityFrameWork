using UnityEngine;

public class AppConst
{
    // 初始化基础配置
    public static void Init()
    {
        // app配置
        ConfigHandler handler = ConfigHandler.Open(Application.streamingAssetsPath + "/app.txt");
        updateHost = handler.ReadValue("UpdateHost");
        loginHost = handler.ReadValue("LoginHost");
        platID = int.Parse(handler.ReadValue("PlatID"));
    }

    public static string updateHost = "";
    public static string loginHost = "";
    public static int platID;

}

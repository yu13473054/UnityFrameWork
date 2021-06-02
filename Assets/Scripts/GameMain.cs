using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public enum LngType
{
    Auto = 0, //根据系统语言确定
    CN,
    EN,
    JP
}
public enum ResMode
{
    Develop = 0,
    AssetBundle,
    ExtraData //使用pc端出包后，数据单独获取
}

[ExecuteInEditMode]
public class GameMain : MonoBehaviour
{
    public static GameMain Inst { get; private set; }

    [SerializeField] [Label("帧率")]
    private int _targetFrameRate = 60;
    // 0 : 读本地 
    // 1 : 读ab 
    // 2 : txt资源从外部项目根目录Data中获取，其他资源从AB中获取
    [SerializeField]
    [Label("游戏模式")]
    private ResMode _resourceMode = ResMode.Develop;
    [Label("显示日志")]
    public bool showLog = true;
    [SerializeField][Label("语言类型")]
    private LngType _lngType = LngType.Auto;

    [SerializeField][Label("音效上限")]
    private int _audioLimit = 6;

    // 包内Steaming地址
    public string appABPath;
    //本地资源路径
    public string localABPath;

    // 平台名称
    public string platformName = "win";

    public string updateHost;
    public string loginHost;
    public string port;
    public int platID;

    // 适配的偏移距离
    public int viewOffstPixel = 0;

    public int TargetFrameRate
    {
        get { return _targetFrameRate; }
    }
    public ResMode ResourceMode
    {
        get { return _resourceMode; }
    }
    public LngType lngType
    {
        get { return _lngType; }
        set { _lngType = value; }
    }
    public int AudioLimit
    {
        get { return _audioLimit; }
    }

    private int _runPlatform;
    public int RunPlatform
    {
        get { return _runPlatform; }
    }

    void Awake()
    {
        Inst = this;

#if UNITY_STANDALONE_WIN
        platformName = "win";
#elif UNITY_STANDALONE_OSX
        platformName = "osx";
#elif UNITY_ANDROID
        platformName = "android";
#elif UNITY_IPHONE
        platformName = "ios";
#endif

        localABPath = Application.persistentDataPath + "/";
        appABPath = Application.streamingAssetsPath + "/assetbundle/";

        if (!Application.isPlaying) return;

        _runPlatform = (int)Application.platform;

#if !UNITY_EDITOR   //读取配置文件
        InitWithConfig();
#endif

        gameObject.AddComponent<Boot>();
    }

    public void InitWithConfig()
    {
        ConfigHandler config = ConfigHandler.Open(CommonUtils.GetABPath("config.txt"));
        //是否配置多语言
        string str = config.ReadValue("lngType", "");
        if (!string.IsNullOrEmpty(str))
        {
            _lngType = (LngType)int.Parse(str);
        }
        Debug.Log(_lngType);
        //音效上限
        str = config.ReadValue("AudioLimit", "");
        if (!string.IsNullOrEmpty(str))
        {
            _audioLimit = int.Parse(str);
        }
        //热更地址
        str = config.ReadValue("updateHost", "");
        if (!string.IsNullOrEmpty(str))
        {
            updateHost = str;
        }
        //登录地址
        str = config.ReadValue("loginHost", "");
        if (!string.IsNullOrEmpty(str))
        {
            loginHost = str;
        }
        //登录端口号
        str = config.ReadValue("port", "");
        if (!string.IsNullOrEmpty(str))
        {
            port = str;
        }
        //平台id
        str = config.ReadValue("platID", "");
        if (!string.IsNullOrEmpty(str))
        {
            platID = int.Parse(str);
        }
    }

}



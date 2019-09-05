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

[ExecuteInEditMode]
public class GameMain : MonoBehaviour
{
    enum ResMode
    {
        Develop = 0,
        AssetBundle,
        ExtraData //使用pc端出包后，数据单独获取
    }

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

    // 包内Steaming地址
    public string appABPath;
    //本地资源路径
    public string localABPath;

    // 平台名称
    #if UNITY_STANDALONE_WIN
    public string platformName = "win";
    #elif UNITY_STANDALONE_OSX
    public string platformName = "osx";
#elif UNITY_ANDROID
    public string platformName = "android";
#elif UNITY_IPHONE
    public string platformName = "ios";
#endif

    public string updateHost;
    public string loginHost;
    public int platID;

    public int TargetFrameRate
    {
        get { return _targetFrameRate; }
    }
    public int ResourceMode
    {
        get { return (int)_resourceMode; }
    }
    public LngType lngType
    {
        get { return _lngType; }
        set { _lngType = value; }
    }

    void Awake()
    {
        Inst = this;

        localABPath = Application.persistentDataPath + "/";
        appABPath = Application.streamingAssetsPath + "/assetbundle/";

        if (!Application.isPlaying) return;
        gameObject.AddComponent<Boot>();
    }

}



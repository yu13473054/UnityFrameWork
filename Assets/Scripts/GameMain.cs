using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[ExecuteInEditMode]
public class GameMain : MonoBehaviour
{
    public static GameMain Inst { get; private set; }

    [SerializeField][Label("帧率")]
    private int _targetFrameRate = 60;
    // 0 : 读本地 
    // 1 : 读ab 
    // 2 : txt资源从外部项目根目录Data中获取，其他资源从AB中获取
    [SerializeField]
    [Label("游戏模式")]
    private int _resourceMode = 0;
    [SerializeField][Label("显示日志")]
    public bool showLog = true;
    [Label("语言类型")]
    public int lngType = 0;

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
        get { return _resourceMode; }
    }

    void Awake()
    {
        Inst = this;

        localABPath = Application.persistentDataPath + "/";
        appABPath = Application.streamingAssetsPath + "/assetbundle/";
    }

}



using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class GameMain : MonoBehaviour
{
    #region 初始化
    private static GameMain _inst;
    public static GameMain Inst
    {
        get { return _inst; }
    }
    #endregion

    #region 游戏属性
    [SerializeField][Label("帧率")]
    private int _targetFrameRate = 30;
    // 0 : 读本地 
    // 1 : 读ab 
    // 2 : txt资源从外部项目根目录Data中获取，其他资源从AB中获取
    [SerializeField]
    [Label("游戏模式", 0, 2)]
    private int _resourceMode = 0;


    public int TargetFrameRate
    {
        get { return _targetFrameRate; }
    }
    public int ResourceMode
    {
        get { return _resourceMode; }
    }
    #endregion

    /// <summary>
    /// 初始化游戏管理器
    /// </summary>
    void Awake()
    {
        _inst = this;
        DontDestroyOnLoad(gameObject);  //防止销毁自己

        //游戏属性设置
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = _targetFrameRate;
    }

    void Start()
    {
        if (_resourceMode == 0)
        {
            //开发者模式下，直接启动游戏
            OnResourceInited();
        }
        else
        {
            MgrInit();//先把管理器启动起来，资源更新完毕后，重启管理器
            //启动更新器
            ResourceUpdate resourceUpdate = gameObject.AddComponent<ResourceUpdate>();
            resourceUpdate.onResourceInited = OnResourceInited;
        }
    }


    public void ReBoot()
    {
        MgrRelease();
    }

    void OnUpdateFailed(string hintText)
    {
    }

    /// 开始下载
    void BeginDownload()
    {

    }

    void OnSingleDownloadCompleted()
    {

    }

    /// 所有文件下载完成
    void OnAllDownloadCompleted()
    {
    }

    /// 资源初始化结束
    void OnResourceInited()
    {
        MgrInit();
        OnInitialize();
    }

    private void MgrInit()
    {
        PrefMgr.Init();
        ResMgr.Init();
        DatabaseMgr.Init();

        UIMgr.Init();
        AudioMgr.Init();
        TimerMgr.Init();
        NetworkMgr.Init();
        PoolMgr.Init();

        LuaMgr.Init();
    }

    void MgrRelease()
    {
        if (DatabaseMgr.Inst != null) DestroyImmediate(DatabaseMgr.Inst);
        if (TimerMgr.Inst != null) DestroyImmediate(TimerMgr.Inst);
        if (AudioMgr.Inst != null) DestroyImmediate(AudioMgr.Inst);
        if (PrefMgr.Inst != null) DestroyImmediate(PrefMgr.Inst);
        if (PrefMgr.Inst != null) DestroyImmediate(UIMgr.Inst);
        if (ResMgr.Inst != null) DestroyImmediate(ResMgr.Inst);
        if (NetworkMgr.Inst != null) DestroyImmediate(NetworkMgr.Inst);
        if (PoolMgr.Inst != null) DestroyImmediate(PoolMgr.Inst);
        if (LuaMgr.Inst != null) DestroyImmediate(LuaMgr.Inst);
    }

    void OnInitialize()
    {
//        LuaMgr.Inst.InitStart();


        //            LuaMgr.Inst.DoFile("Logic/Game");         //加载游戏
        //            LuaMgr.Inst.DoFile("Logic/Network");      //加载网络
        //            NetworkMgr.Inst.OnInit();                     //初始化网络
        //
        //
        //            CommonUtils.CallMethod("Game", "OnInitOK");     //初始化完成
    }

    /// 析构函数
    void OnDestroy()
    {
        if (NetworkMgr.Inst != null)
        {
            NetworkMgr.Inst.Unload();
        }
    }
}



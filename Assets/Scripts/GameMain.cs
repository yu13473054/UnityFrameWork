using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;


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
    [SerializeField][Label("显示日志")]
    private bool _showLog = true;

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

        //显示日志
        if (_showLog)
        {
            Transform rootReporter = GameObject.Find("Reporter").transform;
            for (int i = 0; i < rootReporter.childCount; i++)
            {
                rootReporter.GetChild(i).gameObject.SetActive(_showLog);
            }
        }
    }

    void Start()
    {
//        StartCoroutine(Init(CheckUpdate));

        StartCoroutine(GenerateQRCode());
    }

    private IEnumerator GenerateQRCode()
    {
        WWW www = new WWW("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=wx0c0c31b49819cca6&secret=45d74e925baac11ca875fb5b978cce87");
        yield return www;
        string access_token = JsonUtility.FromJson<AA>(www.text).access_token;


        WWWForm form = new WWWForm();
        form.AddField("scene", "2341");
        form.AddField("width", 430);
        form.AddField("auto_color", "false");
        WWWForm form1 = new WWWForm();
        form1.AddField("r","0");
        form1.AddField("g","0");
        form1.AddField("b","0");
        form.AddField("line_color", Encoding.Default.GetString(form1.data));
        Debug.Log(Encoding.Default.GetString(form.data));
        WWW www1 = new WWW("https://api.weixin.qq.com/wxa/getwxacodeunlimit?access_token="+access_token, form);
        yield return www1;
        if (string.IsNullOrEmpty(www1.error))
        {
            Debug.Log(www1.text);
        }
        else
        {
            Debug.Log(www1.error);
        }


//        JsonUtility.FromJson<BB>(www1.text);

    }

    [System.Serializable]
    class AA
    {
        public string access_token;
        public int expires_in;
    }

    [System.Serializable]
    class BB
    {
        public string data;
    }


    private IEnumerator Init(Action endAction)
    {
        AppConst.Init();
        gameObject.AddComponent<PrefMgr>();
        gameObject.AddComponent<ResMgr>();
        gameObject.AddComponent<DatabaseMgr>();
        gameObject.AddComponent<UIMgr>();
        gameObject.AddComponent<AudioMgr>();
        gameObject.AddComponent<TimerMgr>();
        gameObject.AddComponent<LuaMgr>();
        gameObject.AddComponent<NetworkMgr>();
        Localization.Init();
        yield return null;

        endAction();
    }

    void CheckUpdate()
    {
        GameStart();
        return;


        if (_resourceMode != 0)
        {
            //启动更新器
            ResourceUpdate resourceUpdate = gameObject.AddComponent<ResourceUpdate>();
            resourceUpdate.onUpdateEnd = OnUpdateEnd;
        }
        else //直接启动游戏
        {
            GameStart();
        }
    }

    void GameStart()
    {
        LuaMgr.Inst.InitStart();
    }

    private void OnUpdateEnd()
    {
        //重启
        MgrRelease();
        StartCoroutine(Init(GameStart));
    }

    void MgrRelease()
    {
        if (DatabaseMgr.Inst != null) DestroyImmediate(DatabaseMgr.Inst);
        if (TimerMgr.Inst != null) DestroyImmediate(TimerMgr.Inst);
        if (AudioMgr.Inst != null) DestroyImmediate(AudioMgr.Inst);
        if (PrefMgr.Inst != null) DestroyImmediate(PrefMgr.Inst);
        if (PrefMgr.Inst != null) DestroyImmediate(UIMgr.Inst);
        if (ResMgr.Inst != null) DestroyImmediate(ResMgr.Inst);
//        if (PoolMgr.Inst != null) PoolMgr.Inst.OnDestory();
        if (NetworkMgr.Inst != null) DestroyImmediate(NetworkMgr.Inst);
        if (LuaMgr.Inst != null) DestroyImmediate(LuaMgr.Inst);
        Localization.OnDestroy();
    }

    /// 析构函数
    void OnDestroy()
    {
    }
}



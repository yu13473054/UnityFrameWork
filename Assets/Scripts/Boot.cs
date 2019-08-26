using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class Boot : MonoBehaviour
{
    void Awake()
    {
        if (!Application.isPlaying) return;
        DontDestroyOnLoad(gameObject);  //防止销毁自己

        //游戏属性设置
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = GameMain.Inst.TargetFrameRate;

        //显示日志
        if (GameMain.Inst.showLog)
        {
            Transform rootReporter = GameObject.Find("Reporter").transform;
            for (int i = 0; i < rootReporter.childCount; i++)
            {
                rootReporter.GetChild(i).gameObject.SetActive(true);
            }
        }
    }

    void Start()
    {
        StartCoroutine(Init(CheckUpdate));
    }

    private IEnumerator Init(Action endAction)
    {
        //如果是通用语言，则根据手机的语言系统进行语言的选择
        if (GameMain.Inst.lngType == 0)
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                case SystemLanguage.ChineseTraditional:
                    GameMain.Inst.lngType = 1;
                    break;
                case SystemLanguage.English:
                    GameMain.Inst.lngType = 2;
                    break;
                case SystemLanguage.Japanese:
                    GameMain.Inst.lngType = 3;
                    break;
                default:
                    GameMain.Inst.lngType = 1;//默认使用中文
                    break;
            }
        }
        gameObject.AddComponent<PrefMgr>();
        gameObject.AddComponent<ResMgr>();
        ResMgr.Inst.Init();
        gameObject.AddComponent<DatabaseMgr>();
        gameObject.AddComponent<UIMgr>();
        gameObject.AddComponent<AudioMgr>();
        gameObject.AddComponent<TimerMgr>();
        gameObject.AddComponent<LuaMgr>();
        LuaMgr.Inst.Init();
        gameObject.AddComponent<NetworkMgr>();
        Localization.Init();
        yield return null;

        endAction();
    }

    void CheckUpdate()
    {
        GameStart();
        return;

        if (GameMain.Inst.ResourceMode != 0)
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
        LuaMgr.Inst.Call("LuaStart");
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
        Localization.isInited = false;
    }

    /// 析构函数
    void OnDestroy()
    {
    }
}



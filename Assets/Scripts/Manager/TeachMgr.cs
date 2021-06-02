using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeachMgr : MonoBehaviour
{
    private static TeachMgr _inst;
    public static TeachMgr instance
    {
        get { return _inst; }
    }

    //同步服务器状态定义
    private const int SYCN_NOT = 0;
    private const int SYCN_SELF = 1;
    private const int SYCN_FINISH = 999;

    public bool isTeaching { get; private set; } //是否正在进行教程


    private Dictionary<int, bool> _finishDic;
    private TeachStructParser _structParser; //正在进行的结构对象
    private int _stepNum;  //教程进行到的步数，从0开始

    void Awake()
    {
        _inst = this;
        _finishDic = new Dictionary<int, bool>();
    }

    /// <summary>
    /// 教程管理器初始化接口，游戏启动的第一步。从服务器获取到教程相关数据。
    /// 服务器需要记录的数据：哪些类别的教程已经完成、正在进行中的教程，处理到哪一步
    /// </summary>
    public void InitData()
    {
        //---------模拟服务器数据 开始-----------
        //服务器需要下发每个struct的sn，同时需要发送该Struct记录的StepSn
        List<ServerData> serList = new List<ServerData>()
        {
            new ServerData(10001, 1000102)
        };
        //---------模拟服务器数据 结束-----------

        //开始数据解析
        for (int i = 0; i < serList.Count; i++)
        {
            ServerData serverData = serList[i];
            TeachStepParser stepParser = Data.Inst.TeachStep(serverData.stepSn);
            if (stepParser.syncSn == SYCN_FINISH) // 教程已经完成
            {
                _finishDic.Add(serverData.structSn, true); 
            }
            else //有正在进行中的教程
            {
                if (isTeaching)
                {
                    Debug.LogError("<TeachMgr> 已经有进行中的教程，数据错误，请检查！");
                    return;
                }
                isTeaching = true;
                _structParser = Data.Inst.TeachStruct(serverData.structSn);
                for (int j = 0; j < _structParser.steps.Length; j++)
                {
                    if (_structParser.steps[j] == serverData.stepSn)
                    {
                        _stepNum = j;
                        break;
                    }
                }
            }
        }
    }

    //TODO 教程注册

    //教程下一步
    public void Next()
    {
        if (!isTeaching) return;
        //同步服务器
        SycnServer(_structParser.sn, _structParser.steps[_stepNum]); 
        _stepNum++;
        if (_stepNum >= _structParser.steps.Length)
        {
            Finish(); 
            //TODO 当前教程结束后，如何自动下一个教程
        }
        //TODO 显示方式：等待、立即显示

    }

    public void Show()
    {

    }

    //同步数据到服务器
    public void SycnServer(int structSn, int stepSn)
    {
        if (_finishDic.ContainsKey(structSn)) return; //教程已经完成了，不需要同步到服务器
        TeachStepParser stepParser = Data.Inst.TeachStep(stepSn);
        if (stepParser.syncSn == SYCN_FINISH)  //本地记录下已完成
        {
            _finishDic.Add(structSn, true);
        }
        else if (stepParser.syncSn == SYCN_NOT) //不需要同步给服务器
        {
            return;
        }
        //同步服务器
//        SendMsg(structSn, stepSn);
    }

    ///教程完成
    private void Finish()
    {
        isTeaching = false;
        _stepNum = 0;
        _structParser = null;
    }
}

//模拟的服务器的数据结构
public class ServerData
{
    public int structSn; //TeachStructDataSn
    public int stepSn;   //TeachStepDataSn

    public ServerData(int structSn, int stepSn)
    {
        this.structSn = structSn;
        this.stepSn = stepSn;
    }
}

//教程的父类
public abstract class Teach
{

    public virtual void Init(int teachSn)
    {

    }

    /// <summary>
    /// 记录每一步需要如何处理：显示界面？逻辑判断？
    /// </summary>
    /// <returns></returns>
    public abstract int Step();

    /// <summary>
    /// 进行下一步
    /// </summary>
    /// <returns></returns>
    public abstract int Next();
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TimerElement
{
    /// <summary>
    /// 0:CD Timer  1:Frame Timer
    /// </summary>
    public int type = -1;
    public object key;//注册key
    public bool isTimeScale = true;
    public int loopTimes;//当前已经循环次数
    public int loopCountLimit = 1; //循环次数上限
    public float life = -1; //生命周期
    public TimerEleCallBack callback;
    public TimerEleCallBack endCB;

    //时间计时器独有参数
    public float duration;// 时间间隔
    public float totalExecuteTime;//执行总时长

    public bool isFinish { get; private set; }
    private float _currTime;

    //执行计时器
    public void Execute()
    {
        float deltaTime = isTimeScale ? Time.deltaTime : Time.unscaledDeltaTime;
        totalExecuteTime += deltaTime;
        if (type == 0)//cd Timer
        {
            _currTime += deltaTime;
            if (_currTime >= duration)
            {
                _currTime -= duration;
                loopTimes++;
                callback(this);
            }
        }
        else if (type == 1)// frame Timer
        {
            loopTimes++;//记录执行了多少帧
            callback(this);
        }

        if (loopCountLimit > 0 && loopTimes == loopCountLimit)//达到了指定的循环次数
        {
            isFinish = true;
            if(endCB != null)
                endCB(this);
        }

        if(life > 0 && totalExecuteTime >= life)
        {
            isFinish = true;
            if (endCB != null)
                endCB(this);
        }
    }

    public void OnDestroy()
    {
        type = -1;
        isTimeScale = true;
        loopTimes = 0;
        loopCountLimit = 1;
        life = -1;
        callback = null;

        duration = 0;
        totalExecuteTime = 0;

        isFinish = false;
        _currTime = 0;
    }
}
public delegate void TimerEleCallBack(TimerElement element);

public class TimerMgr : MonoBehaviour
{
    #region 初始化
    private static TimerMgr _inst;
    public static TimerMgr Inst
    {
        get
        {
            return _inst;
        }
    }
    #endregion

    private List<TimerElement> _timerList;
    private Stack<TimerElement> _elementPool;

    void Awake()
    {
        _inst = this;
        DontDestroyOnLoad(gameObject);

        _timerList = new List<TimerElement>();
        _elementPool = new Stack<TimerElement>();
    }

    void Update()
    {
        for (int i = 0; i < _timerList.Count; i++)
        {
            TimerElement element = _timerList[i];
            element.Execute();
            if (element.isFinish)
            {
                RemoveTimer(i);
                i--;
            }
        }
    }

    /// <summary>
    /// 每隔多长时间执行一次
    /// </summary>
    /// <param name="key">注册key</param>
    /// <param name="duration">间隔时间</param>
    /// <param name="callBack">事件回调</param>
    /// <param name="isTimeScale">是否受TimeScale影响</param>
    /// <param name="loopCount">循环次数，小于0为无限循环</param>
    /// <param name="life">生命周期，如果同时指定loopCount，谁先到执行finish</param>
    /// <param name="endCB">结束回调</param>
    /// <param name="immRun">立即执行</param>
    public void RegisterCDTimer(object key, float duration, TimerEleCallBack callBack, bool isTimeScale = true, bool immRun = false,
        int loopCount = 1, float life = -1, TimerEleCallBack endCB = null)
    {
        if (callBack == null)
        {
            return;
        }
        TimerElement element = GetFreeElement();
        element.type = 0;
        element.key = key;
        element.duration = duration;
        element.isTimeScale = isTimeScale;
        element.callback = callBack;
        element.loopCountLimit = loopCount;
        element.life = life;
        element.endCB = endCB;

        if (immRun)
            element.Execute();

        if (!element.isFinish)
            _timerList.Add(element);
        else
            element.OnDestroy();
    }

    /// <summary>
    /// 每帧执行回调函数
    /// </summary>
    public object RegisterFrameTimer(TimerEleCallBack callBack, int loopCount = -1, bool isTimeScale = true, bool immRun = false,
        float life = -1, TimerEleCallBack endCB = null)
    {
        TimerElement element = GetFreeElement();
        element.type = 1;
        element.key = GetUid();
        element.isTimeScale = isTimeScale;
        element.loopCountLimit = loopCount;
        element.callback = callBack;
        element.life = life;
        element.endCB = endCB;

        if (immRun)
            element.Execute();

        if (!element.isFinish)
            _timerList.Add(element);
        else
            element.OnDestroy();

        return element.key;
    }

    /// <summary>
    /// 移除事件
    /// </summary>
    /// <param name="key"></param>
    public void RemoveTimer(object key)
    {
        for (int i = 0; i < _timerList.Count; i++)
        {
            if (_timerList[i].key == key)
            {
                RemoveTimer(i);
                break;
            }
        }
    }

    private void RemoveTimer(int index)
    {
        TimerElementRelease(_timerList[index]);
        _timerList.RemoveAt(index);
    }

    /// <summary>
    /// 得到一个可复用的对象
    /// </summary>
    /// <returns></returns>
    private TimerElement GetFreeElement()
    {
        TimerElement element;
        if (_elementPool.Count > 0)
        {
            element = _elementPool.Pop();
        }
        else
        {
            element = new TimerElement();
        }
        return element;
    }

    private void TimerElementRelease(TimerElement element)
    {
        element.OnDestroy();
        _elementPool.Push(element);
    }

    private uint _idIndex = 0;
    private string GetUid()
    {
        _idIndex++;
        return _idIndex.ToString();
    }


    void OnDestroy()
    {
        for (int i = 0; i < _timerList.Count; i++)
        {
            _timerList[i].callback = null;
        }
        _timerList.Clear();
        _elementPool.Clear();

        Debug.Log("<TimerMgr> OnDestroy");
    }

}
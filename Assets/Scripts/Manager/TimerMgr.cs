using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TimerElement
{
    /// <summary>
    /// 0:CD Timer  1:Frame Timer
    /// </summary>
    public int type = -1;
    public object key;//ע��key
    public bool isTimeScale = true;
    public int loopTimes;//��ǰ�Ѿ�ѭ������
    public int loopCountLimit = 1; //ѭ����������
    public float life = -1; //��������
    public TimerEleCallBack callback;
    public TimerEleCallBack endCB;

    //ʱ���ʱ�����в���
    public float duration;// ʱ����
    public float totalExecuteTime;//ִ����ʱ��

    public bool isFinish { get; private set; }
    private float _currTime;

    //ִ�м�ʱ��
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
            loopTimes++;//��¼ִ���˶���֡
            callback(this);
        }

        if (loopCountLimit > 0 && loopTimes == loopCountLimit)//�ﵽ��ָ����ѭ������
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
    #region ��ʼ��
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
    /// ÿ���೤ʱ��ִ��һ��
    /// </summary>
    /// <param name="key">ע��key</param>
    /// <param name="duration">���ʱ��</param>
    /// <param name="callBack">�¼��ص�</param>
    /// <param name="isTimeScale">�Ƿ���TimeScaleӰ��</param>
    /// <param name="loopCount">ѭ��������С��0Ϊ����ѭ��</param>
    /// <param name="life">�������ڣ����ͬʱָ��loopCount��˭�ȵ�ִ��finish</param>
    /// <param name="endCB">�����ص�</param>
    /// <param name="immRun">����ִ��</param>
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
    /// ÿִ֡�лص�����
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
    /// �Ƴ��¼�
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
    /// �õ�һ���ɸ��õĶ���
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
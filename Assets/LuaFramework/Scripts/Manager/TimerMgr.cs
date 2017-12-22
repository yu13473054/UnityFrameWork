using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LuaFramework {

    public delegate void TimerEleCallBack(TimerElement element);

    public class TimerMgr : MonoBehaviour {
        private static TimerMgr _inst;
        public static TimerMgr Inst
        {
            get
            {
                return _inst;
            }
        }

        //初始化的时候调用
        public static void Init()
        {
            if (_inst != null) return;
            GameObject go = new GameObject("TimerMgr");
            go.AddComponent<TimerMgr>();
        }

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
                TimerElement element_tmp = _timerList[i];
                element_tmp._executeTime += Time.deltaTime;
                if (element_tmp._type == 0)//cd Timer
                {
                    if (element_tmp._executeTime >= element_tmp._cdTime)
                    {
                        element_tmp._loopTimes++;
                        element_tmp._totalExecuteTime += element_tmp._cdTime;//记录这个计时器一共执行了多长时间
                        element_tmp._callback(element_tmp);
                        if (element_tmp._isLoop)//loop为true的话,进入下一次循环
                        {
                            element_tmp._executeTime -= element_tmp._cdTime;
                        }
                        else
                        {
                            RemoveTimer(i);
                            i--;
                        }
                    }
                }
                else if (element_tmp._type == 1)// frame Timer
                {
                    element_tmp._loopTimes++;//记录执行了多少帧
                    element_tmp._callback(element_tmp);
                    if (element_tmp._executeTime >= element_tmp._totalExecuteTime)//执行时间已经达到
                    {
                        RemoveTimer(i);
                        i--;
                    }
                }
            }
        }

        /// <summary>
        /// 每隔多长时间执行一次
        /// </summary>
        /// <param name="key">注册key</param>
        /// <param name="cd">间隔时间</param>
        /// <param name="callBack">事件回调(第一个参数是间隔时间 第二个参数是循环了几次)</param>
        /// <param name="loop">是否循环执行</param>
        /// <param name="immRun">立即执行的情况下，得到的_totalExecuteTime会有一帧的时间差</param>
        public void RegisterCDTimer(string key, float cd, TimerEleCallBack callBack, bool loop = false, bool immRun = false)
        {
            if (callBack == null)
            {
                return;
            }
            TimerElement element_tmp = GetFreeElement();
            element_tmp._type = 0;
            element_tmp._key = key;
            element_tmp._cdTime = cd;
            element_tmp._callback = callBack;
            element_tmp._isLoop = loop;
            _timerList.Add(element_tmp);
            if (immRun)
            {
                element_tmp._loopTimes++;
                element_tmp._callback(element_tmp);
            }
        }

        /// <summary>
        /// 指定时间内每帧执行回调函数
        /// </summary>
        /// <param name="totalExecuteTime">执行总时长</param> 
        /// <param name="callBack"></param>
        /// <param name="immRun">立即执行的情况下，得到的_executeTime会有一帧的时间差</param>
        public string RegisterFrameTimer(float totalExecuteTime, TimerEleCallBack callBack, bool immRun = false)
        {
            TimerElement element_tmp = GetFreeElement();
            element_tmp._type = 1;
            element_tmp._key = getUid();
            element_tmp._executeTime = 0;
            element_tmp._totalExecuteTime = totalExecuteTime;
            element_tmp._callback = callBack;
            _timerList.Add(element_tmp);
            if (immRun)
            {
                element_tmp._loopTimes++;
                callBack(element_tmp);
            }
            return element_tmp._key;
        }

        bool HasKey(string key, out int index)
        {
            for (int i = 0; i < _timerList.Count; i++)
            {
                if (_timerList[i]._key == key)
                {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }
        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="key"></param>
        public void RemoveTimer(string key)
        {
            int index_tmp = 0;
            bool hasKey_tmp = false;
            for (int i = 0; i < _timerList.Count; i++)
            {
                if (_timerList[i]._key == key)
                {
                    index_tmp = i;
                    hasKey_tmp = true;
                    break;
                }
            }

            if (hasKey_tmp)
            {
                RemoveTimer(index_tmp);
            }
            else
            {
                Debug.Log(string.Format("没有key={0}的Timer,请检查key是否正确！！",key));
            }

        }

        private void RemoveTimer(int index_tmp)
        {
            TimerElementRelease(_timerList[index_tmp]);
            _timerList.RemoveAt(index_tmp);
        }

        /// <summary>
        /// 得到一个可复用的对象
        /// </summary>
        /// <returns></returns>
        private TimerElement GetFreeElement()
        {
            TimerElement element_tmp;
            if (_elementPool.Count > 0)
            {
                element_tmp = _elementPool.Pop();
            }
            else
            {
                element_tmp = new TimerElement();
            }
            return element_tmp;
        }

        private void TimerElementRelease(TimerElement element)
        {
            element.Clear();
            _elementPool.Push(element);
        }

        private uint _idIndex = 0;
        private string getUid()
        {
            _idIndex++;
            return _idIndex.ToString();
        }


        void OnDestroy()
        {
            for (int i = 0; i < _timerList.Count; i++)
            {
                _timerList[i]._callback = null;
            }
            _timerList.Clear();
            _elementPool.Clear();
        }

    }

    public class TimerElement
    {
        /// <summary>
        /// 0:CD Timer  1:Frame Timer
        /// </summary>
        public int _type =-1;
        public string _key;//注册key
        public float _cdTime;// 延迟时长
        public bool _isLoop;//是否循环
        public float _totalExecuteTime;//执行总时长
        public TimerEleCallBack _callback;
        public float _executeTime;// 当前时间
        public int _loopTimes;//当前已经循环次数

        public void Clear()
        {
            _type = -1;
            _cdTime = 0;
            _isLoop = false;
            _totalExecuteTime = 0;
            _executeTime = 0;
            _loopTimes = 0;
            _callback = null;
        }
    }
}
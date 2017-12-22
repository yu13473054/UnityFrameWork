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

        //��ʼ����ʱ�����
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
                        element_tmp._totalExecuteTime += element_tmp._cdTime;//��¼�����ʱ��һ��ִ���˶೤ʱ��
                        element_tmp._callback(element_tmp);
                        if (element_tmp._isLoop)//loopΪtrue�Ļ�,������һ��ѭ��
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
                    element_tmp._loopTimes++;//��¼ִ���˶���֡
                    element_tmp._callback(element_tmp);
                    if (element_tmp._executeTime >= element_tmp._totalExecuteTime)//ִ��ʱ���Ѿ��ﵽ
                    {
                        RemoveTimer(i);
                        i--;
                    }
                }
            }
        }

        /// <summary>
        /// ÿ���೤ʱ��ִ��һ��
        /// </summary>
        /// <param name="key">ע��key</param>
        /// <param name="cd">���ʱ��</param>
        /// <param name="callBack">�¼��ص�(��һ�������Ǽ��ʱ�� �ڶ���������ѭ���˼���)</param>
        /// <param name="loop">�Ƿ�ѭ��ִ��</param>
        /// <param name="immRun">����ִ�е�����£��õ���_totalExecuteTime����һ֡��ʱ���</param>
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
        /// ָ��ʱ����ÿִ֡�лص�����
        /// </summary>
        /// <param name="totalExecuteTime">ִ����ʱ��</param> 
        /// <param name="callBack"></param>
        /// <param name="immRun">����ִ�е�����£��õ���_executeTime����һ֡��ʱ���</param>
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
        /// �Ƴ��¼�
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
                Debug.Log(string.Format("û��key={0}��Timer,����key�Ƿ���ȷ����",key));
            }

        }

        private void RemoveTimer(int index_tmp)
        {
            TimerElementRelease(_timerList[index_tmp]);
            _timerList.RemoveAt(index_tmp);
        }

        /// <summary>
        /// �õ�һ���ɸ��õĶ���
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
        public string _key;//ע��key
        public float _cdTime;// �ӳ�ʱ��
        public bool _isLoop;//�Ƿ�ѭ��
        public float _totalExecuteTime;//ִ����ʱ��
        public TimerEleCallBack _callback;
        public float _executeTime;// ��ǰʱ��
        public int _loopTimes;//��ǰ�Ѿ�ѭ������

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
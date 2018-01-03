using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;

public class NetworkMgr : MonoBehaviour
{
    #region ��ʼ��
    private static NetworkMgr _inst;
    public static NetworkMgr Inst
    {
        get { return _inst; }
    }
    public static void Init()
    {
        if (_inst)
        {
            return;
        }
        GameObject go = new GameObject("NetworkMgr");
        go.AddComponent<NetworkMgr>();
    }
    #endregion

    private SocketClient socket;
    static readonly object m_lockObject = new object();
    static Queue<KeyValuePair<int, ByteBuffer>> mEvents = new Queue<KeyValuePair<int, ByteBuffer>>();

    SocketClient SocketClient
    {
        get
        {
            if (socket == null)
                socket = new SocketClient();
            return socket;
        }
    }

    void Awake()
    {
        _inst = this;
        DontDestroyOnLoad(gameObject);

        SocketClient.OnRegister();
    }

    public void OnInit()
    {
        CallMethod("Start");
    }

    public void Unload()
    {
        CallMethod("Unload");
    }

    /// <summary>
    /// ִ��Lua����
    /// </summary>
    public object[] CallMethod(string func, params object[] args)
    {
        return MyUtils.CallMethod("Network", func, args);
    }

    ///------------------------------------------------------------------------------------
    public static void AddEvent(int _event, ByteBuffer data)
    {
        lock (m_lockObject)
        {
            mEvents.Enqueue(new KeyValuePair<int, ByteBuffer>(_event, data));
        }
    }

    /// <summary>
    /// ����Command�����ﲻ����ķ���˭��
    /// </summary>
    void Update()
    {
        if (mEvents.Count > 0)
        {
            while (mEvents.Count > 0)
            {
                KeyValuePair<int, ByteBuffer> _event = mEvents.Dequeue();
                //                    facade.SendMessageCommand(NotiConst.DISPATCH_MESSAGE, _event);
            }
        }
    }

    /// <summary>
    /// ������������
    /// </summary>
    public void SendConnect()
    {
        SocketClient.SendConnect();
    }

    /// <summary>
    /// ����SOCKET��Ϣ
    /// </summary>
    public void SendMessage(ByteBuffer buffer)
    {
        SocketClient.SendMessage(buffer);
    }

    /// <summary>
    /// ��������
    /// </summary>
    void OnDestroy()
    {
        _inst = null;
        SocketClient.OnRemove();
        Debug.Log("~NetworkManager was destroy");
    }
}
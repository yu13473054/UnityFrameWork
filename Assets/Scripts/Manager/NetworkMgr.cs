using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;

// ���ֻ������Luaϵͳֻ����Lua֮������
public struct MsgInfo
{
    public int cid;
    public int msgId;
    public byte[] buffer;

    public MsgInfo(int cid, int msgId, byte[] buffer)
    {
        this.cid = cid;
        this.msgId = msgId;
        this.buffer = buffer;
    }
}

public class NetStatus
{
    public const int None = 0;
    public const int Login = 1;
    public const int OnReconnect = 2;
    public const int NeedStopReconnect = 3;
    public const int WaitReconnect = 4;
    public const int WaitToDlgLogin = 5;
    public const int NeedStopConnect = 6;
}

public class NetworkMgr : MonoBehaviour
{
    #region ��ʼ��
    private static NetworkMgr _inst;
    public static NetworkMgr Inst
    {
        get { return _inst; }
    }
    #endregion

    private SocketClient _socket;
    static readonly object _lockObject = new object();
    Queue<KeyValuePair<int, byte[]>> _events = new Queue<KeyValuePair<int, byte[]>>();

    private LuaFunction _onSocket;
    private LuaFunction _onHttp;
    private LuaFunction _reConnect;
    private List<MsgInfo> _msgList = new List<MsgInfo>();
    private Dictionary<string, string> _httpParamDic = new Dictionary<string, string>();
    [HideInInspector]
    public int netStatus = NetStatus.None;
    public int cid { get; set; } //�ͻ��˷��͵����к�
    public int sid { get; set; } //���������͵����к�


    public Dictionary<int, bool> ignoreMsgs;
    void Awake()
    {
        _inst = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        _socket = new SocketClient();
        _socket.OnRegister();

        _onSocket = LuaMgr.Inst.GetFunction("Network.Response");
        _onHttp = LuaMgr.Inst.GetFunction("Network.HttpResponse");
        _reConnect = LuaMgr.Inst.GetFunction("Network.OnReConnect");
    }

    //���ú�����Ϣ��
    public void SetIgnoreMsg(string str)
    {
        if(ignoreMsgs == null)
            ignoreMsgs = new Dictionary<int, bool>();
        else
        {
            ignoreMsgs.Clear();
        }
        if (!string.IsNullOrEmpty(str))
        {
            string[] strSplits = str.Split(',');
            for (int i = 0; i < strSplits.Length; i++)
            {
                ignoreMsgs[Int32.Parse(strSplits[i])] = true;
            }
        }
    }

    public bool IgnoreMsg(int msgID)
    {
        if (ignoreMsgs == null) return true;
        return ignoreMsgs.ContainsKey(msgID);
    }

    ///------------------------------------------------------------------------------------
    public void AddEvent( int _event, byte[] data )
    {
        lock( _lockObject )
        {
            _events.Enqueue( new KeyValuePair<int, byte[]>( _event, data ) );
        }
    }

    /// <summary>
    /// ����Command�����ﲻ����ķ���˭��
    /// </summary>
    void Update()
    {
        if( _events.Count > 0 )
        {
            while( _events.Count > 0 )
            {
                KeyValuePair<int, byte[]> _event = _events.Dequeue();

                // �ڴ˰���Ϣ�ַ���Lua
                if( _event.Value == null )
                    _onSocket.Call<int>( _event.Key );
                else
                    _onSocket.Call<int, LuaByteBuffer>( _event.Key, new LuaByteBuffer( _event.Value ) );
            }
        }
        if (netStatus == NetStatus.NeedStopReconnect) 
        {
            netStatus = NetStatus.WaitReconnect;
            StopAllCoroutines();
        }
        else if (netStatus == NetStatus.WaitToDlgLogin || netStatus == NetStatus.NeedStopConnect)
        {
            netStatus = NetStatus.None;
            StopAllCoroutines();
        }
    }

    private int _isforntBack = -999;

    void OnApplicationFocus(bool focus)
    {
        if (focus)  
        {
            if (_isforntBack != -1) return;
            _isforntBack = -999;
            if (netStatus != NetStatus.Login) return;
            if (_socket.ClientConnected()) return;
            ReConnect();
        }
        else
            _isforntBack = -1; //����Ѿ������̨��
    }
    void OnApplicationPause(bool pause)
    {
        if (!pause)  //�Ӻ�̨�л�����
        {
            if (_isforntBack != -1) return;
            _isforntBack = -999;
            if (netStatus != NetStatus.Login) return;
            if(_socket.ClientConnected()) return;
            ReConnect();
        }
        else
            _isforntBack = -1; //����Ѿ������̨��
    }
    /// <summary>
    /// ������������
    /// </summary>
    public void SendConnect( string host, string port )
    {
        _socket.ConnectServer( host, port );
        StartCoroutine(ConnectTimeOut(host, port)); //���ó�ʱ����
    }

    private IEnumerator ConnectTimeOut(string host, string port)
    {
        yield return new WaitForSeconds(10);
        _socket.ConnectServer(host, port);
        yield return ConnectTimeOut(host, port);
    }

    public void ReConnect()
    {
        if (netStatus == NetStatus.None || netStatus == NetStatus.OnReconnect || netStatus == NetStatus.WaitToDlgLogin)
            return;
        netStatus = NetStatus.OnReconnect;
        _socket.Close();//�����У�������������֮��������socket
        _reConnect.Call(); //֪ͨlua���������
        SendConnect(GameMain.Inst.loginHost, GameMain.Inst.port);
    }

    /// <summary>
    /// ����SOCKET��Ϣ
    /// </summary>
    public void Send( int msgID, LuaByteBuffer buffer)
    {
        if (!IgnoreMsg(msgID)) //�����Ե���Ϣ�����棬�����ۼ�cid
        {
            cid++;
            CacheMsg(msgID, buffer.buffer);
        }
        Debug.Log(string.Format("<NetworkMgr> ����Э��: {0}, cid = {1}", msgID, cid));
        _socket.WriteMessage( msgID, buffer.buffer, cid, sid);
    }

    //������Ϣ
    private void CacheMsg(int msgID, byte[] bytes)
    {
        _msgList.Add(new MsgInfo(cid, msgID, bytes));
        if (_msgList.Count > 50) //������Ϣ����Ϊ50
        {
            _msgList.RemoveAt(0);
        }
    }

    /// <summary>
    /// ���ͻ������Ϣ
    /// </summary>
    /// <param name="serCID">�������յ������µ�cid</param>
    public void SendCache(int serCID)
    {
        netStatus = NetStatus.Login; //��ʼ���ͻ�����Ϣ��˵��״̬�Ѿ�ת���ɵ�¼��
        Debug.Log(string.Format("<NetworkMgr> ��������¼cid = {0}", serCID));
        for (int i = 0; i < _msgList.Count; i++)
        {
            MsgInfo info = _msgList[i];
            if (info.cid > serCID)
            {
                Debug.Log(string.Format("<NetworkMgr> �ط�Э��: {0}, �ͻ���cid = {1}", info.msgId, info.cid));
                _socket.WriteMessage(info.msgId, info.buffer, info.cid);
            }
        }
    }

    //�����������ǳ��ˣ��������
    public void OnServerLoignOut()
    {
        cid = 0;
        sid = 0;
        _msgList.Clear();
    }

    //--------------------------------------------------
    //-----------------------HTTP-----------------------
    //--------------------------------------------------
    /// <summary>
    /// Http����ʱ�Ĳ�����ÿ��http��һ�����ʱ��isNew ��ҪΪtrue
    /// </summary>
    /// <param name="isNew"> ����һ��Http����ʱ����Ҫ��Ӳ���ʱ����һ��������Ҫ����true���Ա�֤�����һ��http�Ĳ����б� </param>
    public void AddHttpParam(string key, string value, bool isNew = false)
    {
        if(isNew) _httpParamDic.Clear();
        _httpParamDic.Add(key, value);
    }
    /// HTTP GET
    public void HttpGet( int msgID, string url, bool dontDecode, params object[] param )
    {
        HttpClient httpClient = new HttpClient();
        StartCoroutine( httpClient.Get(msgID, url, _httpParamDic, dontDecode, HttpResponse, param) );
    }

    public void HttpPostForm(int msgID, string url, params object[] param)
    {
        HttpClient httpClient = new HttpClient();
        StartCoroutine( httpClient.PostForm( msgID, url, _httpParamDic, HttpResponse, param) );
    }

    public void HttpPostJson( int msgID, string url, string json, params object[] param )
    {
        HttpClient httpClient = new HttpClient();
        StartCoroutine( httpClient.PostJson( msgID, url, json, HttpResponse, param ) );
    }

    /// <summary>
    /// HTTP Response
    /// </summary>
    void HttpResponse( int error, int msgID, string response, bool dontDecode, params object[] param )
    {
        _onHttp.Call( error, msgID, response, dontDecode, param );
    }

    /// <summary>
    /// ��������
    /// </summary>
    void OnDestroy()
    {
        if( _socket != null )
            _socket.OnRemove();
        _events.Clear();
        _msgList.Clear();
        Debug.Log("<NetworkMgr> OnDestroy");
    }
}
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;

// 这个只能依赖Lua系统只能在Lua之后启用
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

public class NetworkMgr : SingletonMono<NetworkMgr>
{
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
    public int cid { get; set; } //客户端发送的序列号
    public int sid { get; set; } //服务器发送的序列号


    public Dictionary<int, bool> ignoreMsgs;

    protected override void Awake()
    {
        base.Awake();
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

    //设置忽略消息号
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
    /// 交给Command，这里不想关心发给谁。
    /// </summary>
    void Update()
    {
        if( _events.Count > 0 )
        {
            while( _events.Count > 0 )
            {
                KeyValuePair<int, byte[]> _event = _events.Dequeue();

                // 在此把消息分发给Lua
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
            _isforntBack = -1; //标记已经进入后台了
    }
    void OnApplicationPause(bool pause)
    {
        if (!pause)  //从后台切换回来
        {
            if (_isforntBack != -1) return;
            _isforntBack = -999;
            if (netStatus != NetStatus.Login) return;
            if(_socket.ClientConnected()) return;
            ReConnect();
        }
        else
            _isforntBack = -1; //标记已经进入后台了
    }
    /// <summary>
    /// 发送链接请求
    /// </summary>
    public void SendConnect( string host, string port )
    {
        _socket.ConnectServer( host, port );
        StartCoroutine(ConnectTimeOut(host, port)); //设置超时连接
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
        _socket.Close();//弱网中，到断线重连了之后，主动断socket
        _reConnect.Call(); //通知lua层断线重连
        SendConnect(GameMain.Inst.loginHost, GameMain.Inst.port);
    }

    /// <summary>
    /// 发送SOCKET消息
    /// </summary>
    public void Send( int msgID, LuaByteBuffer buffer)
    {
        if (!IgnoreMsg(msgID)) //被忽略的消息不缓存，并不累加cid
        {
            cid++;
            CacheMsg(msgID, buffer.buffer);
        }
        Debug.Log(string.Format("<NetworkMgr> 发送协议: {0}, cid = {1}", msgID, cid));
        _socket.WriteMessage( msgID, buffer.buffer, cid, sid);
    }

    //缓存消息
    private void CacheMsg(int msgID, byte[] bytes)
    {
        _msgList.Add(new MsgInfo(cid, msgID, bytes));
        if (_msgList.Count > 50) //缓存消息上限为50
        {
            _msgList.RemoveAt(0);
        }
    }

    /// <summary>
    /// 发送缓存的消息
    /// </summary>
    /// <param name="serCID">服务器收到的最新的cid</param>
    public void SendCache(int serCID)
    {
        netStatus = NetStatus.Login; //开始发送缓存消息，说明状态已经转换成登录了
        Debug.Log(string.Format("<NetworkMgr> 服务器记录cid = {0}", serCID));
        for (int i = 0; i < _msgList.Count; i++)
        {
            MsgInfo info = _msgList[i];
            if (info.cid > serCID)
            {
                Debug.Log(string.Format("<NetworkMgr> 重发协议: {0}, 客户端cid = {1}", info.msgId, info.cid));
                _socket.WriteMessage(info.msgId, info.buffer, info.cid);
            }
        }
    }

    //服务器主动登出了，清除缓存
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
    /// Http请求时的参数，每个http第一次添加时，isNew 需要为true
    /// </summary>
    /// <param name="isNew"> 新起一个Http请求时，需要添加参数时，第一个参数需要传入true，以保证清空上一个http的参数列表 </param>
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
    /// 析构函数
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();
        if( _socket != null )
            _socket.OnRemove();
        _events.Clear();
        _msgList.Clear();
    }
}
using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;

public class Protocal
{
    ///BUILD TABLE
    public const int CONNECT = -1;          //连接服务器
    public const int DISCONNECT = -101;     //正常断线
    public const int EXCEPTION  = -102;     //异常掉线
}

public class SocketClient
{
    private const int MAX_READ = 8192;
    private const int HEAD_SIZE = 16;

    private TcpClient _client = null;
//    private NetworkStream _outStream = null;
    private MemoryStream _memStream;
    private BinaryReader _reader;

    private byte[] byteBuffer = new byte[MAX_READ];
    private byte[] _sendHeaderBuffer = new byte[HEAD_SIZE];


    /// <summary>
    /// 注册代理
    /// </summary>
    public void OnRegister()
    {
        _memStream = new MemoryStream();
        _reader = new BinaryReader( _memStream );
    }

    /// <summary>
    /// 移除代理
    /// </summary>
    public void OnRemove()
    {
        this.Close();
        _reader.Close();
        _memStream.Close();
    }

    /// <summary>
    /// 连接服务器
    /// </summary>
    public void ConnectServer( string host, string port )
    {
        try
        {
            Close(); //关闭上次请求的异步连接，在断线重连时，会一直请求

            IPAddress[] address = Dns.GetHostAddresses( host );
            if( address.Length == 0 )
            {
                Debugger.LogError( "host invalid" );
                return;
            }
            if( address[0].AddressFamily == AddressFamily.InterNetworkV6 )
            {
                _client = new TcpClient( AddressFamily.InterNetworkV6 );
            }
            else
            {
                _client = new TcpClient( AddressFamily.InterNetwork );
            }
            _client.SendTimeout = 1000;
            _client.ReceiveTimeout = 1000;
            _client.NoDelay = true;
            _client.BeginConnect( host, int.Parse(port), OnConnect, null );
        }
        catch( Exception e )
        {
            Close();
            Debugger.Log("<SocketClient> Socket 连接失败！\n"+e.Message);
        }
    }

    /// <summary>
    /// 连接上服务器
    /// </summary>
    void OnConnect( IAsyncResult asr )
    {
        if(_client == null) return; //在重连失败后，调用close方法，会进入回调，此时剔除即可
        try
        {
            _client.GetStream().BeginRead(byteBuffer, 0, MAX_READ, new AsyncCallback(OnRead), null);
            NetworkMgr.Inst.AddEvent(Protocal.CONNECT, null);
            if (NetworkMgr.Inst.netStatus == NetStatus.OnReconnect) //重连
                NetworkMgr.Inst.netStatus = NetStatus.NeedStopReconnect; //非主线程，不能直接关闭携程
            else if (NetworkMgr.Inst.netStatus == NetStatus.None) //登录连接
                NetworkMgr.Inst.netStatus = NetStatus.NeedStopConnect;
        }
        catch (Exception e)
        {
            Close();   //关掉客户端链接
        }
    }

    public bool ClientConnected()
    {
        return _client != null && _client.Connected;
    }

    /// <summary>
    /// 写数据
    /// </summary>
    public void WriteMessage( int msgID, byte[] message, int cid, int sid = 0)
    {
        if (ClientConnected())
        {
            MemoryStream ms = null;
            using( ms = new MemoryStream() )
            {
                ms.Position = 0;
                BinaryWriter writer = new BinaryWriter( ms );
                int msglen = (int)message.Length + HEAD_SIZE; //加上消息头
                SetBufferInt(_sendHeaderBuffer, msglen , 0);
                SetBufferInt(_sendHeaderBuffer, msgID, 4);
                SetBufferInt(_sendHeaderBuffer, cid, 8);
                SetBufferInt(_sendHeaderBuffer, sid, 12);
                writer.Write(_sendHeaderBuffer, 0, HEAD_SIZE);
                writer.Write( message );
                writer.Flush();
                byte[] payload = ms.ToArray();
                _client.GetStream().BeginWrite( payload, 0, payload.Length, new AsyncCallback( OnWrite ), null );
            }
        }
        else
        {
            NetworkMgr.Inst.ReConnect();
        }

    }

    /// <summary>
    /// 读取消息
    /// </summary>
    void OnRead( IAsyncResult asr )
    {
        if ( _client == null )
            return;

        try
        {
            int bytesRead = 0;
            lock( _client.GetStream() )
            {
                //读取字节流到缓冲区
                bytesRead = _client.GetStream().EndRead( asr );
            }
            if( bytesRead < 1 )
            {
                return;
            }

            //分析数据包内容，抛给逻辑层
            OnReceive( byteBuffer, bytesRead );

            lock( _client.GetStream() )
            {
                //分析完，再次监听服务器发过来的新消息
                Array.Clear( byteBuffer, 0, byteBuffer.Length );   //清空数组
                _client.GetStream().BeginRead( byteBuffer, 0, MAX_READ, new AsyncCallback( OnRead ), null );
            }
        }
        catch( Exception ex )
        {
        }
    }

    /// <summary>
    /// 向链接写入数据流
    /// </summary>
    void OnWrite( IAsyncResult r )
    {
        try
        {
            _client.GetStream().EndWrite( r );
        }
        catch( Exception ex )
        {
            Debugger.LogError( "OnWrite--->>>" + ex.Message );
        }
    }

    /// <summary>
    /// 接收到消息
    /// </summary>
    void OnReceive( byte[] bytes, int length )
    {
        _memStream.Seek( 0, SeekOrigin.End );
        _memStream.Write( bytes, 0, length );
        //Reset to beginning
        _memStream.Seek( 0, SeekOrigin.Begin );
        while( RemainingBytes() >= HEAD_SIZE )
        {
            int messageLen = readInt32FromNetwork(_reader) - HEAD_SIZE;//减去消息头占用的长度
            int msgID = readInt32FromNetwork(_reader);
            int cid = readInt32FromNetwork(_reader);
            int sid = readInt32FromNetwork(_reader);
            NetworkMgr.Inst.sid = sid; //记录收到的服务器的包id
            if ( RemainingBytes() >= messageLen )
            {
                MemoryStream ms = new MemoryStream();
                BinaryWriter writer = new BinaryWriter( ms );
                writer.Write( _reader.ReadBytes( messageLen ) );
                ms.Seek( 0, SeekOrigin.Begin );
                OnReceivedMessage( msgID, ms );
            }
            else
            {
                //Back up the position HEAD_SIZE
                _memStream.Position = _memStream.Position - HEAD_SIZE;
                break;
            }
        }
        //Create a new stream with any leftover bytes
        byte[] leftover = _reader.ReadBytes( (int)RemainingBytes() );
        _memStream.SetLength( 0 );     //Clear
        _memStream.Write( leftover, 0, leftover.Length );
    }

    /// <summary>
    /// 剩余的字节
    /// </summary>
    private long RemainingBytes()
    {
        return _memStream.Length - _memStream.Position;
    }

    /// <summary>
    /// 接收到消息
    /// </summary>
    /// <param name="ms"></param>
    void OnReceivedMessage( int msgID, MemoryStream ms )
    {
        BinaryReader r = new BinaryReader( ms );
        byte[] message = r.ReadBytes( (int)( ms.Length - ms.Position ) );
        NetworkMgr.Inst.AddEvent( msgID, message );
    }

    /// <summary>
    /// 关闭链接
    /// </summary>
    public void Close()
    {
        if( _client != null )
        {
            _client.Close();
            _client = null;
        }
    }

#region 配合服务器修改了数据做的转换
    public static void SetBufferInt(byte[] buf, int value, int offset)
    {
        int pos = offset;
        for (int i = 0; i < 4; i++)
        {
            buf[pos++] = (byte)((value >> (24 - 8 * i)) & 0xff);
        }
    }
    private int getBufferInt(byte[] buf, int offset = 0)
    {
        int result = 0;
        for (int i = 0; i < 4; i++)
        {
            int value = buf[i + offset];
            result += value << (24 - 8 * i);
        }
        return result;
    }
    private int readInt32FromNetwork(BinaryReader br)
    {
        byte[] buff = br.ReadBytes(4);
        return getBufferInt(buff);
    }
#endregion
}

Network = DefineConst("Network", {});

Network.RECONNECT = {};
Network.RECONNECT.NORMAL                = 1;            --正常流程
Network.RECONNECT.TOLGOIN               = 2;            --直接登录

Network.NETSTATUS_NONE                  = 0;            
Network.NETSTATUS_LOGIN                 = 1;            --登录成功
Network.NETSTATUS_ONRECONNECT           = 2;            --重连中   
Network.NETSTATUS_NEEDSTOPRECONNECT     = 3;            --重连成功，通知关闭协程
Network.NETSTATUS_WAITRECONNECT         = 4;            --等待重连
Network.NETSTATUS_WAITTODLGLOGIN        = 5;            --等待跳转到登录界面
Network.NETSTATUS_NEEDSTOPCONNECT       = 6;            --登录连接成功。通知关闭协程

Network.reconnectNum = 0;
Network.reconnectStatus = Network.RECONNECT.NORMAL;

local _errorCode = -1;

local json = require "cjson";

-- 连接游戏服务器
function Network.Connect( host, port )
    Log( "<Network> 开始连接服务器...");
    NetworkManager.instance:SendConnect( host, port );
end

-- Socket消息
function Network.Response( msgID, msg )
    -- 特殊消息处理
    if msgID == EVENT_NET_ONCONNECT then
        -- 连接成功
        Log( "<Network> 连接服务器成功!" );
        EventDispatcher.DispatchEvent(EVENT_NET_ONCONNECT);
        return;
    elseif msgID == EVENT_NET_EXCEPTION then
        -- 连接正常断开
        Log( "<Network> 与服务器连接断开!" );
        EventDispatcher.DispatchEvent(EVENT_NET_ERROR, msgID);
        return;
    end

    -- 分发消息分发出去
    if msgID ~= 1801 then  --服务器时间包不输出日志
        Log( "<Network> 接收协议: " .. msgID );
    end
    EventDispatcher.DispatchEvent( msgID, msg );
end

-- 序列化消息，并发送
function Network.Send( msgID, msg, notCheck )
    if _errorCode == Network.ErrorCode.SERVERDISCONNECT then
        DlgWait.Close();
        Network.OnReconnectOver();
        return;
    end
    NetworkManager.instance:Send( msgID, msg:SerializeToString() );-- 发包
end

-- Http发消息, 如果needResponse将会接受返回事件
function Network.HttpGet(msgID, url, dontDecode)
    NetworkManager.instance:HttpGet( msgID, url, dontDecode );
end

-- HttpPost
function Network.HttpPostForm(msgID, url, ...)
    NetworkManager.instance:HttpPostForm( msgID, url, ...);
end

-- HttpPost
function Network.HttpPostJson(msgID, url, json, ...)
    NetworkManager.instance:HttpPostJson( msgID, url, json, ...);
end

-- Http回消息
function Network.HttpResponse( err, msgID, data, dontDecode, ... )
    -- 判错
    if err > 0 then
        LogErr( "<Network> 接收HTTP出错！ " .. data );
        return;
    end
    Log( "<Network> 接收HTTP协议: " .. msgID );

    -- 不解码，直接发
    if dontDecode then
        EventDispatcher.DispatchEvent( msgID, data, ... );
        return;
    end

    -- json拆包
    local ret, msg = pcall(json.decode, data);
    -- 无法解析把返回值抛出
    if not ret then
        LogErr( "<Network> 无效的返回值！msgID = " ..  msgID .. "：" .. data );
        return;
    end

    -- 然后把解析好的表分发出去
    EventDispatcher.DispatchEvent( msgID, msg, ... );
end

--重连
local function Network_OnCSReconnect()
    local msg = account_pb.CSReconnect();
    SDKHelper.DSDK(msg.sdk);
    msg.humanId  = PlayerCache.GetHumanId();
    msg.sessionKey  = SDKHelper.GetUniqueKey();
    Network.Send( MsgId.CSReconnect, msg);
end
local function Network_OnSCReconnect(msg)
    EventDispatcher.RemoveListener( MsgId.SCReconnect, Network_OnSCReconnect );
    msg = account_pb.SCReconnect():Unpack(msg);
    NetworkManager.instance.netStatus = Network.NETSTATUS_LOGIN; --转换成登录了
    if NetworkManager.instance.cid == msg.cMsgReq then
        DlgWait.Close();
    else
        NetworkManager.instance:SendCache(msg.cMsgReq); --发送上次失败的包
    end
end

-- 服务器连接成功
local function Network_OnConnect()
    -- 取消消息注册
    EventDispatcher.RemoveListener(EVENT_NET_ONCONNECT, Network_OnConnect );
    Network.reconnectNum = 0;
    EventDispatcher.AddListener( MsgId.SCReconnect, Network_OnSCReconnect );
    Network_OnCSReconnect();
end
---开始重连
function Network.OnReConnect()
    Log( "<Network> 开始重新连接服务器..." );
    DlgWait.Open();
    Network.reconnectNum = Network.reconnectNum + 1;
    --注册连接回调
    EventDispatcher.AddListener(EVENT_NET_ONCONNECT, Network_OnConnect );
end

function Network.StartReConnect()
    NetworkManager.instance:ReConnect();
end

--单次重连结束
function Network.OnTimeOver()
    NetworkManager.instance.netStatus = Network.NETSTATUS_NEEDSTOPRECONNECT;
end

--多次重连结束
function Network.OnReconnectOver(textSn)
    textSn = textSn or "ui_150102";
    DlgConfirmBox.Show(ReBoot, nil, Localization.Get(textSn));
    --停止重连
    Network.reconnectNum = 0;
    NetworkManager.instance.netStatus = Network.NETSTATUS_WAITTODLGLOGIN;  --显示需要重新登录的弹窗
end
-------------------------------
---------网络异常处理-----------
-------------------------------
----服务器错误码
--function Network.OnSCErrCode(msg)
--    msg = account_pb.SCErrCode():Unpack(msg);
--    Log( "<Network> 服务器错误码：" .. msg.errCode);
--    _errorCode = msg.errCode;
--    if msg.errCode == Network.ErrorCode.OTHERLOGIN then --被踢掉了
--        if NetworkManager.instance.netStatus == Network.NETSTATUS_LOGIN then --已经登录了
--            DlgWait.Close();
--            Network.OnReconnectOver("ui_150103")
--        end
--    elseif msg.errCode == Network.ErrorCode.SERVERLOGINOUT then
--        --发送重连包121后，服务器会根据情况，返回121或者错误码，二者回其一。
--        EventDispatcher.RemoveListener( MsgId.SCReconnect, Network_OnSCReconnect );
--        if Network.reconnectStatus ~= Network.RECONNECT.NORMAL then --服务器登出了，且在特殊玩法中，就需要直接跳转到登录界面
--            DlgWait.Close();
--            Network.OnReconnectOver();
--        else
--            Log( "<Network> 开始重新登录！" );
--            NetworkManager.instance:OnServerLoignOut(); --清除服务器缓存
--            EventDispatcher.AddListener( MsgId.SCInitData, Network_OnReInitData );
--            Network_OnCSFreeLogin();
--        end
--    end
--end
--EventDispatcher.AddListener(MsgId.SCErrCode, Network.OnSCErrCode);


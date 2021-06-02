-- 界面
DlgWait = DefineConst("DlgWait", {});

-- 在OnDestroy时会被清除。需要将Unity队像、回调函数存在该结构中
local _tmpFields = {dlg = nil};
-- 永久存在的数据，一般为值类型的数据
local _finalFields = {};

-- 打开界面
function DlgWait.Open()
	UIMgr.Inst:Open( "DlgWait" );
end

-- 隐藏界面
function DlgWait.Close()
	if _tmpFields.dlg == nil then
		return;
	end
	
	UIMgr.Inst:Close(_tmpFields.dlg);
end

-- 编辑器下检查用接口
function DlgWait.FinalFields()
	return _finalFields;
end

-- 所属按钮点击时调用
function DlgWait.OnEvent( uiEvent, controlID, value, gameObject )
	if uiEvent == UIEVENT_UIBUTTON_CLICK then
        if controlID == -1 then
            DlgWait.Close();
        end
	end
end

-- 同Awake
function DlgWait.OnAwake( gameObject )
	_tmpFields.dlg = gameObject:GetComponent( typeof( UISystem ) );
	local relatedObjs = gameObject:GetComponent( typeof( UIItem ) ).relatedGameObject;
	_tmpFields.waitUI = relatedObjs[0]:GetComponent(typeof(UIImage));

	_tmpFields.delayTime = 2;
    _tmpFields.overTime = 10;
    _tmpFields.connectLimit = 3;
end

-- 同OnEnable：需要时添加，否则注释掉，提升性能
--function DlgWait.OnEnable( gameObject )
--end

-- 同Start：需要时添加，否则注释掉，提升性能
--function DlgWait.OnStart( gameObject )
--end

-- 同OnDisable：需要时添加，否则注释掉，提升性能
--function DlgWait.OnDisable( gameObject )
--end

-- 第一次在Start后执行，之后在OnEnable后
function DlgWait.OnOpen( gameObject )
    TimerManager.DespawnModule("DlgWait");
	--超时显示菊花
	_tmpFields.delayTimer = TimerManager.SpawnTimer("DlgWait", DlgWait.OnShowHint, _tmpFields.delayTime);
	_tmpFields.overTimer = TimerManager.SpawnTimer("DlgWait", DlgWait.ShowRecconect, _tmpFields.overTime);
	_tmpFields.waitUI.gameObject:SetActive(false);
end

-- 界面调用Close后，执行。如果没有通过UIMgr，不会被调用
function DlgWait.OnClose( gameObject )
    TimerManager.DespawnModule("DlgWait");
	_tmpFields.delayTimer = nil;
	_tmpFields.overTimer = nil;
end

-- 同OnDestroy
function DlgWait.OnDestroy( gameObject )
	_tmpFields = {};
end

function DlgWait.OnShowHint( )
	_tmpFields.waitUI.gameObject:SetActive(true);
end
--显示断线 弹窗
function DlgWait.ShowRecconect()
    if NetworkManager.instance.netStatus == Network.NETSTATUS_NONE then --登录界面超时
        NetworkManager.instance.netStatus = Network.NETSTATUS_WAITTODLGLOGIN;
        EventDispatcher.DispatchEvent(EVENT_NET_ERROR, EVENT_NET_EXCEPTION);
    else
        --编辑器上需要在重连状态上才显示
        if (RUNPLATFORM == 0 or RUNPLATFORM == 7) and NetworkManager.instance.netStatus ~= Network.NETSTATUS_ONRECONNECT then
            return;
        end
        DlgWait.Close();
        if Network.reconnectNum > _tmpFields.connectLimit then --重连次数达到上限
            Network.OnReconnectOver();
        else
            Network.OnTimeOver();
            DlgConfirmBox.Show( Network.StartReConnect, nil, Localization.Get( "ui_100026" ), Localization.Get( "ui_100025" ) );
        end
    end
end

function DlgWait.ResetTimer()
    TimerManager.DespawnTimer("DlgWait", _tmpFields.overTimer);
	_tmpFields.overTimer = TimerManager.SpawnTimer("DlgWait", DlgWait.ShowRecconect, _tmpFields.overTime);
end

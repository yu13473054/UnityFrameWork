-- 界面
DlgLogin = DefineConst("DlgLogin", {});

-- 在OnDestroy时会被清除。需要将Unity队像、回调函数存在该结构中
local _tmpFields = {dlg = nil};
-- 永久存在的数据，一般为值类型的数据
local _finalFields = {};

-- 打开界面
function DlgLogin.Open()
	UIMgr.Inst:Open( "DlgLogin" );
end

-- 隐藏界面
function DlgLogin.Close()
	if _tmpFields.dlg == nil then
		return;
	end
	
	UIMgr.Inst:Close(_tmpFields.dlg);
end

-- 编辑器下检查用接口
function DlgLogin.FinalFields()
	return _finalFields;
end

-- 所属按钮点击时调用
function DlgLogin.OnEvent( uiEvent, controlID, value, gameObject )
	if uiEvent == UIEVENT_UIBUTTON_CLICK then
		if controlID == -1 then
			DlgLogin.Close();
		elseif controlID == 1 then --开始游戏
			-- 显示遮罩
			--DlgWait.Open();
			DlgMain.Open();
		end
	end
end

-- 同Awake
function DlgLogin.OnAwake( gameObject )
	_tmpFields.dlg = gameObject:GetComponent( typeof( UISystem ) );
	local relatedObjs = gameObject:GetComponent( typeof( UIItem ) ).relatedGameObject;
	_tmpFields.text = relatedObjs[0]:GetComponent(typeof(UIText));

end

-- 需要入栈的界面，如果在关闭时，需要备份数据，打开，否则注释掉，提升性能
function DlgLogin.StackRecordUI(stackIndex)
	if not _finalFields.uiStackData then
		_finalFields.uiStackData = {};
	end
	_finalFields.uiStackData[stackIndex] = 1111;
end

-- 栈中界面还原用，需要的情况打开，在该方法中进行界面的还原
function DlgLogin.StackRevertUI(stackIndex, needOpen)
	print("DlgLogin", stackIndex)
	if needOpen then
		--需要重新走Open的逻辑，之前缓存的数据，直接用来在open后更新UI
		DlgLogin.Open();
	else
		-- 直接更新界面即可
	end
end

-- 第一次在Start后执行，之后在OnEnable后
function DlgLogin.OnOpen( gameObject )

end

-- 界面调用Close后，执行。如果没有通过UIMgr，不会被调用
function DlgLogin.OnClose( gameObject )
	
end

-- 同OnDestroy
function DlgLogin.OnDestroy( gameObject )
	_tmpFields = {};
end

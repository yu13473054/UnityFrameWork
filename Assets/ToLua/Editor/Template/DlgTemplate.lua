-- 界面
#NAME# = DefineConst("#NAME#", {});

-- 在OnDestroy时会被清除。需要将Unity队像、回调函数存在该结构中
local _tmpFields = {dlg = nil};
-- 永久存在的数据，一般为值类型的数据
local _finalFields = {};

-- 打开界面
function #NAME#.Open()
	UIMgr.Inst:Open( "#NAME#" );
end

-- 隐藏界面
function #NAME#.Close()
	if _tmpFields.dlg == nil then
		return;
	end
	
	UIMgr.Inst:Close(_tmpFields.dlg);
end

-- 编辑器下检查用接口
function #NAME#.FinalFields()
	return _finalFields;
end

-- 所属按钮点击时调用
function #NAME#.OnEvent( uiEvent, controlID, value, gameObject )
	if uiEvent == UIEVENT_UIBUTTON_CLICK then
        if controlID == -1 then
            #NAME#.Close();
        end
	end
end

-- 同Awake
function #NAME#.OnAwake( gameObject )
	_tmpFields.dlg = gameObject:GetComponent( typeof( UISystem ) );
	local relatedObjs = gameObject:GetComponent( typeof( UIItem ) ).relatedGameObject;

end

-- 第一次在Start后执行，之后在OnEnable后
function #NAME#.OnOpen( gameObject )

end

-- 界面调用Close后，执行。如果没有通过UIMgr，不会被调用
function #NAME#.OnClose( gameObject )
	
end

-- 需要入栈的界面，关闭时，备份数据
function #NAME#.StackRecordUI(stackIndex)
--	if not _finalFields.uiStackData then
--		_finalFields.uiStackData = {};
--	end
--	_finalFields.uiStackData[stackIndex] = 1;
end

-- 栈中界面还原用
function #NAME#.StackRevertUI(stackIndex, needOpen)
	if needOpen then
		--需要重新走Open的逻辑，之前缓存的数据，直接用来在open后更新UI
		#NAME#.Open();
	else 
		-- 直接更新界面即可

	end
end

-- 同OnDestroy
function #NAME#.OnDestroy( gameObject )
	_tmpFields = {};
end

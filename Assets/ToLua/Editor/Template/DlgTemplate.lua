-- 界面
#NAME# = {};
local _dlg = nil;

-- 所有对Unity控件的引用都存这个table里
local _cmp = {};

-- 打开界面
function #NAME#.Open()
	_dlg = UIMgr.Inst:Open( "#NAME#" );
end

-- 隐藏界面
function #NAME#.Close()
	if _dlg == nil then
		return;
	end
	
	UIMgr.Inst:Close(_dlg);
end

----------------------------------------
-- 事件
----------------------------------------
-- 所属按钮点击时调用
function #NAME#.OnEvent( uiEvent, controlID, value, gameObject )
	if uiEvent == UIEVENT_UIBUTTON_CLICK then
        if controlID == -1 then
            #NAME#.Close();
        end
	end
end

-- 载入时调用
function #NAME#.OnInit( gameObject )
	-- 控件赋值	
	local objs = gameObject:GetComponent( typeof( UISystem ) ).relatedGameObject;
end

-- 界面显示时调用
function #NAME#.OnOpen( gameObject )
	
end

-- 界面隐藏时调用
function #NAME#.OnClose( gameObject )
	
end

-- 界面删除时调用
function #NAME#.OnDestroy( gameObject )
	_dlg = nil;
    _cmp = {};
end


----------------------------------------
-- 自定
----------------------------------------

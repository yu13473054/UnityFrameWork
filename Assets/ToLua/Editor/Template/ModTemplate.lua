-- 模块
#NAME# = {};

-- 所有对Unity控件的引用都存这个table里
local _cmp = {};

----------------------------------------
-- 事件
----------------------------------------

-- 所属按钮点击时调用
function #NAME#.OnEvent( uiEvent, controlID, value, gameObject )
	if uiEvent == UIEVENT_UIBUTTON_CLICK then
		print( "Button Clicked, nControlID:" .. controlID );
	elseif uiEvent == UIEVENT_UIBUTTON_PRESS then
		if value then
			print( "Button Pressed Down, nControlID:" .. controlID );
		elseif not value then
			print( "Button Pressed UP, nControlID:" .. controlID );
		end
	end
end

-- 载入时调用
function #NAME#.OnInit( gameObject )
	local mod = gameObject:GetComponent( typeof( UIMod ) );
end

-- 界面显示时调用
function #NAME#.OnOpen( gameObject )
	
end

-- 界面隐藏时调用
function #NAME#.OnClose( gameObject )
	
end

-- 界面删除时调用
function #NAME#.OnDestroy( gameObject )
    _cmp = {};
end

----------------------------------------
-- 自定
----------------------------------------

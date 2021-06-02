-- 模块
#NAME# = DefineConst("#NAME#", {});

-- 在OnDestroy时会被清除。需要将Unity队像、回调函数存在该结构中
local _tmpFields = {};
-- 永久存在的数据，一般为值类型的数据
local _finalFields = {};

-- 编辑器下检查用接口
function #NAME#.FinalFields()
	return _finalFields;
end

-- 所属按钮点击时调用
function #NAME#.OnEvent( uiEvent, controlID, value, gameObject )
	if uiEvent == UIEVENT_UIBUTTON_CLICK then
	end
end

-- 同Awake
function #NAME#.OnAwake( gameObject )
	local mod = gameObject:GetComponent( typeof( UIMod ) );
	local relatedObjs = gameObject:GetComponent( typeof( UIItem ) ).relatedGameObject;

	local key = gameObject:GetHashCode();
	if not _tmpFields[key] then _tmpFields[key] = {}; end

end

-- 第一次在Start后执行，之后在OnEnable后
function #NAME#.OnOpen( gameObject )

end

-- 界面调用Close后，执行。如果没有通过UIMgr，不会被调用
function #NAME#.OnClose( gameObject )
	
end

-- 同OnDestroy
function #NAME#.OnDestroy( gameObject )
	local key = gameObject:GetHashCode();
	_tmpFields[key] = nil;
end


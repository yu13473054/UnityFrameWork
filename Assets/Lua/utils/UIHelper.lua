-- 模块
UIHelper = DefineConst("UIHelper", {});

function UIHelper.OnOpen(uiName, gameObject )
	local dlg = _G[uiName];
	if dlg then
		dlg.OnOpen( gameObject );
	else
		LogErr( "<UIHelper> 不存在的UIName:" .. uiName );
		return;
	end
	EventDispatcher.DispatchEvent( Event_UI_ONOPEN, uiName, gameObject );
end

function UIHelper.OnClose(uiName, gameObject )
	local dlg = _G[uiName];
	if dlg then
		dlg.OnClose( gameObject );
	else
		LogErr( "<UIHelper> 不存在的UIName:" .. uiName );
		return;
	end
	EventDispatcher.DispatchEvent( Event_UI_ONOPEN, uiName, gameObject );
end

function UIHelper.OnDestroy(uiName, gameObject )
	local dlg = _G[uiName];
	if dlg then
		dlg.OnDestroy( gameObject );
	else
		LogErr( "<UIHelper> 不存在的UIName:" .. uiName );
		return;
	end
	EventDispatcher.DispatchEvent( Event_UI_ONDESTROY, uiName, gameObject );
end

-------编辑器下进行Lua检查-------
if RUNPLATFORM == 0 or RUNPLATFORM == 7 then
	local _checkDic = {};

	local function UIHelper_OnOpen(uiName, gameObject)
		_checkDic[uiName] = true;
	end
	EventDispatcher.AddListener(Event_UI_ONOPEN, UIHelper_OnOpen);

	local function UIHelper_CheckType(v, uiName)
		local type = type(v)
		if type == "table" then
			-- protol中特殊标志
			if not v.isProtoStruct then
				UIHelper.CheckTable(v, uiName);
			end
		elseif type == "function" or type == "userdata" then
			LogErr( "<UIHelper> 界面" .. uiName .. "中" .. tostring(v) .. "不合法！" );
		end
	end

	function UIHelper.CheckTable(t, uiName)
		for key, value in pairs(t) do
			UIHelper_CheckType(key, uiName);
			UIHelper_CheckType(value, uiName);
		end
	end

	local function UIHelper_OnDestroy(uiName, gameObject)
		if not _checkDic[uiName] then return; end
		_checkDic[uiName] = nil;

		local final = _G[uiName].FinalFields;
		if not final then
			LogErr( "<UIHelper> 界面" .. uiName .. "中没有找到FinalFields，请按照模板建立UI脚本！" );
			return;
		end
		UIHelper.CheckTable(final(), uiName);
	end
	EventDispatcher.AddListener(Event_UI_ONDESTROY, UIHelper_OnDestroy);
end
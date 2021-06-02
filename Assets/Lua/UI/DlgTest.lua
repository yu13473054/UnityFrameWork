-- 界面
DlgTest = DefineConst("DlgTest", {});

-- 在OnDestroy时会被清除。需要将Unity队像、回调函数存在该结构中
local _tmpFields = {dlg = nil};
-- 永久存在的数据，一般为值类型的数据
local _finalFields = {};

-- 打开界面
function DlgTest.Open()
	UIMgr.Inst:Open( "DlgTest" );
end

-- 隐藏界面
function DlgTest.Close()
	if _tmpFields.dlg == nil then
		return;
	end
	
	UIMgr.Inst:Close(_tmpFields.dlg);
end

-- 编辑器下检查用接口
function DlgTest.FinalFields()
	return _finalFields;
end

-- 所属按钮点击时调用
function DlgTest.OnEvent( uiEvent, controlID, value, gameObject )
	if uiEvent == UIEVENT_UIBUTTON_CLICK then
        if controlID == -1 then
            DlgTest.Close();
		elseif controlID == 1 then
			DlgLogin.Open();
		elseif controlID == 2 then
			UIMgr.Inst:StackBackup("DlgMain");
			UIMgr.Inst:UnloadAllUI();
			UIMgr.Inst:RevertBackup();
			TimerManager.SpawnTimer("aaa", function()
				UIMgr.Inst:RevertTopUI();
			end, 1);
		end
	end
end

-- 同Awake
function DlgTest.OnAwake( gameObject )
	_tmpFields.dlg = gameObject:GetComponent( typeof( UISystem ) );
	local relatedObjs = gameObject:GetComponent( typeof( UIItem ) ).relatedGameObject;
end

-- 需要入栈的界面，如果在关闭时，需要备份数据，打开，否则注释掉，提升性能
function DlgTest.StackRecordUI(stackIndex)
	--if not _finalFields.uiStackData then
	--	_finalFields.uiStackData = {};
	--end
	--_finalFields.uiStackData[stackIndex] = 1;
end

 --栈中界面还原用，需要的情况打开，在该方法中进行界面的还原
function DlgTest.StackRevertUI(stackIndex, needOpen)
	print("DlgTest", stackIndex);
	if needOpen then
		--需要重新走Open的逻辑，之前缓存的数据，直接用来在open后更新UI
		DlgTest.Open();
	else
		-- 直接更新界面即可

	end
end

-- 第一次在Start后执行，之后在OnEnable后
function DlgTest.OnOpen( gameObject )

end

-- 界面调用Close后，执行。如果没有通过UIMgr，不会被调用
function DlgTest.OnClose( gameObject )
	
end

-- 同OnDestroy
function DlgTest.OnDestroy( gameObject )
	_tmpFields = {};
end

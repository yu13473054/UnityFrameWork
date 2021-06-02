require "logic/GlobalDefine"

RUNPLATFORM = GameMain.Inst.RunPlatform;
-- Lua启动入口
function LuaStart()
    -- 设随机种子
    math.randomseed( os.time() );
	--启动登陆UI
	DlgLogin.Open();
end

require "logic/Include"

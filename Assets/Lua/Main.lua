-- 主入口函数。从这里开始lua逻辑

local go

function Main()
    local tmp = LuaFramework.ResMgr.Inst:GetPrefab("Cube")
    go = UnityEngine.GameObject.Instantiate(tmp)
    go.name = "aa"
    go.transform.position = Vector3.one

    UpdateBeat:Add(Update, self)


end

function Update()
    --    Debug.Log("ddjlsdjslljllsdljfls")
    local Input = UnityEngine.Input
    local x = Input.GetAxis("Horizontal")
    local y = Input.GetAxis("Vertical")
    go.transform.position = Vector3.New(x, y, 0)

end

-- 场景切换通知
function OnLevelWasLoaded(level)
    collectgarbage("collect")
    Time.timeSinceLevelLoad = 0
end

function OnApplicationQuit()
end
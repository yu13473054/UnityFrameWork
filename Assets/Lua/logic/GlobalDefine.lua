-- 库
require "bit"
-- 工具
require "utils/Utils"
require "utils/ObjectPool"
-- 基本逻辑
require "tolua/events"
require "logic/Define"
require "logic.EventDefine"

---限制全局变量
function ResstrictConst()
    local mt = {
        __newindex = function(t, k, v)
            print("<限制> 不能直接创建全局对象：".. k, debug.traceback());
            rawset(t, k, v);
        end
    };
    setmetatable(_G, mt);
end

--- 定义全局变量
---@param k string 全局变量的名称
function DefineConst(k, v)
    rawset(_G, k, v);
    return v;
end
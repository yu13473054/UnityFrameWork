--[[ 这里面放 不太可能会被修改的&&和逻辑无关的 工具方法 ]]

-- 克隆一个表，*注意*会递归拷贝子表
function Clone(object)
    local lookup_table = {}
    local function _copy(object)
        if type(object) ~= "table" then
            return object
        elseif lookup_table[object] then
            return lookup_table[object]
        end
        local new_table = {}
        lookup_table[object] = new_table
        for key, value in pairs(object) do
            new_table[_copy(key)] = _copy(value)
        end
        return setmetatable(new_table, getmetatable(object))
    end
    return _copy(object)
end

-- 浅拷贝
function table.copy( tab )
    local newTable = {};
    for i, v in pairs( tab ) do
        newTable[i] = v;
    end
    return newTable;
end
-- 类
function Class(classname, super)
    local superType = type(super)
    local cls

    if superType ~= "function" and superType ~= "table" then
        superType = nil
        super = nil
    end

    if superType == "function" or (super and super.__ctype == 1) then
        -- inherited from native C++ Object
        cls = {}

        if superType == "table" then
            -- copy fields from super
            for k,v in pairs(super) do cls[k] = v end
            cls.__create = super.__create
            cls.super    = super
        else
            cls.__create = super
            cls.Ctor = function() end
        end

        cls.__cname = classname
        cls.__ctype = 1

        function cls.New(...)
            local instance = cls.__create(...)
            -- copy fields from class to native object
            for k,v in pairs(cls) do instance[k] = v end
            instance.class = cls
            instance:Ctor(...)
            return instance
        end

    else
        cls = {}
        if super then
            super.__index = super;
        else
            cls.Ctor = function() end
        end
        cls.__cname = classname
        cls.__ctype = 2 -- lua

        function cls.New(...)
            -- inherited from Lua Object
            local instance = {}
            if super then
                instance.super = setmetatable({}, super)
                instance.super.__index = instance.super;
                local cloneC = Clone(cls);
                cloneC.__index = setmetatable(cloneC, instance.super);
                setmetatable(instance, cloneC);
            else
                setmetatable(instance, {__index = cls})
            end
            instance:Ctor(...)
            return instance
        end
    end

    return cls
end

-- Log，打印调试print就行
function Log( msg )
    Debugger.Log( msg );
end
function LogWarning( msg )
    Debugger.LogWarning( msg );
end
function LogErr( msg )
    Debugger.LogError( msg );
end
function error( msg )
    Debugger.LogError( msg );
end

-- 判空，仅用于Unity.Object
function IsNilOrNull( obj )
    if obj == nil or obj:Equals( nil ) then
        return true;
    end
    return false;
end

function PCallLogger( e )
    LogErr( tostring( e ) .. "\n" .. debug.traceback() );
end

function PCall( func, ... )
    -- 安全调用
    if RUNPLATFORM == 0 or RUNPLATFORM == 1 or RUNPLATFORM == 8 then
        --IOS和mac平台
        pcall( func, ... );
    else
        xpcall( func, PCallLogger, ... );
    end
end

-- 应用焦点
function LuaOnFocus( hasFocus )
    EventDispatcher.DispatchEvent( EVENT_APPLICATION_ONFOCUS, hasFocus );
end
function LuaOnQuit()
    EventDispatcher.DispatchEvent( EVENT_APPLICATION_ONQUIT );
end
function LuaOnPause(pause)
    EventDispatcher.DispatchEvent( EVENT_APPLICATION_ONPAUSE, pause );
end
function LuaOnBackClick() --返回按钮回调
    local go = UIMgr.Inst:PopBackDlg();
    if go == nil then return; end
    local dlg = rawget(_G, go.name);
    dlg.OnEvent( UIEVENT_UIBUTTON_CLICK, -1, 0, go );
end
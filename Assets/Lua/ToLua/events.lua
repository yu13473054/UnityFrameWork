-- 事件发布器
local EventClass = Class("EventClass");

function EventClass:Ctor()
    self.lock = false
    self.opList = {};
    self.list = {};
end

local function EventClass_Add(self, func)
    table.insert(self.list, 1, func);
end
function EventClass:Add(func)
    if self.lock then
        table.insert(self.opList, function() EventClass_Add(self, func) end);
    else
        EventClass_Add(self, func);
    end
end

local function EventClass_Remove(self, func)
    for i, value in ipairs(self.list) do
        if value == func then
            table.remove(self.list, i);
            break;
        end
    end
end
function EventClass:Remove(func, nextFrame)
    if self.lock and nextFrame then --下一帧移除
        table.insert(self.opList, function() EventClass_Remove(self, func) end);
    else
        EventClass_Remove(self, func)
    end
end

function EventClass:Dispatch(...)
    self.lock = true
    for i = #self.list, 1, -1 do
        local func = self.list[i];
        if func then
            PCall(func, ...);
        end
    end

    self.lock = false

    for i, op in ipairs(self.opList) do
        op()
        self.opList[i] = nil
    end
end

EventDispatcher = {};

local listeners = {};

-- 添加事件监听
function EventDispatcher.AddListener(eventName, func)
    if not eventName then
        print("<EventDispatcher> 传入的Key值为nil，请检查！", debug.traceback())
    end
    eventName = tostring( eventName );

    if listeners[eventName] == nil then
        listeners[eventName] = EventClass.New();
    end
    listeners[eventName]:Add(func);
end

-- 分发事件
function EventDispatcher.DispatchEvent( eventName, ... )
    if not eventName then
        print("<EventDispatcher> 传入的Key值为nil，请检查！", debug.traceback())
    end
    eventName = tostring( eventName );

    if listeners[eventName] == nil then
        return;
    end

    listeners[eventName]:Dispatch(...);
end

-- 移除监听
function EventDispatcher.RemoveListenerAll( eventName )
    if not eventName then
        print("<EventDispatcher> 传入的Key值为nil，请检查！", debug.traceback())
    end
    eventName = tostring( eventName );
    listeners[eventName] = nil;
end

function EventDispatcher.RemoveListener( eventName, func, nextFrame )
    if not eventName then
        print("<EventDispatcher> 传入的Key值为nil，请检查！", debug.traceback())
    end
    eventName = tostring( eventName );
    if listeners[eventName] == nil then
        return;
    end
    listeners[eventName]:Remove(func, nextFrame)
end



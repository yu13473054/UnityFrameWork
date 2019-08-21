-- 事件发布器
EventDispatcher = Class( "EventDispatcher" );

function EventDispatcher:Ctor()    
    self._listeners = {};
end

-- 添加事件监听
function EventDispatcher:AddListener( eventName, listener )
    eventName = tostring( eventName );

    if self._listeners[eventName] == nil then
        self._listeners[eventName] = {};
    end
    for i, existListener in pairs( self._listeners[eventName] ) do
        if listener == existListener then
            return;
        end
    end
    table.insert( self._listeners[eventName], listener );
end

-- 分发事件
function EventDispatcher:DispatchEvent( eventName, ... )
	eventName = tostring( eventName );

    if self._listeners[eventName] == nil then 
    	return;
    end

    for i, listener in pairs( self._listeners[eventName] ) do
        -- 安全调用
        if RUNPLATFORM == 0 or RUNPLATFORM == 1 or RUNPLATFORM == 8 then
            --IOS和mac平台
            pcall( listener, ... );
        else
            xpcall( listener, PCallLogger, ... );
        end
    end
end

-- 移除监听
function EventDispatcher:RemoveListenerAll( eventName )
	eventName = tostring( eventName );
    self._listeners[tostring( eventName )] = {};
end

-- 移除监听 *调用时要注意：*
-- 如果事件有多个监听者，不要在DispatchEvent，中调用Remove
function EventDispatcher:RemoveListener( eventName, target )
	eventName = tostring( eventName );
    if self._listeners[eventName] == nil then 
    	return ;
    end

    for i, listener in pairs( self._listeners[eventName] ) do
        if listener == target then
            table.remove( self._listeners[eventName], i );
        end
    end
end
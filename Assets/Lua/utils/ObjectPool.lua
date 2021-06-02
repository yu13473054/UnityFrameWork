---------------------------------
-- Lua Class对象池
-- 自己管理好池的清空时机
---------------------------------
---@class ObjectPool
ObjectPool = Class( "ObjectPool" );

-- 构造函数，传需实例化的对象
function ObjectPool:Ctor( class )
    self.instanceTarget = class;
    -- 产出列表
    self.spawnList = {};
    -- 缓存列表
    self.cacheList = {};
end

-- 拿一个对象，传构造参数
function ObjectPool:Spawn(...)
    local obj;
    if #self.cacheList > 0 then
        -- 如果不是预加载，缓存里还存在，直接拿
        obj = self.cacheList[1];
        table.remove( self.cacheList, 1 );
        -- 调初始化方法
        obj:Ctor(...);
    else
        -- 缓存里不存在，新建
        obj = self.instanceTarget.New(...);
    end

    -- 压入产出列表
    table.insert( self.spawnList, obj );
    return obj;
end

-- 回收一个对象 
function ObjectPool:Despawn( obj )
    -- 从产出列表中删除
    local found = false;
    for i, v in ipairs( self.spawnList ) do
        if v == obj then
            table.remove( self.spawnList, i );
            table.insert( self.cacheList, obj );
            break;
        end
    end
end

-- 回收所有
function ObjectPool:DespawnAll()
    for i, obj in ipairs( self.spawnList ) do
        table.insert( self.cacheList, obj );
    end
    self.spawnList = {};
end

-- 销毁所有
function ObjectPool:Clear()
    self.spawnList = {};
    self.cacheList = {};
end

-- 所有产出列表中的调析构，并置空
function ObjectPool:Destroy()
    local destoryList = {};    
    for i = 1, #self.spawnList do
        table.insert( destoryList, self.spawnList[i] );
    end

    for i = 1, #destoryList do
        local obj = destoryList[i];
        if obj.Destroy ~= nil then
            obj:Destroy();
        end
    end
    self:Clear();
end


---------------------------------
-- GameObject对象池
-- 自己管理好池的清空时机
---------------------------------
---@class GameObjectPool
GameObjectPool = Class( "GameObjectPool" );

-- 回收GameObject的地方
local _cacheRoot = nil;

-- 构造函数，传需实例化的对象, instFunc可以为空，如果不为空，就调用该方法，对实例化对象进行获取组件的一些操作
-- isInsertCache：当instanceTarget是已实例化的对象时，isInsertCache必须为True，否则为false。即通过ResManager.instance.LoadPrefab出来的未实例化对象isInsertCache为false。
-- autoAddResModule：创建新实例时，自动挂载ResModuleUntility，默认False
function GameObjectPool:Ctor( instanceTarget, isInsertCache, instFunc, autoAddResModule )
    if _cacheRoot == nil then
        _cacheRoot = GameObject.Find( "DefaultPoolRoot" ).transform;
    end

    -- 产出列表
    self.spawnList = {};
    -- 缓存列表
    self.cacheList = {};

    --是否需要缓存table对象
    if instFunc ~= nil then
        self.isInstFunc = true;
        self.instFunc = instFunc;
    end

    self.instanceTarget = instanceTarget;
    self.autoAddResModule = autoAddResModule or false;
    
    if isInsertCache then
        instanceTarget:SetActive( false );
        instanceTarget.transform:SetParent( _cacheRoot );
        if self.isInstFunc then
            local tableObj = instFunc(instanceTarget);
            tableObj.inst = instanceTarget;
            table.insert( self.cacheList,  tableObj );
        else
            table.insert( self.cacheList, instanceTarget );
        end
    end
end

-- 拿一个对象
function GameObjectPool:Spawn( isPreSpawn )
    ---@type UnityEngine.GameObject
    local go;
    if not isPreSpawn and #self.cacheList > 0 then
        -- 如果不是预加载，缓存里还存在，直接拿
        go = self.cacheList[1];
        table.remove( self.cacheList, 1 );
        if self.isInstFunc then
            go.inst:SetActive(true);
        else
            go:SetActive( true );
        end
        -- 压入产出列表
        table.insert( self.spawnList, go );
        return go;
    else
        -- 缓存里不存在，新建
        go = GameObject.Instantiate( self.instanceTarget );
        -- 自动挂载资源管理
        if self.autoAddResModule then
            Utils.AddComponent( go, typeof( ResModuleUtility ) );
        end
        go.name = self.instanceTarget.name .. "_" .. ( #self.cacheList + #self.spawnList + 1 );
    end

    if isPreSpawn then
        -- 如果是预加载直接
        go.transform:SetParent( _cacheRoot );
        go:SetActive( false );
        -- 如果有初始化方法，对产生的新对象进行初始化
        if self.isInstFunc then
            local tableObj = self.instFunc(go);
            tableObj.inst = go;
            go = tableObj;
        end
        -- 缓存
        table.insert( self.cacheList, go );
    else
        -- 初始化
        go:SetActive( true );
        -- 如果有初始化方法，对产生的新对象进行初始化
        if self.isInstFunc then
            ---@class SuperPoolObj
            local tableObj = self.instFunc(go);
            tableObj.inst = go;
            go = tableObj;
        end
        -- 压入产出列表
        table.insert( self.spawnList, go );
    end
    return go;
end

-- 回收一个对象，如果是isInstFunc，传.inst
function GameObjectPool:Despawn( go )
    -- 从产出列表中删除
    for i, v in ipairs( self.spawnList ) do
        local realObj = v;
        -- 有初始化方法情况下，v.inst才是真正的实例化的GameObject
        if self.isInstFunc then
            realObj = v.inst;
        end

        if realObj == go then
            table.remove( self.spawnList, i );
            if IsNilOrNull(realObj) then return; end
            -- 重置
            realObj.transform:SetParent( _cacheRoot, false );
            realObj:SetActive( false );

            -- 压入缓存列表
            table.insert( self.cacheList, v );
            return;
        end
    end
end

-- 回收所有
function GameObjectPool:DespawnAll()
    if IsNilOrNull( _cacheRoot ) then return end;
    for i, v in ipairs( self.spawnList ) do
        local realObj = v;
        -- 有初始化方法情况下，v.inst才是真正的实例化的GameObject
        if self.isInstFunc then
            realObj = v.inst;
        end
        if not IsNilOrNull( realObj ) then
            realObj.transform:SetParent( _cacheRoot, false );
            realObj:SetActive( false );

            table.insert( self.cacheList, v );
        end
    end
    self.spawnList = {};
end

-- 检测GO是否已经被回收
function GameObjectPool:IsDespawn( gameObject )
    for i = 1, #self.cacheList, 1 do
        if gameObject == self.cacheList[i]  then
           return true;
        end
    end
    return false;
end

-- 销毁所有
function GameObjectPool:Clear()
    for i, v in ipairs( self.spawnList ) do
        -- 有初始化方法情况下，v.inst才是真正的实例化的GameObject        
        local realObj = v;
        if self.isInstFunc then
            realObj = v.inst;
        end

        if not IsNilOrNull( realObj ) then
            GameObject.Destroy( realObj );
        end
    end
    for i, v in ipairs( self.cacheList ) do
        -- 有初始化方法情况下，v.inst才是真正的实例化的GameObject
        local realObj = v;
        if self.isInstFunc then
            realObj = v.inst;
        end

        if not IsNilOrNull( realObj ) then
            GameObject.Destroy( realObj );
        end
    end
    self.spawnList = {};
    self.cacheList = {};
    self.instFunc = nil;
    self.instanceTarget = nil;
end
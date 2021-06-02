--------------------------------------------
-- 时间管理器：集中生成，集中销毁
--------------------------------------------
TimerManager = DefineConst("TimerManager", {});

local _UpdateClock = 5 * 60 * 60; --每天5:00

local _moduleList = {};
local _timerPool = ObjectPool.New(Timer);
local _stampTimerPool = ObjectPool.New(StampTimer);

-- 新建一个Timer
function TimerManager.SpawnTimer( moduleName, func, dura, loop, unscaled, isStamp )
    -- 没有模块，创建一个
    if _moduleList[moduleName] == nil then
        _moduleList[moduleName] = {};
    end

    local timer;
    if isStamp then
        timer = _stampTimerPool:Spawn(func, dura, loop);
    else
        timer = _timerPool:Spawn(func, dura, loop, unscaled);
    end
    timer.isStamp = isStamp; --记录下类型
    _moduleList[moduleName][timer] = timer;

    --单次的计时器，自动销毁
    if not loop or loop == 1 then
        timer.func = function()
            func();
            TimerManager.DespawnTimer( moduleName, timer );
        end
    end
    timer:Start();
    return timer;
end
function TimerManager.SpawnStampTimer( moduleName, func, dura, loop )
    return TimerManager.SpawnTimer( moduleName, func, dura, loop, nil, true );
end

-- 销毁指定的timer
function TimerManager.DespawnTimer( moduleName, timer )
    if _moduleList[moduleName] == nil then
        return;
    end
    local safeTimer = _moduleList[moduleName][timer];
    if safeTimer then
        if safeTimer.running then
            safeTimer:Stop();
        end
        _moduleList[moduleName][safeTimer] = nil;
        safeTimer:Reset(nil, 0);
        if safeTimer.isStamp then
            _stampTimerPool:Despawn(safeTimer);
        else
            _timerPool:Despawn(safeTimer);
        end
    end
end

-- 销毁指定模块的timer
function TimerManager.DespawnModule( moduleName )
    if _moduleList[moduleName] == nil then
        return;
    end    
    for i, timer in pairs( _moduleList[moduleName] ) do
        TimerManager.DespawnTimer( moduleName, timer );
    end
    _moduleList[moduleName] = nil;
end

function TimerManager.PauseTimerByModule(moduleName)
    if _moduleList[moduleName] == nil then
        return;
    end
    for i, timer in pairs( _moduleList[moduleName] ) do
        timer.running = false;
    end
end
function TimerManager.ReStartTimerByModule(moduleName)
    if _moduleList[moduleName] == nil then
        return;
    end
    for i, timer in pairs( _moduleList[moduleName] ) do
        timer.running = true;
    end
end

-- 清空所有
function TimerManager.DespawnAll()
    for moduleName, moduleList in pairs( _moduleList ) do
        TimerManager.DespawnModule( moduleName );
    end  
    _moduleList = {};  
end

local _timeOffset = 0;
--获取服务器时间：month （1–12），day （1–31）， hour （0–23），min （0–59），sec （0–61）， wday （星期几，星期天为 1 ）， yday （当年的第几天）
function TimerManager.GetServerTimeUtc()
    return os.date("!*t", TimerManager.GetServerTimeStamp());
end

--获取服务器时间：month （1–12），day （1–31）， hour （0–23），min （0–59），sec （0–61）， wday （星期几，星期天为 1 ）， yday （当年的第几天）
function TimerManager.GetServerTime()
    return os.date("*t", TimerManager.GetServerTimeStamp());
end

--获取当天0点的时间戳
function TimerManager.GetToday0()
    local date = TimerManager.GetServerTime();
    date.hour = 0;
    return os.time( date );
end

--获取服务器时间戳
function TimerManager.GetServerTimeStamp()
    return os.time() + _timeOffset;
end

local _dateTimeStamp = nil; --客户端记录的日期的时间戳
function TimerManager.CheckDay(timeStamp)
    if not PlayerCache.useriniByAccout then return; end -- 尚未初始化
    --计算时间差，是否到了新的一天
    if not _dateTimeStamp then
        local localDate = PlayerCache.useriniByAccout:ReadValue("NPC_DateStamp", "");
        if localDate == "" then
            local date = os.date("*t", timeStamp);
            _dateTimeStamp = os.time({year = date.year, month = date.month, day = date.day, hour = 0});
            PlayerCache.useriniByAccout:WriteValue("NPC_DateStamp", date.year .."/" .. date.month .."/" .. date.day);
        else
            local result = string.split(localDate, '/');
            _dateTimeStamp = os.time({year = tonumber(result[1]), month = tonumber(result[2]), day = tonumber(result[3]), hour = 0});
        end
    end
    if timeStamp - _dateTimeStamp - _UpdateClock > 24 * 3600 then --时间差大于一天
        EventDispatcher.DispatchEvent(EVENT_NET_NEWDAY);
        local date = os.date("*t", timeStamp);
        _dateTimeStamp = os.time({year = date.year, month = date.month, day = date.day, hour = 0});
        PlayerCache.useriniByAccout:WriteValue("NPC_DateStamp", date.year .."/" .. date.month .."/" .. date.day);
    end
end

----服务器同步时间
--local _cheatDetected = false;
--local _firstOffset;
--local function TimerManager_OnSCServerTime(msg)
--    msg = common_pb.SCServerTime():Unpack(msg);
--
--    --计算时间偏移量
--    local serverTimeStamp = Mathf.Ceil(msg.millisecond / 1000);
--    _timeOffset = serverTimeStamp - os.time();
--
--    TimerManager.CheckDay(serverTimeStamp);
--
--    -- 设置初值
--    if _firstOffset == nil then
--        _firstOffset = _timeOffset;
--    end
--
--    ---- 如果和初始偏移值过大表明使用了加速挂，踢下线
--    ---- 目前1分钟
--    --if math.abs( _timeOffset - _firstOffset ) > 60 and not _cheatDetected then
--    --    LogErr( "<作弊检测> 检测到加速挂：" .. _timeOffset - _firstOffset );
--    --    _cheatDetected = true;
--    --    DlgConfirmBox.Show( function() Application.Quit() end, nil, Localization.Get( "cheat_detected" ), Localization.Get( "quit_game" ) );
--    --end
--end
--Event.Net:AddListener(MsgId.SCServerTime, TimerManager_OnSCServerTime)
--[[ 这里放一些公用定义，和一些公用逻辑方法 ]]
----------------------------------
-- UI定义
----------------------------------
-- UILayer
UILAYER_FULL                = 0;
UILAYER_POP                 = 1;
UILAYER_TOP                 = 2;

--UI事件
UIEVENT_UIBUTTON_CLICK = 10;                -- UIButton单击                 无参
UIEVENT_UIBUTTON_PRESS = 11;		        -- UIButton按下					0 按下，1 抬起

UIEVENT_UITOGGLE_CLICK = 22;                -- UIToggle单击                 无参
UIEVENT_UITOGGLE_PRESS = 23;		        -- UIToggle按下                 0 按下，1 抬起
UIEVENT_UITOGGLE_ONVALUECHANGE = 21;	    -- UIToggle内容发生变化时        bool值

UIEVENT_UISLIDER_DRAG = 31;                 -- UISlider拖动                 0 开始拖动，1 拖动中，2 结束拖动
UIEVENT_UISLIDER_PRESS = 34;                -- UISlider按下                 0 按下，1 抬起

UIEVENT_CAMERA_CLICK = 41;                  -- Camera单击，也是抬起			 组件的名称作为标志值。无controlID
UIEVENT_CAMERA_PRESS = 42;                  -- Camera按下					组件的名称作为标志值。无controlID

UIEVENT_UISCROLLVIEW_DRAG = 51;		        -- UIScrollView拖动             0 开始拖动，1 拖动中，2 结束拖动
UIEVENT_UISCROLLVIEW_ONVALUECHANGE = 52;	-- UIScrollView内容发生变化时    Vector2对象
UIEVENT_WRAPCONTENT_ONITEMUPDATE = 53;	    -- WrapContent中Item更新        自定义对象：index，Transform
UIEVENT_WRAPCONTENT_ONINITDONE = 54;	    -- WrapContent中初始化完成       无
UIEVENT_UIWRAPVARCONTENT_ADD = 55;          -- UIWrapVarContent控件变动      内容编号，GameObject对象

UIEVENT_UIINPUT_SUBMIT = 61;                --         

UIEVENT_UISCROLLBAR_ONVALUECHANGE = 71;	    -- UIScrollbar内容发生变化时     float值
UIEVENT_UISCROLLBAR_PRESS = 72;	            -- UIScrollbar按下              0 按下，1 抬起

----------------------------------
-- 事件分发实例
----------------------------------
Event = {};
Event.Net = EventDispatcher.New();
Event.Fight = EventDispatcher.New();
Event.UI = EventDispatcher.New();

----------------------------------
-- 基础方法
----------------------------------
--星级对应等级上限
function LVLimitByStar(starNum)
    if starNum ==1 then
        return 40;
    elseif starNum ==2 then
        return 55;
    elseif starNum ==3 then
        return 70;
    elseif starNum ==4 then
        return 85;
    else
        return 100;
    end
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

function PlayTweenAnim(go)
    local anims = go:GetComponents(typeof(DOTweenAnimation));

	local longestAnim = anims[0];
	local dura = 0;
    --获取时间最长的animation
	for i = 0, anims.Length-1 do
	    local anim = anims[i];
	    anim:DOPlay();

	    if anim.duration > dura then
	        dura = anim.duration;
	        longestAnim = anim;
	    end
	end
    return longestAnim;
end


using System;
using System.Collections.Generic;

/// 每个模块需要单独设置一个EventDispatcher用来管理相应模块的事件
public class EventMgr : Singleton<EventMgr>
{
    //按照模块分发
    public EventDispatcher<EventID> CommonEvt { get; } = new EventDispatcher<EventID>();

    //网络模块
    public EventDispatcher<int> NetworkEvt { get; } = new EventDispatcher<int>();

    //红点
    public EventDispatcher<int> RedPointEvt { get; } = new EventDispatcher<int>();
}



using System;
using System.Collections.Generic;

/// 每个模块需要单独设置一个EventDispatcher用来管理相应模块的事件
public class EventMgr : Singleton<EventMgr>
{
    //按照模块分发
    private EventDispatcher<string> _commonEvt = new EventDispatcher<string>();
    public EventDispatcher<string> CommonEvt
    {
        get { return _commonEvt; }
    }
    //网络模块
    private EventDispatcher<int> _networkEvt = new EventDispatcher<int>();
    public EventDispatcher<int> NetworkEvt
    {
        get { return _networkEvt; }
    }
    //红点
    private EventDispatcher<int> _redPointEvt = new EventDispatcher<int>();
    public EventDispatcher<int> RedPointEvt
    {
        get { return _redPointEvt; }
    }
}



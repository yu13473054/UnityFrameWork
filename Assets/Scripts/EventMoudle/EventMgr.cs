using System;
using System.Collections.Generic;

/// 每个模块需要单独设置一个EventDispatcher用来管理相应模块的事件
public class EventMgr
{
    private static EventMgr _inst;

    public static EventMgr Inst
    {
        get
        {
            return _inst;
        }
    }

    public static void Init()
    {
        if (_inst == null)
            _inst = new EventMgr();
    }

    public void OnDestory()
    {
        _inst = null;
    }

    //按照模块分发
    private EventDispatcher _commonEvt = new EventDispatcher();
    private EventDispatcher _networkEvt = new EventDispatcher();

    public EventDispatcher CommonEvt
    {
        get { return _commonEvt; }
    }

    public EventDispatcher NetworkEvt
    {
        get { return _networkEvt; }
    }
}



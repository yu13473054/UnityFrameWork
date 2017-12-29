using UnityEngine;
using System.Collections;
using LuaFramework;
using System.Collections.Generic;

public class Base : MonoBehaviour {
    private AppFacade m_Facade;
    private NetworkMgr m_NetMgr;
    private SoundMgr m_SoundMgr;
    private TimerMgr m_TimerMgr;
    private ThreadManager m_ThreadMgr;
    private PoolMgr _mPoolMgr;

    /// <summary>
    /// 注册消息
    /// </summary>
    /// <param name="view"></param>
    /// <param name="messages"></param>
    protected void RegisterMessage(IView view, List<string> messages) {
        if (messages == null || messages.Count == 0) return;
        Controller.Instance.RegisterViewCommand(view, messages.ToArray());
    }

    /// <summary>
    /// 移除消息
    /// </summary>
    /// <param name="view"></param>
    /// <param name="messages"></param>
    protected void RemoveMessage(IView view, List<string> messages) {
        if (messages == null || messages.Count == 0) return;
        Controller.Instance.RemoveViewCommand(view, messages.ToArray());
    }

    protected AppFacade facade {
        get {
            if (m_Facade == null) {
                m_Facade = AppFacade.Instance;
            }
            return m_Facade;
        }
    }

    protected NetworkMgr NetMgr {
        get {
            if (m_NetMgr == null) {
                m_NetMgr = facade.GetManager<NetworkMgr>(ManagerName.Network);
            }
            return m_NetMgr;
        }
    }

    protected SoundMgr SoundMgr {
        get {
            if (m_SoundMgr == null) {
                m_SoundMgr = facade.GetManager<SoundMgr>(ManagerName.Sound);
            }
            return m_SoundMgr;
        }
    }

    protected TimerMgr TimerMgr {
        get {
            if (m_TimerMgr == null) {
                m_TimerMgr = facade.GetManager<TimerMgr>(ManagerName.Timer);
            }
            return m_TimerMgr;
        }
    }

    protected ThreadManager ThreadManager {
        get {
            if (m_ThreadMgr == null) {
                m_ThreadMgr = facade.GetManager<ThreadManager>(ManagerName.Thread);
            }
            return m_ThreadMgr;
        }
    }

    protected PoolMgr ObjPoolMgr {
        get {
            if (_mPoolMgr == null) {
                _mPoolMgr = facade.GetManager<PoolMgr>(ManagerName.ObjectPool);
            }
            return _mPoolMgr;
        }
    }
}

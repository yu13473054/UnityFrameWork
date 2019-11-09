using System;
using System.Collections.Generic;
using UnityEngine;

public class EventDispatcher
{
    private Dictionary<object, Delegate> _dicEvents = new Dictionary<object, Delegate>();

    void Add(object eventId, Delegate listener)
    {
        if (!_dicEvents.ContainsKey(eventId))
        {
            _dicEvents.Add(eventId, listener);
        }
        else
        {
            Delegate tmpDel = _dicEvents[eventId];
            if (tmpDel.GetType() != listener.GetType())
            {
                Debug.LogError("需要添加的委托，参数类型不一致！eventId = "+eventId);
                return;
            }
            _dicEvents[eventId] = Delegate.Combine(tmpDel, listener);
        }
    }
    void Remove(object eventId, Delegate listener)
    {
        if (!_dicEvents.ContainsKey(eventId))
            return;

        Delegate tmpDel = _dicEvents[eventId];
        if (tmpDel.GetType() != listener.GetType())
        {
            Debug.LogError("需要移除的委托，参数类型不一致！eventId = " + eventId);
            return;
        }
        tmpDel = Delegate.Remove(tmpDel, listener);
        //移除后，如果没有委托就移除键值对
        if (tmpDel == null)
        {
            _dicEvents.Remove(eventId);
        }
        else
        {
            _dicEvents[eventId] = tmpDel;
        }
    }

    #region Void
    public void AddEventListener(object eventId, Action listener)
    {
        Add(eventId,listener);
    }

    public void RemoveEventListener(object eventId, Action listener)
    {
        Remove(eventId, listener);
    }

    public void TriggerEvent(object eventId)
    {
        Delegate del = null;
        if (_dicEvents.TryGetValue(eventId, out del))
        {
            del.DynamicInvoke();
        }
    }

    #endregion

    #region One param
    public void AddEventListener<T>(object eventId, Action<T> listener)
    {
        Add(eventId, listener);
    }

    public void RemoveEventListener<T>(object eventId, Action<T> listener)
    {
        Remove(eventId, listener);
    }

    public void TriggerEvent<T>(object eventId, T p)
    {
        Delegate del = null;
        if (_dicEvents.TryGetValue(eventId, out del))
        {
            del.DynamicInvoke(p);
        }
    }
    #endregion

    #region Two params
    public void AddEventListener<T0, T1>(object eventId, Action<T0, T1> listener)
    {
        Add(eventId, listener);
    }

    public void RemoveEventListener<T0, T1>(object eventId, Action<T0, T1> listener)
    {
        Remove(eventId, listener);
    }

    public void TriggerEvent<T0, T1>(object eventId, T0 p0, T1 p1)
    {
        Delegate del = null;
        if (_dicEvents.TryGetValue(eventId, out del))
        {
            del.DynamicInvoke(p0, p1);
        }
    }
    #endregion

    #region Thress params
    public void AddEventListener<T0, T1, T2>(object eventId, Action<T0, T1, T2> listener)
    {
        Add(eventId, listener);
    }

    public void RemoveEventListener<T0, T1, T2>(object eventId, Action<T0, T1, T2> listener)
    {
        Remove(eventId, listener);
    }

    public void TriggerEvent<T0, T1, T2>(object eventId, T0 p0, T1 p1, T2 p2)
    {
        Delegate del = null;
        if (_dicEvents.TryGetValue(eventId, out del))
        {
            del.DynamicInvoke(p0, p1, p2);
        }
    }
    #endregion

    #region four params
    public void AddEventListener<T0, T1, T2, T3>(object eventId, Action<T0, T1, T2, T3> listener)
    {
        Add(eventId, listener);
    }

    public void RemoveEventListener<T0, T1, T2, T3>(object eventId, Action<T0, T1, T2, T3> listener)
    {
        Remove(eventId, listener);
    }

    public void TriggerEvent<T0, T1, T2, T3>(object eventId, T0 p0, T1 p1, T2 p2, T3 p3)
    {
        Delegate del = null;
        if (_dicEvents.TryGetValue(eventId, out del))
        {
            del.DynamicInvoke(p0, p1, p2, p3);
        }
    }
    #endregion

}
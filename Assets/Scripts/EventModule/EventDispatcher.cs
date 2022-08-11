using System;
using System.Collections.Generic;
using UnityEngine;

public class EventDispatcher<K>
{
    private readonly Dictionary<K, Delegate> _dicEvents = new Dictionary<K, Delegate>();

    void Add(K eventId, Delegate listener)
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
    void Remove(K eventId, Delegate listener)
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
    public void AddEventListener(K eventId, Action listener)
    {
        Add(eventId,listener);
    }

    public void RemoveEventListener(K eventId, Action listener)
    {
        Remove(eventId, listener);
    }

    public void TriggerEvent(K eventId)
    {
        if (!_dicEvents.TryGetValue(eventId, out var del)) return;
        if (del is Action action)
        {
            action.Invoke();
        }
        else
        {
            Debug.LogError($"<Event> 参数不匹配！eventId = {eventId}");
        }
    }

    #endregion

    #region One param
    public void AddEventListener<T>(K eventId, Action<T> listener)
    {
        Add(eventId, listener);
    }

    public void RemoveEventListener<T>(K eventId, Action<T> listener)
    {
        Remove(eventId, listener);
    }

    public void TriggerEvent<T>(K eventId, T p)
    {
        if (!_dicEvents.TryGetValue(eventId, out var del)) return;
        if (del is Action<T> action)
        {
            action.Invoke(p);
        }
        else
        {
            Debug.LogError($"<Event> 参数不匹配！eventId = {eventId}，param1 = {del.Method.GetParameters()[0].ParameterType}");
        }
    }
    #endregion

    #region Two params
    public void AddEventListener<T0, T1>(K eventId, Action<T0, T1> listener)
    {
        Add(eventId, listener);
    }

    public void RemoveEventListener<T0, T1>(K eventId, Action<T0, T1> listener)
    {
        Remove(eventId, listener);
    }

    public void TriggerEvent<T0, T1>(K eventId, T0 p0, T1 p1)
    {
        if (!_dicEvents.TryGetValue(eventId, out var del)) return;
        if (del is Action<T0, T1> action)
        {
            action.Invoke(p0, p1);
        }
        else
        {
            var parameterInfos = del.Method.GetParameters();
            Debug.LogError(
                $"<Event> 参数不匹配！eventId = {eventId}，param1 = {parameterInfos[0].ParameterType}," +
                $" param2 = {parameterInfos[1].ParameterType}");
        }
    }
    #endregion

    #region Thress params
    public void AddEventListener<T0, T1, T2>(K eventId, Action<T0, T1, T2> listener)
    {
        Add(eventId, listener);
    }

    public void RemoveEventListener<T0, T1, T2>(K eventId, Action<T0, T1, T2> listener)
    {
        Remove(eventId, listener);
    }

    public void TriggerEvent<T0, T1, T2>(K eventId, T0 p0, T1 p1, T2 p2)
    {
        if (!_dicEvents.TryGetValue(eventId, out var del)) return;
        if (del is Action<T0, T1, T2> action)
        {
            action.Invoke(p0, p1, p2);
        }
        else
        {
            var parameterInfos = del.Method.GetParameters();
            Debug.LogError(
                $"<Event> 参数不匹配！eventId = {eventId}，param1 = {parameterInfos[0].ParameterType}," +
                $" param2 = {parameterInfos[1].ParameterType},  param3 = {parameterInfos[2].ParameterType}");
        }
    }
    #endregion

    #region four params
    public void AddEventListener<T0, T1, T2, T3>(K eventId, Action<T0, T1, T2, T3> listener)
    {
        Add(eventId, listener);
    }

    public void RemoveEventListener<T0, T1, T2, T3>(K eventId, Action<T0, T1, T2, T3> listener)
    {
        Remove(eventId, listener);
    }

    public void TriggerEvent<T0, T1, T2, T3>(K eventId, T0 p0, T1 p1, T2 p2, T3 p3)
    {
        if (!_dicEvents.TryGetValue(eventId, out var del)) return;
        if (del is Action<T0, T1, T2, T3> action)
        {
            action.Invoke(p0, p1, p2, p3);
        }
        else
        {
            var parameterInfos = del.Method.GetParameters();
            Debug.LogError(
                $"<Event> 参数不匹配！eventId = {eventId}，param1 = {parameterInfos[0].ParameterType}," +
                $" param2 = {parameterInfos[1].ParameterType},  param3 = {parameterInfos[2].ParameterType}," +
                $" param4 = {parameterInfos[3].ParameterType}");
        }
    }
    #endregion
    
    #region five params
    public void AddEventListener<T0, T1, T2, T3, T4>(K eventId, Action<T0, T1, T2, T3, T4> listener)
    {
        Add(eventId, listener);
    }

    public void RemoveEventListener<T0, T1, T2, T3, T4>(K eventId, Action<T0, T1, T2, T3, T4> listener)
    {
        Remove(eventId, listener);
    }

    public void TriggerEvent<T0, T1, T2, T3, T4>(K eventId, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4)
    {
        if (!_dicEvents.TryGetValue(eventId, out var del)) return;
        if (del is Action<T0, T1, T2, T3, T4> action)
        {
            action.Invoke(p0, p1, p2, p3, p4);
        }
        else
        {
            var parameterInfos = del.Method.GetParameters();
            Debug.LogError(
                $"<Event> 参数不匹配！eventId = {eventId}，param1 = {parameterInfos[0].ParameterType}," +
                $" param2 = {parameterInfos[1].ParameterType}, param3 = {parameterInfos[2].ParameterType}," +
                $" param4 = {parameterInfos[3].ParameterType}, param5 = {parameterInfos[4].ParameterType}");
        }
    }
    #endregion
    
    #region six params
    public void AddEventListener<T0, T1, T2, T3, T4, T5>(K eventId, Action<T0, T1, T2, T3, T4, T5> listener)
    {
        Add(eventId, listener);
    }

    public void RemoveEventListener<T0, T1, T2, T3, T4, T5>(K eventId, Action<T0, T1, T2, T3, T4, T5> listener)
    {
        Remove(eventId, listener);
    }

    public void TriggerEvent<T0, T1, T2, T3, T4, T5>(K eventId, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
    {
        if (!_dicEvents.TryGetValue(eventId, out var del)) return;
        if (del is Action<T0, T1, T2, T3, T4, T5> action)
        {
            action.Invoke(p0, p1, p2, p3, p4, p5);
        }
        else
        {
            var parameterInfos = del.Method.GetParameters();
            Debug.LogError(
                $"<Event> 参数不匹配！eventId = {eventId}，param1 = {parameterInfos[0].ParameterType}," +
                $" param2 = {parameterInfos[1].ParameterType}, param3 = {parameterInfos[2].ParameterType}," +
                $" param4 = {parameterInfos[3].ParameterType}, param5 = {parameterInfos[4].ParameterType}," +
                $" param5 = {parameterInfos[4].ParameterType}");
        }
    }
    #endregion
    
    #region seven params
    public void AddEventListener<T0, T1, T2, T3, T4, T5, T6>(K eventId, Action<T0, T1, T2, T3, T4, T5, T6> listener)
    {
        Add(eventId, listener);
    }

    public void RemoveEventListener<T0, T1, T2, T3, T4, T5, T6>(K eventId, Action<T0, T1, T2, T3, T4, T5, T6> listener)
    {
        Remove(eventId, listener);
    }

    public void TriggerEvent<T0, T1, T2, T3, T4, T5, T6>(K eventId, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6)
    {
        if (!_dicEvents.TryGetValue(eventId, out var del)) return;
        if (del is Action<T0, T1, T2, T3, T4, T5, T6> action)
        {
            action.Invoke(p0, p1, p2, p3, p4, p5, p6);
        }
        else
        {
            var parameterInfos = del.Method.GetParameters();
            Debug.LogError(
                $"<Event> 参数不匹配！eventId = {eventId}，param1 = {parameterInfos[0].ParameterType}," +
                $" param2 = {parameterInfos[1].ParameterType}, param3 = {parameterInfos[2].ParameterType}," +
                $" param4 = {parameterInfos[3].ParameterType}, param5 = {parameterInfos[4].ParameterType}," +
                $" param5 = {parameterInfos[4].ParameterType}, param6 = {parameterInfos[5].ParameterType}");
        }
    }
    #endregion
    
    #region eight params
    public void AddEventListener<T0, T1, T2, T3, T4, T5, T6, T7>(K eventId, Action<T0, T1, T2, T3, T4, T5, T6, T7> listener)
    {
        Add(eventId, listener);
    }

    public void RemoveEventListener<T0, T1, T2, T3, T4, T5, T6, T7>(K eventId, Action<T0, T1, T2, T3, T4, T5, T6, T7> listener)
    {
        Remove(eventId, listener);
    }

    public void TriggerEvent<T0, T1, T2, T3, T4, T5, T6, T7>(K eventId, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7)
    {
        if (!_dicEvents.TryGetValue(eventId, out var del)) return;
        if (del is Action<T0, T1, T2, T3, T4, T5, T6, T7> action)
        {
            action.Invoke(p0, p1, p2, p3, p4, p5, p6, p7);
        }
        else
        {
            var parameterInfos = del.Method.GetParameters();
            Debug.LogError(
                $"<Event> 参数不匹配！eventId = {eventId}，param1 = {parameterInfos[0].ParameterType}," +
                $" param2 = {parameterInfos[1].ParameterType}, param3 = {parameterInfos[2].ParameterType}," +
                $" param4 = {parameterInfos[3].ParameterType}, param5 = {parameterInfos[4].ParameterType}," +
                $" param5 = {parameterInfos[4].ParameterType}, param6 = {parameterInfos[5].ParameterType}," +
                $" param7 = {parameterInfos[6].ParameterType}");
        }
    }
    #endregion
}
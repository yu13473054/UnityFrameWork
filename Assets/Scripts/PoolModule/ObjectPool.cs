using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectPool<T> where T : class
{
    private readonly Stack<T> _stack = new Stack<T>();
    private readonly Action<T> _onSpawn;
    private readonly Action<T> _onDespawn;

    public ObjectPool(Action<T> actionOnSpawn, Action<T> actionOnDespawn)
    {
        _onSpawn = actionOnSpawn;
        _onDespawn = actionOnDespawn;
    }

    public T Spawn()
    {
        T element = _stack.Pop();
        if (_onSpawn != null)
            _onSpawn(element);
        if(element == null)
            element = default(T);
        return element;
    }

    public void Despawn(T element)
    {
        if (_stack.Count > 0 && ReferenceEquals(_stack.Peek(), element))
            Debug.LogError("<ObjectPool> Trying to destroy object that is already released to pool.");
        if (_onDespawn != null)
            _onDespawn(element);
        _stack.Push(element);
    }
}

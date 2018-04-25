using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectPool<T> where T : class
{
    private readonly Stack<T> m_Stack = new Stack<T>();
    private readonly Action<T> _onSpawn;
    private readonly Action<T> _onDespawn;

    public int countAll { get; private set; }
    public int countActive { get { return countAll - countInactive; } }
    public int countInactive { get { return m_Stack.Count; } }

    public ObjectPool(Action<T> actionOnSpawn, Action<T> actionOnDespawn)
    {
        _onSpawn = actionOnSpawn;
        _onDespawn = actionOnDespawn;
    }

    public T Spawn()
    {
        T element = m_Stack.Pop();
        if (_onSpawn != null)
            _onSpawn(element);
        return element;
    }

    public void Despawn(T element)
    {
        if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element))
            Debug.LogError("<ObjectPool> Trying to destroy object that is already released to pool.");
        if (_onDespawn != null)
            _onDespawn(element);
        m_Stack.Push(element);
    }
}

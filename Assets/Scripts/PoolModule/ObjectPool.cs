using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectPool<T> where T : class,new()
{
    private readonly List<T> _spawnList = new List<T>();
    private readonly Stack<T> _cacheStack = new Stack<T>();
    private readonly Action<T> _onSpawn;
    private readonly Action<T> _onDespawn;

    public ObjectPool(Action<T> actionOnSpawn, Action<T> actionOnDespawn)
    {
        _onSpawn = actionOnSpawn;
        _onDespawn = actionOnDespawn;
    }

    public T Spawn()
    {
        T element;
        if (_cacheStack.Count > 0)
        {
            element = _cacheStack.Pop();
        }
        else
        {
            element =  new T();
        }
        if (_onSpawn != null)
            _onSpawn(element);
        _spawnList.Add(element);
        return element;
    }

    public void Despawn(T element)
    {
        bool isExist = _spawnList.Remove(element);
        if (!isExist)
        {
            Debug.LogError("<GameObjectPool> 回收一个未被使用的对象，请检查其来源！");
            return;
        }
        _cacheStack.Push(element);
        if (_onDespawn != null)
            _onDespawn(element);
    }

    public void DespawnAll()
    {
        for (int i = 0; i < _spawnList.Count; i++)
        {
            T element = _spawnList[i];
            _cacheStack.Push(element);
            if (_onDespawn != null)
                _onDespawn(element);
        }
        _spawnList.Clear();
    }

    public void Clear()
    {
        _spawnList.Clear();
        _cacheStack.Clear();
    }
}

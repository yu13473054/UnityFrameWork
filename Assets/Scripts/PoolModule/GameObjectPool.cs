using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public class GameObjectPool<T> where T : Component
{
    private Transform _poolRoot;
    private GameObject _prefab;

    private List<T> _spawnList = new List<T>();
    private Stack<T> _cacheStack = new Stack<T>();

    public GameObjectPool(GameObject go, GameObject poolRoot, bool isInstantiate = false) 
    {
        _prefab = go;
        _poolRoot = poolRoot.transform;

        //如果go已经实例化了，需要保存
        if (isInstantiate)
            _spawnList.Add(go.GetComponent<T>());
    }

    //产生一个新对象
    public T Spawn()
    {
        T t;
        if (_cacheStack.Count > 0)
            t = _cacheStack.Pop();
        else
        {
            GameObject go = Object.Instantiate(_prefab);
            t = go.GetComponent<T>();
        }
        t.gameObject.SetActive(true);
        _spawnList.Add(t);
        return t;
    }
    //回收一个使用的对象
    public void Despawn(T t)
    {
        bool isExist = _spawnList.Remove(t);
        if (isExist)
            MoveToPool(t);
        else
            Debug.LogError("<GameObjectPool> 回收一个未被使用的对象，请检查其来源！");
    }
    //回收所有使用的对象
    public void DespawnAll()
    {
        for(int i = 0; i < _spawnList.Count; i++)
        {
            MoveToPool(_spawnList[i]);
        }
        _spawnList.Clear();
    }
    //清空对象池
    public void Clear()
    {
        for (int i = 0; i < _spawnList.Count; i++)
        {
#if UNITY_EDITOR //编辑器模式中使用立即销毁的api
            Object.DestroyImmediate(_spawnList[i]);
#else
            Object.Destroy(_spawnList[i]);
#endif
        }
        _spawnList.Clear();
        while (_cacheStack.Count > 0)
        {
#if UNITY_EDITOR
            Object.DestroyImmediate(_cacheStack.Pop());
#else
            Object.Destroy(_cacheStack.Pop());
#endif
        }
        _cacheStack.Clear();
    }

    private void MoveToPool(T t)
    {
        t.gameObject.SetActive(false);
        _cacheStack.Push(t);
        t.transform.SetParent(_poolRoot, false);
    }
}

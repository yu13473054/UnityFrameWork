using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using Object = UnityEngine.Object;

/// <summary>
/// 对象池管理器，分普通类对象池+资源游戏对象池
/// </summary>
public class PoolMgr
{
    #region 初始化
    private static PoolMgr _inst;
    public static PoolMgr Inst
    {
        get
        {
            return _inst;
        }
    }
    public static void Init()
    {
        if (_inst == null)
        {
            _inst = new PoolMgr();
        }
    }

    public void OnDestory()
    {
        _defaultPoolRoot = null;
        _objPools.Clear();
        _inst = null;
    }

    #endregion

    private GameObject _defaultPoolRoot;//默认的GameObject缓存节点，需要外部指定

    private Dictionary<string, object> _objPools = new Dictionary<string, object>();

    private PoolMgr()
    {
        //查找默认的缓存节点
        _defaultPoolRoot = GameObject.Find("DefaultPoolRoot");
    }

    #region GameObject

    public GameObjectPool<T> CreateGoPool<T>(GameObject prefab, bool isInstantiate = false) where T : Component
    {
        var type = typeof(T);
        var pool = new GameObjectPool<T>(prefab, _defaultPoolRoot, isInstantiate);
        if (_objPools.ContainsKey(type.Name))
        {
            Debug.LogError("<PoolMgr> 已经创建过相同名称的对象池！" + type.Name);
        }
        _objPools[type.Name] = pool;
        return pool;
    }

    private GameObjectPool<T> GetGoPool<T>() where T : Component
    {
        var type = typeof(T);
        GameObjectPool<T> reslut = null;
        if (_objPools.ContainsKey(type.Name))
            reslut = _objPools[type.Name] as GameObjectPool<T>;
        return reslut;
    }

    public T SpawnGo<T>() where T : Component
    {
        var pool = GetGoPool<T>();
        if (pool != null)
        {
            return pool.Spawn();
        }
        return default(T);
    }

    public void DespawnGo<T>(T t) where T : Component
    {
        var pool = GetGoPool<T>();
        if (pool != null)
        {
            pool.Despawn(t);
        }
    }
    public void DespawnAllGo<T>() where T : Component
    {
        var pool = GetGoPool<T>();
        if (pool != null)
        {
            pool.DespawnAll();
        }
    }

    //销毁对象池
    public void ClearGoPool<T>() where T : Component
    {
        var pool = GetGoPool<T>();
        if (pool != null)
            pool.Clear();
        _objPools.Remove(typeof(T).Name);
    }

    #endregion

    #region Object

    //创建对象池
    public ObjectPool<T> CreateObjPool<T>(Action<T> actionOnGet, Action<T> actionOnRelease) where T : class,new()
    {
        var type = typeof(T);
        var pool = new ObjectPool<T>(actionOnGet, actionOnRelease);
        if (_objPools.ContainsKey(type.Name))
        {
            Debug.LogError("<PoolMgr> 已经创建过相同名称的对象池！" + type.Name);
        }
        _objPools[type.Name] = pool;
        return pool;
    }

    private ObjectPool<T> GetObjPool<T>() where T : class, new()
    {
        var type = typeof(T);
        ObjectPool<T> pool = null;
        if (_objPools.ContainsKey(type.Name))
            pool = _objPools[type.Name] as ObjectPool<T>;
        return pool;
    }

    //产生对象
    public T SpawnObj<T>() where T : class, new()
    {
        var pool = GetObjPool<T>();
        if (pool != null)
            return pool.Spawn();
        return default(T);
    }

    //回收
    public void DespawnObj<T>(T obj) where T : class, new()
    {
        var pool = GetObjPool<T>();
        if (pool != null)
            pool.Despawn(obj);
    }

    public void DespawnAllObj<T>() where T : class, new()
    {
        var pool = GetObjPool<T>();
        if (pool != null)
            pool.DespawnAll();
    }

    //销毁对象池
    public void ClearObjPool<T>() where T : class, new()
    {
        var pool = GetObjPool<T>();
        if (pool != null)
            pool.Clear();
        _objPools.Remove(typeof(T).Name);

    }

    #endregion
}
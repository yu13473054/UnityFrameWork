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
        //_gameObjPools.Clear();
        _inst = null;
    }

    #endregion

    private Transform _defaultPoolRoot;//默认的GameObject缓存节点，需要外部指定

    private Dictionary<string, object> _objPools = new Dictionary<string, object>();
    //private Dictionary<string, GameObjectPool> _gameObjPools = new Dictionary<string, GameObjectPool>();

    private PoolMgr()
    {
        //查找默认的缓存节点
        _defaultPoolRoot = GameObject.Find("DefaultPoolRoot").transform;
        Object.DontDestroyOnLoad(_defaultPoolRoot.gameObject);
    }

    //#region GameObject

    //public GameObjectPool CreatePool(string poolName, GameObject prefab, int initSize = 1, Transform poolCacheRoot = null)
    //{
    //    if (!poolCacheRoot) poolCacheRoot = _defaultPoolRoot;
    //    var pool = new GameObjectPool(poolName, prefab, initSize, poolCacheRoot);
    //    _gameObjPools[poolName] = pool;
    //    return pool;
    //}

    //public GameObjectPool GetPool(string poolName)
    //{
    //    GameObjectPool reslut = null;
    //    _gameObjPools.TryGetValue(poolName, out reslut);
    //    return reslut;
    //}

    //public GameObject Spawn(string poolName)
    //{
    //    GameObject result = null;
    //    GameObjectPool pool = GetPool(poolName);
    //    if (pool != null)
    //    {
    //        result = pool.NextAvailableObject();
    //    }
    //    else
    //    {
    //        Debug.LogError("<PoolMgr> Invalid pool name specified: " + poolName);
    //    }
    //    return result;
    //}

    //public void Despawn(string poolName, GameObject go)
    //{
    //    GameObjectPool pool = GetPool(poolName);
    //    if (pool != null)
    //    {
    //        pool.ReturnObjectToPool(poolName, go);
    //    }
    //    else
    //    {
    //        Debug.LogError("<PoolMgr> Invalid pool name specified: " + poolName);
    //    }
    //}

    //#endregion

    #region Object
    public ObjectPool<T> CreateObjPool<T>(Action<T> actionOnGet, Action<T> actionOnRelease) where T : class
    {
        var type = typeof(T);
        var pool = new ObjectPool<T>(actionOnGet, actionOnRelease);
        _objPools[type.Name] = pool;
        return pool;
    }

    public ObjectPool<T> GetObjPool<T>() where T : class
    {
        var type = typeof(T);
        ObjectPool<T> pool = null;
        if (_objPools.ContainsKey(type.Name))
        {
            pool = _objPools[type.Name] as ObjectPool<T>;
        }
        return pool;
    }

    public T SpawnObj<T>() where T : class
    {
        var pool = GetObjPool<T>();
        if (pool != null)
        {
            return pool.Spawn();
        }
        return default(T);
    }

    public void DespawnObj<T>(T obj) where T : class
    {
        var pool = GetObjPool<T>();
        if (pool != null)
        {
            pool.Despawn(obj);
        }
    }
    #endregion
}
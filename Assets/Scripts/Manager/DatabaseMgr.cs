using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void LoadTableDelegate(Dictionary<string, Dictionary<string, object>> dic);

public class DatabaseMgr : MonoBehaviour
{
    #region 初始化
    private static DatabaseMgr _inst;
    public static DatabaseMgr Inst
    {
        get { return _inst; }
    }

    #endregion

    private Dictionary<string, Dictionary<string, object>> _cacheData;//事先缓存的数据
    private Dictionary<string, Dictionary<string, object>> _tmpData;//随用随取的数据

    public LoadTableDelegate _cacheDelegate;

    void Awake()
    {
        _inst = this;
        DontDestroyOnLoad(gameObject);

        _cacheData = new Dictionary<string, Dictionary<string, object>>();
        _tmpData = new Dictionary<string, Dictionary<string, object>>();

        //            ab = ResMgr.Inst.getAB("cfgdata.ab");

        CacahData();
    }

    /// <summary>
    /// 事先缓存一些数据，防止在第一次获取到数据时，有卡顿
    /// </summary>
    void CacahData()
    {
        //添加需要事先加载的委托
        //_cacheDelegate += LoadUIResPathData;

        if (_cacheDelegate != null)
        {
            _cacheDelegate(_cacheData);
            _cacheDelegate = null;
        }
    }

    Dictionary<string, object> GetDic(Type type, LoadTableDelegate callBack)
    {
        string tableName = type.Name;
        Dictionary<string, object> resTable = null;
        if (_cacheData.ContainsKey(tableName))
        {
            resTable = _cacheData[tableName];
        }
        else if (_tmpData.ContainsKey(tableName))
        {
            resTable = _tmpData[tableName];
        }
        else
        {
            callBack(_tmpData);
            resTable = _tmpData[tableName];
        }
        return resTable;
    }

    public void ClearAllData()
    {
        _cacheData.Clear();
        _tmpData.Clear();
    }
    public void ClearTmpData()
    {
        _tmpData.Clear();
    }

    void OnDestroy()
    {
        //            ab = null;
        ClearAllData();

        Debug.Log("<DatabaseMgr> OnDestroy");
    }
}

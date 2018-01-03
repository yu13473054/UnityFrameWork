using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void LoadTableDelegate(Dictionary<string, Dictionary<string, object>> dic);

public class DatabaseMgr : MonoBehaviour
{
    #region ��ʼ��
    private static DatabaseMgr _inst;
    public static DatabaseMgr Inst
    {
        get { return _inst; }
    }

    public static void Init()
    {
        if (_inst)
        {
            return;
        }
        GameObject go = new GameObject("DatabaseMgr");
        go.AddComponent<DatabaseMgr>();
    }
    #endregion

    private Dictionary<string, Dictionary<string, object>> _cacheData;//���Ȼ��������
    private Dictionary<string, Dictionary<string, object>> _tmpData;//������ȡ������

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
    /// ���Ȼ���һЩ���ݣ���ֹ�ڵ�һ�λ�ȡ������ʱ���п���
    /// </summary>
    void CacahData()
    {
        //�����Ҫ���ȼ��ص�ί��
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

    #region ��Դ·��
    void LoadUIResPathData(Dictionary<string, Dictionary<string, object>> dic)
    {
        string tableName = typeof(UIResPathData).Name;
        Dictionary<string, object>  resTable = new Dictionary<string, object>();
        UIResPathData data = ResMgr.Inst.GetData<UIResPathData>();
        for (int i = 0; i < data._properties.Count; i++)
        {
            UIResPathProperty property = data._properties[i];
            string key = string.Format("{0}_{1}", property._resName, property._tagName);
            resTable[key] = property;
        }
        dic[tableName] = resTable;
    }

    public UIResPathProperty GetUIResPathProperty(string fileName)
    {
        return GetUIResPathProperty(fileName, "");
    }

    public UIResPathProperty GetUIResPathProperty(string resName, string tagName)
    {

        Dictionary<string, object> resTable = GetDic(typeof(UIResPathData), LoadUIResPathData);
        object result;
        resTable.TryGetValue(string.Format("{0}_{1}", resName, tagName), out result);
        if (result==null)
        {
            Debug.LogErrorFormat("��ȡ�ļ�ʧ�ܣ�resName = {0}, tagName = {1}", resName, tagName);
            return null;
        }
        return result as UIResPathProperty;
    }
#endregion

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
    }



}

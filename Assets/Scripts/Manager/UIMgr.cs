using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using LuaInterface;

public class UIMgr : MonoBehaviour
{
    #region 初始化
    private static UIMgr _inst;
    public static UIMgr Inst
    {
        get { return _inst; }
    }
    public static void Init()
    {
        if (_inst)
        {
            return;
        }
        GameObject go = new GameObject("UIMgr");
        go.AddComponent<UIMgr>();
    }
    #endregion


    private Transform _rootCanvas;

    void Awake()
    {
        _inst = this;
        DontDestroyOnLoad(gameObject);

        GameObject go = GameObject.FindWithTag("RootCanvas");
        if (go != null) _rootCanvas = go.transform;
        else Debug.LogError("没有找到RootCanvas!!");
    }

    void Destroy()
    {
        _rootCanvas = null;
    }

    /// <summary>
    /// 显示一个面板
    /// </summary>
    /// <param name="type"></param>
    public void CreatePanel(string name, LuaFunction func = null)
    {
        GameObject go = Instantiate(ResMgr.Inst.GetPrefab(name));
        go.transform.SetParent(_rootCanvas,false);
        go.transform.localScale = Vector3.one;
        go.transform.localPosition = Vector3.zero;


        //        string assetName = name + "Panel";
        //        string abName = name.ToLower() + AppConst.ExtName;
        //        if (Parent.Find(name) != null) return;
        //
        //#if ASYNC_MODE
        //        //            ResMgr.Inst.LoadPrefab(abName, assetName, delegate(UnityEngine.Object[] objs) {
        //        //                if (objs.Length == 0) return;
        //        //                GameObject prefab = objs[0] as GameObject;
        //        //                if (prefab == null) return;
        //        //
        //        //                GameObject go = Instantiate(prefab) as GameObject;
        //        //                go.name = assetName;
        //        //                go.layer = LayerMask.NameToLayer("Default");
        //        //                go.transform.SetParent(Parent);
        //        //                go.transform.localScale = Vector3.one;
        //        //                go.transform.localPosition = Vector3.zero;
        //        //                go.AddComponent<LuaBehaviour>();
        //        //
        //        //                if (func != null) func.Call(go);
        //        //                Debug.LogWarning("CreatePanel::>> " + name + " " + prefab);
        //        //            });
        //#else
        //            GameObject prefab = ResManager.LoadAsset<GameObject>(name, assetName);
        //            if (prefab == null) return;
        //
        //            GameObject go = Instantiate(prefab) as GameObject;
        //            go.name = assetName;
        //            go.layer = LayerMask.NameToLayer("Default");
        //            go.transform.SetParent(Parent);
        //            go.transform.localScale = Vector3.one;
        //            go.transform.localPosition = Vector3.zero;
        //            go.AddComponent<LuaBehaviour>();
        //
        //            if (func != null) func.Call(go);
        //            Debug.LogWarning("CreatePanel::>> " + name + " " + prefab);
        //#endif
    }

    /// <summary>
    /// 关闭面板
    /// </summary>
    /// <param name="name"></param>
    public void ClosePanel(string name)
    {
        //var panelName = name + "Panel";
        //var panelObj = Parent.Find(panelName);
        //if (panelObj == null) return;
        //Destroy(panelObj.gameObject);
    }
}

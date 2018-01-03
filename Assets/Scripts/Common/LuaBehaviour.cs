using UnityEngine;
using LuaInterface;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class LuaBehaviour : MonoBehaviour
{
    private string data = null;
    private Dictionary<string, LuaFunction> buttons = new Dictionary<string, LuaFunction>();

    void Awake()
    {
        MyUtils.CallMethod(name, "Awake", gameObject);
    }

    void Start()
    {
        MyUtils.CallMethod(name, "Start");
    }

    void OnClick()
    {
        MyUtils.CallMethod(name, "OnClick");
    }

    void OnClickEvent(GameObject go)
    {
        MyUtils.CallMethod(name, "OnClick", go);
    }

    /// <summary>
    /// 添加单击事件
    /// </summary>
    public void AddClick(GameObject go, LuaFunction luafunc)
    {
        if (go == null || luafunc == null) return;
        buttons.Add(go.name, luafunc);
        go.GetComponent<Button>().onClick.AddListener(
            delegate ()
            {
                luafunc.Call(go);
            }
        );
    }

    /// <summary>
    /// 删除单击事件
    /// </summary>
    /// <param name="go"></param>
    public void RemoveClick(GameObject go)
    {
        if (go == null) return;
        LuaFunction luafunc = null;
        if (buttons.TryGetValue(go.name, out luafunc))
        {
            luafunc.Dispose();
            luafunc = null;
            buttons.Remove(go.name);
        }
    }

    /// <summary>
    /// 清除单击事件
    /// </summary>
    public void ClearClick()
    {
        foreach (var de in buttons)
        {
            if (de.Value != null)
            {
                de.Value.Dispose();
            }
        }
        buttons.Clear();
    }

    //-----------------------------------------------------------------
    void OnDestroy()
    {
        ClearClick();
        string abName = name.ToLower().Replace("panel", "");
        ResMgr.Inst.UnloadAB(abName + AppConst.ExtName);
        MyUtils.ClearMemory();
        Debug.Log("~" + name + " was destroy!");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum RedViewType
{
    Dot,
    Num
}

public class RedPointer : MonoBehaviour
{
    public int id; //为0时，为动态红点
    public RedViewType type;
    public GameObject viewGo;
    public Text numTxt;

    void Awake()
    {
        viewGo.SetActive(false);
        if(id > 0)
            EventMgr.Inst.RedPointEvt.AddEventListener<int>(id, OnRedValueChange);
    }

    void Start()
    {
        if (id > 0)
            OnRedValueChange(RedPointMgr.Inst.GetPointNum(id));
    }

    private void OnDestroy()
    {
        if (id > 0)
            EventMgr.Inst.RedPointEvt.RemoveEventListener<int>(id, OnRedValueChange);
    }

    void OnRedValueChange(int num)
    {
        switch (type)
        {
            case RedViewType.Dot:
                viewGo.SetActive(num > 0);
                break;
            case RedViewType.Num:
                viewGo.SetActive(num > 0);
                numTxt.text = num.ToString();
                break;
        }
    }

    /// <summary>
    /// 改变红点id
    /// </summary>
    /// <param name="newId"></param>
    public void ChangeId(int newId, bool isShow)
    {
        if (newId <= 0) return;
        if (newId == id) return;
        if(id > 0)
            EventMgr.Inst.RedPointEvt.RemoveEventListener<int>(id, OnRedValueChange);
        id = newId;
        EventMgr.Inst.RedPointEvt.AddEventListener<int>(id, OnRedValueChange);

        //显示红点数据
        if (isShow)
            OnRedValueChange(RedPointMgr.Inst.GetPointNum(id));
    }
}

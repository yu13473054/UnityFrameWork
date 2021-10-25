using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RedPointAssetCfg
{
    public string key;
    public bool isShowChild;
    [HideInInspector]
    public List<int> childs = new List<int>();
    [HideInInspector]
    public int id;
    [HideInInspector]
    public int parentId;
}

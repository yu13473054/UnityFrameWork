using System;
using UnityEngine;
using System.Collections.Generic;
using LuaInterface;

public class UIMgr : SingletonMono<UIMgr>
{
    Transform _UIRoot;
    Camera _UICamera;
    //层
    private Dictionary<int, Transform> _layers;
    // 名字查找
    Dictionary<string, UISystem> _UIListByName;
    // UI栈
    Stack<string> _uiStack;
    private List<string> _backup;
    private Dictionary<string, UIStackInfo> _stackInfoDic;
    private int _uiCount = 0;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        //初始化
        _layers = new Dictionary<int, Transform>();
        _UIListByName = new Dictionary<string, UISystem>();

        _uiStack = new Stack<string>();
        _stackInfoDic = new Dictionary<string, UIStackInfo>();
        _backup = new List<string>();

        GameObject uiRoot = GameObject.Find("UIRoot");
        if (uiRoot == null)
        {
            Debug.LogError("<UIMgr> 没有找到UIRoot!");
            return;
        }
        _UIRoot = uiRoot.transform;
        DontDestroyOnLoad(uiRoot);  //防止销毁

        Transform uicamera = _UIRoot.Find("UICamera");
        if (uicamera == null)
        {
            Debug.LogError("<UIMgr> 没有找到UICamera!");
            return;
        }
        _UICamera = uicamera.GetComponent<Camera>();

        string[] layerName = Enum.GetNames(typeof(UILayer));
        for (int i = 0; i < layerName.Length; i++)
        {
            string goName = "Layer_" + i + "_" + layerName[i];
            Transform layer = _UIRoot.Find(goName);
            if (layer == null)
            {
                Debug.LogError("<UIMgr> 没有找到Layer:" + goName);
                return;
            }
            _layers[(int)Enum.Parse(typeof(UILayer), layerName[i])] = layer;
        }

        //创建对象池
        PoolMgr.Inst.CreateObjPool<UIStackInfo>(null, (obj) =>
        {
            obj.indexStack.Clear();
            obj.halfDicByIndex.Clear();
        });
    }

    /// <summary>
    /// 界面显示
    /// </summary>
    /// <param name="uiName">待显示的界面的名称：需要保证传入的uiName的一致性</param> 
    /// <returns></returns>
    public UISystem Open(string uiName)
    {
        UISystem uiSystem = null;
        if (!_UIListByName.TryGetValue(uiName, out uiSystem))
        {
            //加载新的UI
            uiSystem = LoadUISystem(uiName);
            //管理UI
            _UIListByName.Add(uiName, uiSystem);
        }

        OnOpen(uiSystem);

        uiSystem.gameObject.SetActive(true);
        uiSystem.transform.SetAsLastSibling(); // 移至队列最后
        return uiSystem;
    }

    //负责显示UI
    void OnOpen(UISystem uiSystem)
    {
        //处理堆栈逻辑
        switch (uiSystem.stackLevel)
        {
            case StackLevel.AUTO:
                string oldUIName = null;
                if (_uiStack.Count > 0)
                    oldUIName = _uiStack.Peek();

                if(uiSystem.uiName.Equals(oldUIName)) //打开已经销毁的栈顶界面会出现这种情况
                    break;

                UIStackInfo stackInfo;
                if (!string.IsNullOrEmpty(oldUIName))
                {
                    //关闭上一个Auto界面和相关的half界面
                    UISystem oldSys = _UIListByName[oldUIName];
                    StackRecord(oldSys);
                    oldSys.gameObject.SetActive(false);
                    stackInfo = _stackInfoDic[oldUIName];
                    List<string> halfList;
                    if (stackInfo.halfDicByIndex.TryGetValue(oldSys.stackIndex, out halfList))
                    {
                        for (int i = 0; i < halfList.Count; i++)
                        {
                            UISystem halfSys = _UIListByName[halfList[i]];
                            StackRecord(halfSys);
                            halfSys.gameObject.SetActive(false);
                        }
                    }
                }
                RecordStackID(uiSystem);
                //记录当前堆栈界面
                _uiStack.Push(uiSystem.uiName);
                if (!_stackInfoDic.ContainsKey(uiSystem.uiName))
                {
                    stackInfo = PoolMgr.Inst.SpawnObj<UIStackInfo>();
                    _stackInfoDic.Add(uiSystem.uiName, stackInfo);
                }
                break;
            case StackLevel.HALF:
                if (_uiStack.Count == 0)
                {
                    Debug.LogErrorFormat("<UIMgr> 无法直接打开Half界面，必须要有一个已开启的Auto界面！");
                    return;
                }
                string uiName = _uiStack.Peek();
                UISystem autoSys = _UIListByName[uiName];
                if (!autoSys.gameObject.activeSelf)
                {
                    Debug.LogErrorFormat("<UIMgr> 无法直接打开Half界面，必须要有一个已开启的Auto界面！");
                    return;
                }
                //注意：重新打开已经销毁的half界面，stackIndex会被修改掉
                stackInfo = _stackInfoDic[uiName];
                stackInfo.AddHalfUI(autoSys.stackIndex, uiSystem.uiName);
                RecordStackID(uiSystem);
                break;
            case StackLevel.Manual:
                break;
        }

        //调用界面的Open
        UIMod[] childrens = uiSystem.GetComponentsInChildren<UIMod>();
        for (int i = 0; i < childrens.Length; i++)
        {
            childrens[i].Open();
        }
    }

    public void Close(UISystem uiSystem)
    {
        switch (uiSystem.stackLevel)
        {
            case StackLevel.AUTO: //打开上一个auto界面
                string topUIName = _uiStack.Pop();
                if (topUIName != uiSystem.uiName)
                {
                    Debug.LogErrorFormat("<UIMgr> {0}界面不在栈顶，当前栈顶界面为{1}", uiSystem.uiName, topUIName);
                    return;
                }
                RemoveStackInfo(topUIName);
                RevertTopUI();
                break;
            case StackLevel.HALF:  //将half界面从Auto界面中移除掉
                topUIName = _uiStack.Peek();
                UIStackInfo info = _stackInfoDic[topUIName];
                info.RemoveHalfUI(_UIListByName[topUIName].stackIndex, uiSystem.uiName);
                RemoveStackInfo(uiSystem.uiName);
                break;
            case StackLevel.Manual:
                break;
        }

        OnClose(uiSystem);
    }

    void OnClose(UISystem uiSystem)
    {
        //调用界面的Close
        UIMod[] childrens = uiSystem.GetComponentsInChildren<UIMod>();
        for (int i = 0; i < childrens.Length; i++)
        {
            childrens[i].Close();
        }
        uiSystem.gameObject.SetActive(false);
        if (uiSystem.uiState == UIState.DESTROYONCLOSE)
        {
            UnloadUI(uiSystem);
        }
    }

    void StackRecord(UISystem uiSystem)
    {
        uiSystem.StackRecordUI();//lua层需要备份数据
        UIStackInfo stackInfo;
        if (!_stackInfoDic.TryGetValue(uiSystem.uiName, out stackInfo))
        {
            stackInfo = PoolMgr.Inst.SpawnObj<UIStackInfo>();
            _stackInfoDic.Add(uiSystem.uiName, stackInfo);
        }
        stackInfo.PushIndex(uiSystem.stackIndex);
    }

    private void RecordStackID(UISystem uiSystem)
    {
        _uiCount++;
        uiSystem.stackIndex = _uiCount;
    }

    /// <summary>
    /// 将uiName(包含) 之前的所有UI进行备份
    /// </summary>
    /// <param name="isFirst"> 是否是第一个出现的位置 </param>
    public void StackBackup(string uiName, bool isFirst = true, bool isClear = true)
    {
        _backup.Clear();

        List<string> list = new List<string>(_uiStack);
        int index;
        if (isFirst)
        {
            index = list.LastIndexOf(uiName);
        }
        else
        {
            index = list.IndexOf(uiName);
        }

        for (int i = 0; i < list.Count; i++)
        {
            string removeUIName = list[i];
            if (i < index) //不保存的界面，移除缓存数据
            {
                //栈顶界面，还没有缓存index
                RemoveUIOnBackup(removeUIName, i != 0);
            }
            else
            {
                _backup.Add(removeUIName);
            }
        }

        if (isClear)
            _uiStack.Clear();
    }

    private void RemoveUIOnBackup(string uiName, bool isRecordIndex)
    {
        UISystem uiSystem = _UIListByName[uiName];
        UIStackInfo info = _stackInfoDic[uiName];
        switch (uiSystem.stackLevel)
        {
            case StackLevel.AUTO:
                int index;
                if (isRecordIndex)
                    index = info.indexStack.Pop();
                else
                    index = uiSystem.stackIndex;
                RemoveStackInfo(uiName);
                //移除该Index下的half界面
                List<string> halfList;
                if (info.halfDicByIndex.TryGetValue(index, out halfList))
                {
                    for (int i = 0; i < halfList.Count; i++)
                    {
                        RemoveUIOnBackup(halfList[i], isRecordIndex);
                    }
                    info.halfDicByIndex.Remove(index);
                }
                break;
            case StackLevel.HALF:
                if (isRecordIndex)
                    info.indexStack.Pop(); //移除index
                RemoveStackInfo(uiName);
                break;
        }
    }

    //将ui堆栈还原成备份的状态
    public void RevertBackup()
    {
        _uiStack.Clear();
        for (int i = _backup.Count - 1; i >= 0; i--)
        {
            _uiStack.Push(_backup[i]);
        }
        _backup.Clear();
    }

    //打开栈顶UI
    public void RevertTopUI()
    {
        if (_uiStack.Count == 0) return; //没有UI了
        string topUIName = _uiStack.Peek();
        OnRevert(topUIName);
    }
    private void OnRevert(string uiName)
    {
        bool needOpen = false;
        UISystem uiSystem = null;
        if (!_UIListByName.TryGetValue(uiName, out uiSystem))  //加载UI
        {
            uiSystem = LoadUISystem(uiName);
            _UIListByName.Add(uiName, uiSystem);
            //先把界面隐藏，之后需要在lua层重新走一遍Open流程
            uiSystem.gameObject.SetActive(false);
            needOpen = true;
        }
        else
        {
            uiSystem.gameObject.SetActive(true);
            uiSystem.transform.SetAsLastSibling(); // 移至队列最后
        }

        //处理堆栈逻辑
        switch (uiSystem.stackLevel)
        {
            case StackLevel.AUTO:
            case StackLevel.HALF:
                UIStackInfo info;
                if (_stackInfoDic.TryGetValue(uiSystem.uiName, out info))
                {
                    int index = info.indexStack.Pop();
                    uiSystem.stackIndex = index; //修改为正确的数据索引
                    uiSystem.StackRevertUI(needOpen); //通知lua层
                    //打开half界面
                    List<string> halfList;
                    if (info.halfDicByIndex.TryGetValue(index, out halfList))
                    {
                        halfList = new List<string>(halfList); //复制一份，防止打开销毁的half界面时，list错乱
                        for (int i = 0; i < halfList.Count; i++)
                        {
                            OnRevert(halfList[i]);
                        }
                    }
                }
                break;
            case StackLevel.Manual:
                break;
        }
    }

    /// <summary>
    /// 从UI的管理列表中删除对应的UI
    /// </summary>
    private void UnloadUI(UISystem uiSystem)
    {
        if (uiSystem == null) return;
        Destroy(uiSystem.gameObject);
        _UIListByName.Remove(uiSystem.uiName);
    }

    private void RemoveStackInfo(string uiName)
    {
        UIStackInfo info;
        if(_stackInfoDic.TryGetValue(uiName, out info) && info.indexStack.Count == 0) //没有缓存的数据了
        {
            PoolMgr.Inst.DespawnObj(info);
            _stackInfoDic.Remove(uiName);
        }
    }

    //获取最上一层能受返回方法控制的界面
    public GameObject PopBackDlg()
    {
        for (int i = _UIRoot.childCount - 1; i >= 0; i--)
        {
            Transform layerTrans = _UIRoot.GetChild(i);
            for (int j = layerTrans.childCount - 1; j >= 0; j--)
            {
                Transform dlgTrans = layerTrans.GetChild(j);
                if (!dlgTrans.gameObject.activeSelf) continue; //未显示
                UISystem uiSystem;
                if (!_UIListByName.TryGetValue(dlgTrans.name, out uiSystem)) continue;
                if (uiSystem.backOpt == BackOpt.Ignore) continue; //不受返回键影响
                if (uiSystem.backOpt == BackOpt.Block) return null;
                return dlgTrans.gameObject;
            }
        }
        return null;
    }

    UISystem LoadUISystem(string uiName)
    {
        GameObject obj = ResMgr.Inst.LoadAsset<GameObject>(uiName, 2, uiName);
        if (obj == null)
        {
            Debug.LogError("<UIMgr> 没有找到UI:" + uiName);
            return null;
        }
        GameObject go = Instantiate(obj);
        UISystem uiSystem = go.GetComponent<UISystem>();
        if (uiSystem == null)
        {
            Destroy(go);
            Debug.LogError("<UIMgr> " + uiName + "is not a UI!");
            return null;
        }
        if (!uiName.Equals(uiSystem.uiName))
        {
            Destroy(go);
            Debug.LogErrorFormat("<UIMgr> UI名称不一致，需要打开的是{0}，实际得到的是：{1}", uiSystem.uiName, uiName);
            return null;
        }
        go.name = uiName;
        go.transform.SetParent(_layers[(int)uiSystem.layer], false);
        go.transform.localScale = Vector3.one;
        go.transform.localPosition = Vector3.zero;
        return uiSystem;
    }

    /// <summary>
    /// 将场景中的世界坐标转换成UI坐标
    /// </summary>
    /// <param name="worldPos"></param> 世界坐标
    /// <param name="worldCam"></param> 当前世界坐标对应的相机
    /// <returns></returns>
    public Vector3 World2ScreenPos(Vector3 worldPos, Camera worldCam)
    {
        Vector3 screenPos = worldCam.WorldToScreenPoint(worldPos);
        Vector3 uiPos = _UICamera.ScreenToWorldPoint(screenPos);
        return uiPos;
    }

    // UI 根节点
    public Transform GetUIRoot()
    {
        return _UIRoot;
    }

    // UI摄影机
    public Camera GetUICamera()
    {
        return _UICamera;
    }

    // 获得对应层
    public Transform GetLayer(int layerValue)
    {
        return GetLayer((UILayer)layerValue);
    }

    public Transform GetLayer(UILayer layer)
    {
        int key = (int)layer;
        if (!_layers.ContainsKey(key))
        {
            return null;
        }
        return _layers[key];
    }

    // 清空UI
    public void UnloadAllUI()
    {
        foreach (var pair in _UIListByName)
        {
            UISystem uiSystem = pair.Value;
            if (uiSystem != null)
            {
                Destroy(uiSystem.gameObject);
            }
        }
        _UIListByName.Clear();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        UnloadAllUI();
    }
}

// 单个UI界面的堆栈信息
public class UIStackInfo
{
    public Stack<int> indexStack = new Stack<int>();
    public Dictionary<int, List<string>> halfDicByIndex = new Dictionary<int, List<string>>();

    public void AddHalfUI(int index, string uiName)
    {
        List<string> list;
        if (!halfDicByIndex.TryGetValue(index, out list))
        {
            list = new List<string>();
            halfDicByIndex.Add(index, list);
        }
        list.Remove(uiName);
        list.Add(uiName);
    }

    public void RemoveHalfUI(int index, string uiName)
    {
        List<string> list = halfDicByIndex[index];
        list.Remove(uiName);
        if (list.Count == 0)
        {
            halfDicByIndex.Remove(index);
        }
    }

    public void PushIndex(int index)
    {
        indexStack.Push(index);
    }

}

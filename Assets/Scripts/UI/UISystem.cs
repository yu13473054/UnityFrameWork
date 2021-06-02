using System.Collections.Generic;
using LuaInterface;
using UnityEngine;

public enum UILayer
{
    FULL = 0,
    POP = 1,
    TOP = 2,
}

public enum UIState
{
    NORMAL,
    DONTDESTROY,
    DESTROYONCLOSE,
}

public enum StackLevel
{
    AUTO,   // 自动级，开启时会被压入主栈中，一般全屏界面为Auto级
    HALF ,   // 半自动级，不会压入主栈中，会压入当前自动级界面的独立栈中。多数半透界面
    Manual ,   // 手动，不归任何堆栈管理，DlgWait、DlgMsg、DlgMask
}

public enum BackOpt
{
    NORMAL = 0,   // 正常
    Block = 1,   // 阻塞：界面存在时，返回按钮无效
    Ignore = 2,   // 忽略
}

public class UISystem : UIMod
{
    public UILayer layer = UILayer.POP;
    public UIState uiState = UIState.NORMAL;
    public StackLevel stackLevel = StackLevel.AUTO;
    [Label("是否受返回键控制")]
    public BackOpt backOpt = BackOpt.NORMAL;
    [HideInInspector]
    public int stackIndex; //栈中数据的索引：用于恢复界面取具体数据用

    // 事件函数
    protected LuaFunction _recordUI;
    protected LuaFunction _revertUI;

    protected override void Awake()
    {
        base.Awake();
        _recordUI = LuaMgr.Inst.GetFunction(uiName + ".StackRecordUI", false);
        _revertUI = LuaMgr.Inst.GetFunction(uiName + ".StackRevertUI", false);
    }

    /// <summary>
    /// 通知Lua层，备份界面相关数据
    /// </summary>
    /// <returns> 成功备份数据后，返回true </returns>
    public void StackRecordUI()
    {
        if (_recordUI != null)
        {
            _recordUI.Call(stackIndex);
        }
    }
    public void StackRevertUI(bool needOpen)
    {
        if (_revertUI != null)
        {
            _revertUI.Call(stackIndex, needOpen);
        }
    }
}

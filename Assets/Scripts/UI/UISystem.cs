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

public class UISystem : UIMod
{
    public UILayer layer = UILayer.POP;
    public UIState uiState = UIState.NORMAL;

    protected override void Awake()
    {
        base.Awake();
        resModule = uiName; //UISystem中的模块名自动指定为界面名
    }
}

using UnityEngine;
using LuaInterface;

// 需要传递事件到lua的Mod挂载。不需lua的Mod无需挂载。有cs的并需要调用lua的Mod继承此类。
// UISystem用于对话框，继承UIMod的消息处理模式！

public class UIEVENT
{
	public const int UIBUTTON_CLICK = 10;                // UIButton单击					无参
    public const int UIBUTTON_PRESS = 11;		         // UIButton按下					0 按下，1 抬起

	public const int UITOGGLE_CLICK = 22;                // UIToggle单击					无参
	public const int UITOGGLE_PRESS = 23;		         // UIToggle按下					0 按下，1 抬起
    public const int UITOGGLE_ONVALUECHANGE = 21;	     // UIToggle内容发生变化时       bool值

    public const int UISLIDER_DRAG = 31;                 // UISlider拖动                0 开始拖动，1 拖动中，2 结束拖动
    public const int UISLIDER_PRESS = 34;                // UISlider按下				    0 按下，1 抬起

    public const int UITIMER_TIMERUNOUT = 39;            // UITimer计时结束

    public const int UISCROLLVIEW_DRAG = 51;		     // UIScrollView拖动             0 开始拖动，1 拖动中，2 结束拖动
    public const int UISCROLLVIEW_ONVALUECHANGE = 52;	 // UIScrollView内容发生变化时    Vector2对象
    public const int WRAPCONTENT_ONITEMUPDATE = 53;	     // WrapContent中Item更新        自定义对象：index，Transform
    public const int WRAPCONTENT_ONINITDONE = 54;	     // WrapCoNtent中初始化完成       无
    public const int UISCROLLVIEW_ALIGNTOFINISH = 56;	 // ScrollView对齐完成            对齐过程停止时的itemIndex

    public const int UIINPUT_SUBMIT = 61;                // 完成修改
    public const int UIINPUT_VALUECHANGED = 62;          // 值改变

    public const int UISCROLLBAR_ONVALUECHANGE = 71;	 // UIScrollbar内容发生变化时     float值
    public const int UISCROLLBAR_PRESS = 72;             // UIScrollbar按下				 0 按下，1 抬起

    public const int UIRICHREXT_CLICK = 81;              // UI富文本点击

}

public enum OpenState
{
    Init,
    needOpen,
    Open,
    needClose,
    Close,
}

[RequireComponent(typeof(UIItem))]
[RequireComponent(typeof(ResModuleUtility))]
public class UIMod : MonoBehaviour 
{
    public string uiName = "";
    [HideInInspector]
    public ResModuleUtility resUtility;
    // 事件函数
	protected LuaFunction _onEvent;

    protected OpenState _openState;

    private static LuaFunction _uiHelperOpen;
    private static LuaFunction _uiHelperClose;
    private static LuaFunction _uiHelperDestroy;

    protected virtual void Awake()
	{
		// 找到对应脚本的函数 事件函数命名为：obj名字加事件名
	    if (uiName == "")
	        uiName = gameObject.name.Replace("(Clone)", "").TrimEnd();

	    if (resUtility == null)
	        resUtility = GetComponent<ResModuleUtility>();

        _onEvent = LuaMgr.Inst.GetFunction( uiName + ".OnEvent" );

	    if (_uiHelperOpen == null)
	    {
	        _uiHelperOpen = LuaMgr.Inst.GetFunction("UIHelper.OnOpen");
	        _uiHelperClose = LuaMgr.Inst.GetFunction("UIHelper.OnClose");
	        _uiHelperDestroy = LuaMgr.Inst.GetFunction("UIHelper.OnDestroy");
	    }

        LuaFunction func = LuaMgr.Inst.GetFunction( uiName + ".OnAwake");
		if(func != null )
            func.Call( gameObject );

	    _openState = OpenState.Init;
    }

	protected virtual void Start()
	{
	    if (_openState == OpenState.needOpen)
            OnOpen();
    }

	protected virtual void OnEnable()
	{
        if (_openState == OpenState.needOpen)
		    OnOpen();
	}

	protected virtual void OnDisable()
	{
        if(_openState == OpenState.needClose)
            OnClose();
	}

	protected virtual void OnDestroy()
	{
        OnClose(); //如果逻辑上没有关闭过，在销毁的时候调用

	    if (_uiHelperDestroy != null)
	        _uiHelperDestroy.Call(uiName, gameObject);

    }

    /// <summary>
    /// 逻辑上的开启，当UI第一次加载，会在Start后调用。否则在OnEnable后调用
    /// Lua层，可以在该方法中进行界面的初始化
    /// </summary>
    protected virtual void OnOpen()
    {
        if (_openState == OpenState.Open) return; 

        _openState = OpenState.Open;
        if (_uiHelperOpen != null)
            _uiHelperOpen.Call(uiName, gameObject);
    }

    /// <summary>
    /// 逻辑上的关闭，某些界面可能会隐藏了，但是并没有调用过Close
    /// Lua层，可以在该界面中，进行界面的资源回收、监听移除等
    /// </summary>
    protected virtual void OnClose()
    {
        if (_openState == OpenState.Close) return;

        _openState = OpenState.Close;
        if (_uiHelperClose != null)
            _uiHelperClose.Call(uiName, gameObject);
    }

    public virtual void OnEvent( int eventID, int controlID, object value )
	{
		if( _onEvent != null )
            _onEvent.Call( eventID, controlID, value, gameObject );
	}

    public virtual void Open()
    {
        _openState = OpenState.needOpen;
    }

    public virtual void Close()
    {
        _openState = OpenState.needClose;
    }

    #region 资源获取

    public Texture GetTexture(string reskeyname, bool isTry = false)
    {
        return resUtility.LoadTexture(reskeyname, isTry);
    }

    public UnityEngine.Object GetObject(string reskeyname, bool isTry = false)
    {
        return resUtility.LoadObject(reskeyname, isTry);
    }

    public Sprite GetSprite(string resName, bool isTry = false)
    {
        return resUtility.LoadSprite(resName, isTry);
    }

    public GameObject GetPrefab(string resName, bool instantiate = false, bool isTry = false)
    {
        return resUtility.LoadPrefab(resName, instantiate, isTry);
    }

    public Material GetMaterial(string resName, bool isTry = false)
    {
        return resUtility.LoadMaterial(resName, isTry);
    }
    #endregion
}

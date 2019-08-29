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

public class UIMod : MonoBehaviour 
{
	public GameObject[] relatedGameObject;
    public string uiName = "";
    public string resModule = "";
	// 事件函数
	protected LuaFunction _onOpen;
	protected LuaFunction _onClose;
	protected LuaFunction _onEvent;

    private bool _isFirst = true;

    public bool isFirst
    {
        get
        {
            return _isFirst;
        }
    }

	protected virtual void Awake ()
	{
		// 找到对应脚本的函数 事件函数命名为：obj名字加事件名
	    if (uiName == "")
	        uiName = gameObject.name.Replace("(Clone)", "").TrimEnd();

        _onEvent = LuaMgr.Inst.GetFunction( uiName + ".OnEvent" );
		_onOpen = LuaMgr.Inst.GetFunction( uiName + ".OnOpen" );
		_onClose = LuaMgr.Inst.GetFunction( uiName + ".OnClose" );

        LuaFunction onInit = LuaMgr.Inst.GetFunction( uiName + ".OnInit" );
		if(onInit != null )
		{
            onInit.Call( gameObject );
		}
	}

	protected virtual void Start()
	{
        if (_onOpen != null)
            _onOpen.Call(gameObject);
	    _isFirst = false;
    }

	protected virtual void OnEnable()
	{
	    if (!_isFirst)
	    {
		    if( _onOpen != null )
			    _onOpen.Call( gameObject );
	    }
	}

	protected virtual void OnDisable()
	{
		if( _onClose != null ) 
			_onClose.Call( gameObject );
	}

	protected virtual void OnDestroy()
	{
	    _isFirst = true;
        LuaFunction onDestroy = LuaMgr.Inst.GetFunction( uiName + ".OnDestroy" );
        if (onDestroy != null )
            onDestroy.Call( gameObject );
	}

	public virtual void OnEvent( int eventID, int controlID, object value )
	{
		if( _onEvent != null )
            _onEvent.Call<int,int,object,GameObject>( eventID, controlID, value, gameObject );
	}

    public Sprite GetSprite(string resName)
    {
        return ResMgr.Inst.LoadSprite(resName, resModule);
    }

    public GameObject GetPrefab(string resName, bool dontInst = false)
    {
        if (dontInst)
            return ResMgr.Inst.LoadPrefab(resName, resModule);
        else
            return Instantiate(ResMgr.Inst.LoadPrefab(resName, resModule));
    }

    public Material GetMaterial(string resName)
    {
        return ResMgr.Inst.LoadMaterial(resName, resModule);
    }
}

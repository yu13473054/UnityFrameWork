using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**  适配方式枚举 **/
public enum AdaptationMethod
{
    SizeDelta = 0,
    Scale = 1, //适用于非全屏时的适配
    InSizeDelta = 2, // 最大边显示满
}

[ExecuteInEditMode]
public class FullScreen : MonoBehaviour
{
    // 枚举方式
    [SerializeField]
    public AdaptationMethod _method = AdaptationMethod.SizeDelta;
    [SerializeField]
    private bool _keepAspect = true;
    //在设计分辨率下的显示大小
    [SerializeField]
    private int _showWidth = 1600;
    [SerializeField]
    private int _showHeight = 900;
    [SerializeField]
    private int _offsetWidth = 0;
    [SerializeField]
    private int _offsetHeight = 0;
    [SerializeField]
    private bool _manualSet = false;
    //手动设置的设计分辨率：如果没有手动设置，则默认初始显示大小就为设计分辨率
    [SerializeField]
    private int _manualWidth = 1;
    [SerializeField]
    private int _manualHeight = 1;

    private RectTransform _rootTrans;
    private Vector2 _currScreenSize;

    void Start()
    {
#if UNITY_EDITOR
        _rootTrans = Application.isPlaying && UIMgr.Inst ? (RectTransform)UIMgr.Inst.GetUIRoot() : GameObject.Find("UIRoot").transform as RectTransform;
#else
        _rootTrans = (RectTransform) UIMgr.Inst.GetUIRoot();
#endif
        Adjust();
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Application.isPlaying) return;
        Adjust();
    }
#endif

    private void Adjust()
    {
        //获取RootCanvas的SizeDelta，这个是实际上的分辨率
        Vector2 screenSize = _rootTrans.sizeDelta;
        if (_currScreenSize != screenSize)
        {
            _currScreenSize = screenSize;
            if (_method == AdaptationMethod.Scale)
                Adjust_Scale();
            else if (_method == AdaptationMethod.SizeDelta)
                Adjust_SizeDelta();
            else if (_method == AdaptationMethod.InSizeDelta)
                Adjust_InSizeDelta();
        }
    }

    public void Adjust_SizeDelta()
    {
        if (_keepAspect)
        {
            //获取显示大小
            Vector2 showSize;
            //获取设计大小
            Vector2 designSize;

            if (_manualSet)
            {
                designSize = new Vector2(_manualWidth, _manualHeight);
                showSize = new Vector2(_showWidth, _showHeight);
            }
            else
            {
                designSize = showSize = new Vector2(_showWidth, _showHeight);
            }

            //获取缩放比例
            float scale_w = _currScreenSize.x / designSize.x;
            float scale_h = _currScreenSize.y / designSize.y;
            float scaleFactor = Mathf.Max(scale_w, scale_h);
            showSize *= scaleFactor;
            showSize.x += _offsetWidth;
            showSize.y += _offsetHeight;

            ((RectTransform)transform).sizeDelta = showSize;
        }
        else
        {
            ((RectTransform)transform).sizeDelta = _currScreenSize;
        }
    }

    public void Adjust_InSizeDelta()
    {
        if (_keepAspect)
        {
            //获取显示大小
            Vector2 showSize;
            //获取设计大小
            Vector2 designSize;

            if (_manualSet)
            {
                designSize = new Vector2(_manualWidth, _manualHeight);
                showSize = new Vector2(_showWidth, _showHeight);
            }
            else
            {
                designSize = showSize = new Vector2(_showWidth, _showHeight);
            }

            //获取缩放比例
            float scale_w = _currScreenSize.x / designSize.x;
            float scale_h = _currScreenSize.y / designSize.y;
            float scaleFactor = Mathf.Min( scale_w, scale_h );
            showSize *= scaleFactor;
            showSize.x += _offsetWidth;
            showSize.y += _offsetHeight;

            ((RectTransform)transform).sizeDelta = showSize;
        }
        else
        {
            ((RectTransform)transform).sizeDelta = _currScreenSize;
        }
    }

    public void Adjust_Scale()
    {
        //获取设计大小
        Vector2 designSize;
        if (_manualSet)
        {
            designSize = new Vector2(_manualWidth, _manualHeight);
        }
        else
        {
            designSize = new Vector2(_showWidth, _showHeight);
        }
        //获取缩放比例
        float scale_w = _currScreenSize.x / designSize.x;
        float scale_h = _currScreenSize.y / designSize.y;
        if (_keepAspect)
        {
            float scaleFactor = Mathf.Max(scale_w, scale_h);
            transform.localScale = scaleFactor * Vector3.one;
        }
        else
        {
            transform.localScale = new Vector3(scale_w, scale_h, 1);
        }
    }

    public void SetSize( int w, int h )
    {
        _showWidth  = w;
        _showHeight = h;

        // 重新设置尺寸
        if ( _method == AdaptationMethod.Scale )
            Adjust_Scale();
        else if ( _method == AdaptationMethod.SizeDelta )
            Adjust_SizeDelta();
        else if ( _method == AdaptationMethod.InSizeDelta )
            Adjust_InSizeDelta();
    }

}
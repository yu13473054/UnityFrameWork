using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FullScreen : MonoBehaviour
{
    public enum AdapterMethod
    {
        Size,
        Scale
    }

    public AdapterMethod method;
    [SerializeField]
    private bool _keepAspect = true;
    //在设计分辨率下的显示大小
    [SerializeField]
    private int _showWidth = 1280;
    [SerializeField]
    private int _showHeight = 720;
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

    void Start()
    {
        if (method == AdapterMethod.Size)
            Adjust_Size();
        else
            Adjust_Scale();

    }

    #if UNITY_EDITOR
    void Update()
    {
        if (Application.isPlaying) return;
        if (method == AdapterMethod.Size)
            Adjust_Size();
        else
            Adjust_Scale();
    }
    #endif
    private void Adjust_Scale()
    {
        RectTransform rectTrans = (RectTransform)transform;
        if (rectTrans != null)
        {
            //获取RootCanvas的SizeDelta，这个是实际上的分辨率
#if UNITY_EDITOR
            Vector2 screenSize = Application.isPlaying ? ((RectTransform)UIMgr.Inst.GetCanvas().transform).sizeDelta : ((RectTransform)rectTrans.GetComponentInParent<Canvas>().rootCanvas.transform).sizeDelta;
#else
            Vector2 screenSize = ((RectTransform)UIManager.instance.GetCanvas().transform).sizeDelta;
#endif
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
                float scale_w = screenSize.x / designSize.x;
                float scale_h = screenSize.y / designSize.y;
                rectTrans.transform.localScale = scale_h * scale_w * Vector2.one;
            }
        }
    }

    public void Adjust_Size()
    {
        RectTransform rectTrans = (RectTransform)transform;
        if (rectTrans != null)
        {
            //获取RootCanvas的SizeDelta，这个是实际上的分辨率
#if UNITY_EDITOR
            Vector2 screenSize = Application.isPlaying ? ((RectTransform)UIMgr.Inst.GetCanvas().transform).sizeDelta : ((RectTransform)rectTrans.GetComponentInParent<Canvas>().rootCanvas.transform).sizeDelta;
#else
            Vector2 screenSize = ((RectTransform)UIManager.instance.GetCanvas().transform).sizeDelta;
#endif
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
                float scale_w = screenSize.x / designSize.x;
                float scale_h = screenSize.y / designSize.y;

                //放大显示大小：设计大小只是作为一个缩放比例的参考值
                if (scale_w > scale_h) //需要按照宽度放大
                {
                    showSize.x = Mathf.RoundToInt(scale_w * showSize.x);
                    showSize.y = Mathf.RoundToInt(scale_w * showSize.y);
                }
                else //需要按照高度放大
                {
                    showSize.x = Mathf.RoundToInt(scale_h * showSize.x);
                    showSize.y = Mathf.RoundToInt(scale_h * showSize.y);
                }

                showSize.x += _offsetWidth;
                showSize.y += _offsetHeight;

                rectTrans.sizeDelta = showSize;
            }
            else
            {
                rectTrans.sizeDelta = screenSize;
            }
        }
    }
}

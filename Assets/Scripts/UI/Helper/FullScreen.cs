using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FullScreen : MonoBehaviour
{
    [SerializeField] private bool _keepAspect = true;
    //手动设置基础宽高, 否则按RectTransform的大小计算
    [SerializeField] private bool _manualSet = false;
    [SerializeField] private int _manualWidth = 1;
    [SerializeField] private int _manualHeight = 1;
    [SerializeField]
    private int _offsetWidth = 0;
    [SerializeField]
    private int _offsetHeight = 0;

//    void Awake()
//    {
//        Adjust();
//    }

    void Start()
    {
        Adjust();
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Application.isPlaying) return;
        Adjust();
    }
#endif

    public void Adjust()
    {
        RectTransform rectTrans = (RectTransform) transform;
        if (rectTrans != null)
        {
            //获取RootCanvas的SizeDelta，这个是实际上的分辨率
#if UNITY_EDITOR
            Vector2 screenSize = Application.isPlaying ? ((RectTransform)UIMgr.Inst.GetCanvas().transform).sizeDelta : ((RectTransform)rectTrans.GetComponentInParent<Canvas>().rootCanvas.transform).sizeDelta;
#else
            Vector2 screenSize = ((RectTransform)UIMgr.Inst.GetCanvas().transform).sizeDelta;
#endif
            if (_keepAspect)
            {
                //获取设计大小
                Vector2 originSize = _manualSet ? new Vector2(_manualWidth, _manualHeight) : rectTrans.sizeDelta;
                Vector2 ret = originSize;

                int tw = (int)originSize.x;
                int th = (int)originSize.y;

                int sw = (int)screenSize.x;
                int sh = (int)screenSize.y;

                float scale_w = sw / (tw * 1.0f);
                float scale_h = sh / (th * 1.0f);

                if (scale_w > scale_h)
                {
                    ret.x = sw;
                    ret.y = Mathf.CeilToInt(scale_w * th);
                }
                else
                {
                    ret.x = Mathf.CeilToInt(scale_h * tw);
                    ret.y = sh;
                }
                ret.x += _offsetWidth;
                ret.y += _offsetHeight;

                rectTrans.sizeDelta = ret;
            }
            else
            {
                rectTrans.sizeDelta = screenSize;
            }
        }
    }
}

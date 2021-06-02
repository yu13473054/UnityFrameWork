using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ RequireComponent( typeof( RectTransform ) ) ]
public class UIEdgeAdaptation : MonoBehaviour
{
    public enum AdjustMode
    {
        OneSide,        // 只对刘海测进行适配
        BothSide,       // 两边都进行适配
    }

    private static Camera   _camera;
    private static float    _scaleFactor;

    int       _currOrientation = 0;

    [Label("适配模式")]
    public AdjustMode  adjustMode = AdjustMode.OneSide;

    // 偏移值
    public float _offset;

    void Awake()
    {
        float wScale = Screen.width / 1600f;
        float hScale = Screen.height / 900f;

        // 计算出不同分辨率下，设定像素值需要缩放的比例
        _scaleFactor = wScale / hScale; 

        // 获取UI相机
        if( _camera == null && UIMgr.Inst != null )
            _camera = UIMgr.Inst.GetUICamera();
        else if( _camera == null )
            _camera = GameObject.Find( "UICamera" ).GetComponent<Camera>();
    }

    private void Start()
    {
        Adjust( true );
    }

    void Update()
    {
        Adjust( false );
    }

    private void Adjust( bool force )
    {
        int currOri = 0;
        if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft)
        {
            currOri = 1;
        }
        else if (Input.deviceOrientation == DeviceOrientation.LandscapeRight)
        {
            currOri = 2;
        }

        // 若发生改变 赋值
        if (_currOrientation != currOri || force)
        {
            _currOrientation = currOri;
        }
        else
            return;

        //Debug.Log( string.Format( "<屏幕适配>当前屏幕模式：{0}，安全区x：{1}", _currOrientation, Screen.safeArea.x ) );

        float value = GameMain.Inst.viewOffstPixel * _scaleFactor + Screen.safeArea.x;

        //将屏幕坐标系转换成UGUI坐标系中
        Vector2 outPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle( (RectTransform)UIMgr.Inst.GetUIRoot(), new Vector2( Screen.width / 2f + value, Screen.height / 2f ), _camera, out outPos );

        if( adjustMode == AdjustMode.BothSide || _currOrientation == 0 )
        {
            ( (RectTransform)this.transform ).offsetMin = new Vector2( outPos.x + _offset, outPos.y );
            ( (RectTransform)this.transform ).offsetMax = new Vector2( -outPos.x - _offset, -outPos.y );
        }
        else if ( _currOrientation == 1 )
        {
            ( (RectTransform)this.transform ).offsetMin = new Vector2( outPos.x + _offset, outPos.y );
            ( (RectTransform)this.transform ).offsetMax = new Vector2( 0, -outPos.y );
        }
        else if ( _currOrientation == 2 )
        {
            ( (RectTransform)this.transform ).offsetMin = new Vector2( 0, outPos.y );
            ( (RectTransform)this.transform ).offsetMax = new Vector2( -outPos.x - _offset, -outPos.y );
        }
    }
}

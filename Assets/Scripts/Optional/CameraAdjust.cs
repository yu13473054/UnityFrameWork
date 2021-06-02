using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum CamScaleMode
{
    Width,    //以宽度适配
    FullScale, //全屏
    Height, // 以高度
}

[RequireComponent(typeof(Camera))]
public class CameraAdjust : MonoBehaviour
{
    public int designWidth = 1280;
    public int designHeight = 720;
    public CamScaleMode mode = CamScaleMode.Width;
    public bool inverse = false;

    float _size = 0;
    Camera _camera;
    private Vector2 _currResolution;

    void Awake()
    {
        _camera = GetComponent<Camera>();
        if (_camera.orthographic)
        {
            _size = _camera.orthographicSize;
        }
        else
        {
            float dis = 10;
            _size = dis * Mathf.Tan(Mathf.Deg2Rad * _camera.fieldOfView / 2);
        }
        Calculate();
    }

    void Update()
    {
        Calculate();
    }

    void Calculate()
    {
        float scaleFactor = Screen.width * 9f / Screen.height / 16; //计算出不同分辨率下，设定像素值需要缩放的比例
        float width = Screen.safeArea.width - GameMain.Inst.viewOffstPixel * 2f * scaleFactor;
        Vector2 resolution = new Vector2(width , Screen.height);
        if (_currResolution == resolution) return;
        _currResolution = resolution;

        float wScale = _currResolution.x / designWidth;
        float hScale = _currResolution.y / designHeight;
        if (Mathf.Approximately(hScale, wScale))
            return;

        float factor = 1;
        switch (mode)
        {
            case CamScaleMode.Width:
                if( inverse )
                    factor = wScale / hScale;
                else
                    factor = hScale / wScale;
                break;
            case CamScaleMode.FullScale:
                if ( inverse )
                    factor = wScale > hScale ? 1 : hScale / wScale;
                else
                    factor = wScale > hScale ? hScale / wScale : 1;
                break;
            case CamScaleMode.Height:
                if ( inverse )
                    factor = wScale / hScale;
                else
                    factor = hScale / wScale;
                factor *= factor;
                break;
        }

        if (_camera.orthographic)
        {
            _camera.orthographicSize = _size * factor;
        }
        else
        {
            float dis = 10;
            float newSize = _size * factor;
            _camera.fieldOfView = Mathf.Atan(newSize / dis) * Mathf.Rad2Deg * 2;
        }
    }
}


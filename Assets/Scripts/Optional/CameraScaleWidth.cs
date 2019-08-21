using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 以宽度为基准，适配相。界面显示的宽度在各个分辨率上一致
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraScaleWidth : MonoBehaviour
{
    public int designWidth = 1280;
    public int designHeight = 720;

    float _size = 0;
    Camera _camera;

    void Awake()
    {
        _camera = GetComponent<Camera>();
        if (_camera.orthographic)
        {
            _size = _camera.orthographicSize;
        }
        else
        {
            float dis = 0 - _camera.transform.position.z;
            _size = dis * Mathf.Tan(Mathf.Deg2Rad * _camera.fieldOfView / 2);
        }
        Calculate();
    }

    void Calculate()
    {
        int screenW = Screen.width;
        int screenH = Screen.height;

        float wScale = screenW * 1f / designWidth;
        float hScale = screenH * 1f / designHeight;
        if (Math.Abs(wScale - hScale) < 0.0000001f)
        {
            return;
        }
        if (_camera.orthographic)
        {
            if (wScale > hScale)
            {
                _camera.orthographicSize = _size / (wScale / hScale);
            }
            else
            {
                _camera.orthographicSize = _size * (hScale / wScale);
            }
        }
        else
        {
            float dis = 0 - _camera.transform.position.z;
            float newHalfHeight = 0;
            if (wScale > hScale)
            {
                newHalfHeight = _size / (wScale / hScale);
            }
            else
            {
                newHalfHeight = _size * (hScale / wScale);
            }
            _camera.fieldOfView = Mathf.Atan(newHalfHeight / dis) * Mathf.Rad2Deg * 2;
        }
    }
}


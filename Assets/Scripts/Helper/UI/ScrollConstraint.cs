using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ScrollView的滑动扩展，用于在内容不足时，限制其不能滑动
/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class ScrollConstraint : MonoBehaviour
{
    private ScrollRect _scroll;

    private float _lastContentW, _lastContentH;

	void Start ()
	{
	    _scroll = GetComponent<ScrollRect>();
	}


    void Update()
    {
        if (_scroll.horizontal && _scroll.vertical)
        {
            
        }
        else if (_scroll.vertical)//只是竖直滚动
        {
            float currContentH = _scroll.content.rect.height;
            if (Math.Abs(currContentH - _lastContentH) > 0.0001f)
            {
                if (currContentH > _scroll.viewport.rect.height)
                {
                    _scroll.movementType = ScrollRect.MovementType.Elastic;
                }
                else
                {
                    _scroll.movementType = ScrollRect.MovementType.Clamped;
                }
                _lastContentH = currContentH;
            }
        }
        else if (_scroll.horizontal)//只是水平滚动
        {
            float currContentW = _scroll.content.rect.width;
            if (Math.Abs(currContentW - _lastContentW) > 0.0001f)
            {
                if (currContentW > _scroll.viewport.rect.width)
                {
                    _scroll.movementType = ScrollRect.MovementType.Elastic;
                }
                else
                {
                    _scroll.movementType = ScrollRect.MovementType.Clamped;
                }
                _lastContentW = currContentW;
            }
        }
    }
	
}

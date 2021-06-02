using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class UIStampTimer : Text
{
    public  UIMod uiMod;
    public  int controlID = 0;

    string  _textID;
    string  _finishTextID;

    int     _timeStampOffset;
    int     _startTimeStamp;
    int     _endTimeStamp;
    bool    _forward;

    bool    _started = false;

    protected override void Start()
    {
        base.Start();
        if ( uiMod == null )
        {
            uiMod = gameObject.GetComponentInParent<UIMod>();
        }
    }

    int GetTimeStamp()
    {
        TimeSpan ts = DateTime.Now - new DateTime( 1970, 1, 1, 0, 0, 0, 0 );
        return Convert.ToInt32( ts.TotalSeconds );
    }

    // 单位：秒
    // 结束时间戳，持续时间，倒计时方向(true+, false-)，格式化文本ID，结束时的文本ID
    public void SetTimer( int endTimeStamp, int duration, bool forward = false, string textID = null, string finishTextID = null )
    {
        // 计算起始和时间差
        _startTimeStamp = endTimeStamp - duration;
        _timeStampOffset = _startTimeStamp - GetTimeStamp();

        // 缓存
        _endTimeStamp = endTimeStamp;
        _forward = forward;
        _textID = textID;
        _finishTextID = finishTextID;

        // 开始计时
        _started = true;
    }

    //更新
    private void Update()
    {
        if( !_started )
            return;

        // 当前剩余时间
        int time = 0;
        bool finish = false;
        
        // +还是-
        if( _forward )
        {
            time = GetTimeStamp() - _startTimeStamp + _timeStampOffset;
            if (time >= _endTimeStamp - _startTimeStamp)
            {
                finish = true;
                time = _endTimeStamp - _startTimeStamp;
            }
        }
        else
        {
            time = _endTimeStamp - GetTimeStamp() - _timeStampOffset;
            if (time <= 0)
            {
                time = 0;
                finish = true;
            }
        }

        // 换算，并显示文本
        int hour = (int)( (float)time / 60 / 60 );
        int minute = (int)( (float)time / 60 % 60 );
        int second = (int)( (float)time % 60 );

        // 有格式化文本
        if( _textID != null )
        {
            this.text = Localization.Get( _textID, hour, minute, second );
        }
        // 没有
        else
        {
            this.text = string.Format( "{0:d2}:{1:d2}:{2:d2}", hour, minute, second );
        }

        // 结束
        if (finish)
        {
            _started = false;
            if (_finishTextID != null)
                this.text = Localization.Get(_finishTextID);

            if (uiMod)
                uiMod.OnEvent(UIEVENT.UITIMER_TIMERUNOUT, controlID, 0);
        }
    }

    public float GetCurrTime()
    {
        float curTime = _forward ? GetTimeStamp() - _startTimeStamp + _timeStampOffset : _endTimeStamp - GetTimeStamp() - _timeStampOffset;
        return curTime;
    }
}

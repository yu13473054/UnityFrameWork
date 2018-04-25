using System;
using System.Collections.Generic;
using UnityEngine;

// 本地化
public class Localization
{
    // 文字表
    static Dictionary<int, string> _localization;
    public static bool isInited = false;

    // 读取文字表
    public static void Init()
    {
        TableHandler localizetxt =TableHandler.Open("localization.txt", "data");

        _localization = new Dictionary<int, string>();

        for ( int row = 0; row < localizetxt.GetRecordsNum(); row++ )
        {
            int key = Convert.ToInt32( localizetxt.GetValue( row, 0 ) );
            string text = localizetxt.GetValue( row, 1 );
            text = text.Replace( "\\n", "\n" );
            _localization.Add( key, text );
        }

        isInited = true;
    }

    public static void OnDestroy()
    {
        isInited = false;
        _localization = null;
    }

    public static string Get( int id )
    {
        if (_localization == null) return "";
        if (!_localization.ContainsKey(id))
        {
            if(Application.isPlaying)
                Debug.LogErrorFormat("<Localization> 文本丢失，id = {0}", id);
            return "";
        }
        return _localization[id];
    }

    public static string SafeGet(int id, string defaultText)
    {
        string result = Get(id);
        if (string.IsNullOrEmpty(result))
            return defaultText;
        return result;
    }
}


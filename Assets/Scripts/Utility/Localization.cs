using System.Collections.Generic;
using System;
using UnityEngine;

// 本地化
public static class Localization
{
    // 文字表
    static Dictionary<string, string> _localization;
    public static bool isInited = false;

    public static void Reload()
    {
        isInited = false;
        Init();
    }

    // 读取文字表
    public static void Init()
    {
        if (isInited) return;
        isInited = true;

        _localization = new Dictionary<string, string>();
        ParserTxt("localization");
        ParserTxt("Localization");
    }

    static void ParserTxt(string fileName)
    {
        TableHandler localizetxt;
        if (!Application.isPlaying)
        {
            //编辑器模式下，获取多语言表
            string reskeyName = null;
            //获取reskeyName
            TableHandler multiLngNameTable = TableHandler.OpenFromData("multiLngConfig");
            for (int row = 0; row < multiLngNameTable.GetRecordsNum(); row++)
            {
                string key = multiLngNameTable.GetValue(row, 0);
                if (key == fileName)
                {
                    if (!GameMain.Inst || GameMain.Inst.lngType == 0)
                    {
                        reskeyName = multiLngNameTable.GetValue(row, 1);
                    }
                    else
                    {
                        for (int i = 1; i < multiLngNameTable.GetFieldsNum(); i++)
                        {
                            if (i == GameMain.Inst.lngType)
                            {
                                reskeyName = multiLngNameTable.GetValue(row, i);
                                break;
                            }
                        }
                    }
                    break;
                }
            }
            if (reskeyName == null)
            {
                Debug.LogError("<Localization> 请先生成resmap！");
                return;
            }
            //获取ab名称
            string abName = null;
            TableHandler objectTable = TableHandler.OpenFromData("resmap_obj");
            for (int row = 0; row < objectTable.GetRecordsNum(); row++)
            {
                string key = objectTable.GetValue(row, 0);
                if (key == reskeyName)
                {
                    abName = objectTable.GetValue(row, 2);
                    break;
                }
            }
            localizetxt = TableHandler.OpenInAB(fileName, abName);
        }
        else
        {
            localizetxt = TableHandler.OpenFromResmap(fileName);
        }

        for (int row = 0; row < localizetxt.GetRecordsNum(); row++)
        {
            string key = localizetxt.GetValue(row, 0);
            string text = localizetxt.GetValue(row, 1);
            text = text.Replace("\\n", "\n");
            try
            {
                _localization.Add(key, text);
            }
            catch
            {
                Debug.LogError("<文字表> 重复的文字表ID：" + key + "--" + text);
            }
        }
    }

    public static string Get( string id, params object[] args)
    {
        if (_localization == null) return "";
        if ( !_localization.ContainsKey( id ) )
        {
            if( Application.isPlaying )
                Debug.LogErrorFormat("<Localization> 文本丢失，id = {0}", id);
            return "";
        }
        string resultStr = _localization[id];
        if (args != null && args.Length > 0)
        {
            resultStr = string.Format(resultStr, args);
        }
        return resultStr;
    }
}


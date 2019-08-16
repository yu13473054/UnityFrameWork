using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
/// <summary>
/// 编辑器下的表格读取工具
/// </summary>
public class TableHandlerEditor
{
    /// 行数
    private int _recordsNum;
    /// 列数
    private int _fieldsNum;
    /// 数据区
    List<string> _dataBuf;

    private string _fileName = "";
    /// 构造
    public TableHandlerEditor(string fileName)
    {
        _recordsNum = 0;
        _fieldsNum = 0;
        _dataBuf = new List<string>();
    }

    /// <summary>
    /// 打开TXT文件
    /// </summary>
    /// 
    public static TableHandlerEditor Open(string fileName)
    {
        return Open( "Assets/Data/Data/", fileName );
    }

    public static TableHandlerEditor Open( string dir ,string fileName )
    {
        TableHandlerEditor handle = new TableHandlerEditor( fileName );

        //拼装出EditorPath
        string editorPath = dir + fileName;
        TextAsset asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(editorPath);
        if (asset == null)
        {
            Debug.LogErrorFormat("<TableHandlerEditor> {0}文件不存在！", fileName);
            return handle;
        }
        string allText = asset.text;
        //解析数据
        if (!string.IsNullOrEmpty(allText))
        {
            handle.Parser(allText);
        }

        return handle;
    }

    /// <summary>
    /// 解析内存数据
    /// </summary>
    void Parser(string content)
    {
        // 拆分得到每行的内容
        content = content.Replace("\r\n", "\n");
        string[] lineArray = content.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lineArray.Length < 1)
            return;

        // 分解第一行
        string[] strArray = lineArray[0].Split(new char[] { '\t' });
        int recordsNum = 0;
        int fieldsNum = strArray.Length;

        // 遍历余下行数
        for (int i = 1; i < lineArray.Length; ++i)
        {
            if (lineArray[i].Length == 0)
                break;

            strArray = lineArray[i].Split(new char[] { '\t' });

            // 是不是有内容
            if (strArray.Length == 0)
                break;

            if (strArray[0].Length == 0)
                break;

            if (strArray[0][0] == '\0')
                break;

            // 是不是注释行
            if (strArray[0][0] == '#')
                continue;
            // 填充数据区
            for (int n = 0; n < fieldsNum; ++n)
            {
                _dataBuf.Add(strArray[n]);
            }

            ++recordsNum;
        }

        // 记录相关信息
        _recordsNum = recordsNum;
        _fieldsNum = fieldsNum;
    }

    /// <summary>
    /// 取数据
    /// </summary>
    /// <param name="recordLine">从0开始</param>
    /// <param name="columNum">从0开始</param>
    /// <returns></returns>
    public string GetValue(int recordLine, int columNum)
    {
        int position = recordLine * _fieldsNum + columNum;

        if (position < 0 || position > _dataBuf.Count)
        {
            string error = string.Format("文件:{0} 读取出现异常! recordLine:{1} columNum:{2}", _fileName, recordLine, columNum);
            Debug.LogError("<TableHandle> " + error);
            return "";
        }
        return _dataBuf[position];
    }
    /// <summary>
    /// 获取记录数
    /// </summary>
    public int GetRecordsNum()
    {
        return _recordsNum;
    }

    /// <summary>
    /// 获取列数
    /// </summary>
    public int GetFieldsNum()
    {
        return _fieldsNum;
    }

}
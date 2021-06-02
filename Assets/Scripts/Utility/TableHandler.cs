using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
/// <summary>
/// 表格读取工具
/// </summary>
public class TableHandler
{
    /// 行数
    private int _recordsNum;
    /// 列数
    private int _fieldsNum;
    /// 数据区
    List<string> _dataBuf;
    /// 表格的列
    string[] _columns;
    private string _fileName;
    /// 构造
    public TableHandler(string fileName)
    {
        _fileName = fileName;
        _recordsNum = 0;
        _fieldsNum = 0;
        _dataBuf = new List<string>();
    }

    public static TableHandler OpenFromData(string fileName)
    {
        TableHandler handle = new TableHandler(fileName);
        handle.Open(fileName, "Data");
        return handle;
    }

    public static TableHandler OpenFromResmap(string reskeyName)
    {
        return OpenInAB(reskeyName, ResMgr.Inst.GetMapInfo(reskeyName, 3).abName); ;
    }

    public static TableHandler OpenInAB(string fileName, string abName)
    {
        TableHandler handle = new TableHandler(fileName);
        handle.Open(fileName, abName);
        return handle;
    }

    void Open(string fileName, string abName)
    {
        string allText = null;
        if (GameMain.Inst && GameMain.Inst.ResourceMode == ResMode.ExtraData) //数据从外部读，其他的资源从AB获取
        {
            abName = abName.ToLower();
            string filePath = System.Environment.CurrentDirectory + "/Assets/" + abName.Replace("_", "/") + "/" + fileName + ".txt";
            allText = CommonUtils.ReadFileText(filePath);
        }
        else if(GameMain.Inst && GameMain.Inst.ResourceMode == ResMode.AssetBundle) //ab包模式
        {
            //数据文件的使用者为自己，不受UI界面控制
            TextAsset asset = ResMgr.Inst.LoadAsset<TextAsset>(abName, fileName, "data");
            allText = CommonUtils.DecryptTxt(asset.text);
            Resources.UnloadAsset(asset);
        }
        else
        {
            //拼装出EditorPath
            string editorPath = "Assets/" + abName.Replace("_", "/") + "/" + fileName;
            if (!fileName.EndsWith(".txt"))
                editorPath += ".txt";
#if UNITY_EDITOR
            TextAsset asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(editorPath);
            allText = asset.text;
#endif
        }

        //解析数据
        if (!string.IsNullOrEmpty(allText))
        {
            Parser(allText);
        }
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
        _columns = new string[fieldsNum];
        Array.Copy(strArray, _columns, fieldsNum);

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
                if (n >= strArray.Length)
                {
                    Debug.LogErrorFormat("<TableHandle> 数据数量不正确，文件：{1}.txt, key = {0}", strArray[0], _fileName);
                    break;
                }
                _dataBuf.Add(strArray[n]);
            }

            ++recordsNum;
        }

        // 记录相关信息
        _recordsNum = recordsNum;
        _fieldsNum = fieldsNum;
    }

    /// 取数据
    /// <param name="recordLine">从0开始</param>
    /// <param name="columNum">从0开始</param>
    public string GetValue(int recordLine, int columNum)
    {
        int position = recordLine * _fieldsNum + columNum;

        if (position < 0 || position > _dataBuf.Count)
        {
            string error = string.Format("文件:{0}.txt 读取出现异常! recordLine:{1} columNum:{2}", _fileName, recordLine, columNum);
            Debug.LogError("<TableHandle> " + error);
            return "";
        }
        return _dataBuf[position];
    }
    /// 获取列
    public string GetColumn(int columnNum)
    {
        return _columns[columnNum];
    }
    /// 获取记录数
    public int GetRecordsNum()
    {
        return _recordsNum;
    }
    /// 获取列数
    public int GetFieldsNum()
    {
        return _fieldsNum;
    }

    public string GetColumn( int recordLine, string name )
    {
        for ( int i = 0; i < _columns.Length; ++i )
        {
            // 找对应列数
            if ( _columns[i] == name )
            {
                return GetValue( recordLine, i );
            }
        }

        return "";
    }
}
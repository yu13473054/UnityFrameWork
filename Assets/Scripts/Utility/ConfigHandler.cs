using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// 配置txt文件读取工具
public class ConfigHandler
{
	/// <summary>
	/// 节点值
	/// </summary>
	private Dictionary<string, string> _dict;

    private string _filePath;

    /// <summary>
    /// 构造
    /// </summary>
    public ConfigHandler()
    {
		_dict = new Dictionary<string, string>();
    }

    public ConfigHandler(string filePath) : this()
    {
        _filePath = filePath;
    }


    /// <summary>
    /// 打开TXT文件
    /// </summary>
    public static ConfigHandler Open( string filePath )
    {
        ConfigHandler handler = new ConfigHandler(filePath);
        //直接按照路径获取
        string allText = CommonUtils.ReadFileText(filePath);
        if (!string.IsNullOrEmpty(allText))
        {
            handler.Parser(allText);
        }
        return handler;
    }

    public static ConfigHandler OpenFromAB(string fileName, string abName)
    {
        ConfigHandler handler = new ConfigHandler(fileName);

        string allText = null;
        if (GameMain.Inst.ResourceMode == 2) // 配置文件也放在外部文件夹中，需要从外部文件夹中取得
        {
            string filePath = System.Environment.CurrentDirectory + "/" + abName.Replace("_", "/") + "/" + fileName;
            allText = CommonUtils.ReadFileText(filePath);
        }
        else
        {
            //拼装出EditorPath
            string editorPath = Application.dataPath + "/" + abName.Replace("_", "/") + "/" + fileName;
            //数据文件的使用者为自己，不受UI界面控制
            TextAsset asset = ResMgr.Inst.LoadAsset<TextAsset>(abName, fileName, abName, editorPath);
            allText = asset.text;
        }
        //解析数据
        if (!string.IsNullOrEmpty(allText))
        {
            handler.Parser(allText);
        }
        return handler;
    }

    public void Parser(byte[] data)
    {
        Parser(Encoding.UTF8.GetString(data));
    }

    /// <summary>
    /// 解析内存数据
    /// </summary>
    private void Parser( string content )
    {
        // 拆分得到每行的内容
        content = content.Replace( "\r\n", "\n" );
		string[] lineArray = content.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
		if (lineArray.Length < 1)
			return;

		for ( int tmpi=0; tmpi < lineArray.Length; tmpi++ )
		{
            string st = lineArray[tmpi].Trim();
            //开始解析         
            int equalSignPos = st.IndexOf('=');
            if (equalSignPos != 0)
            {
                var key = st.Substring(0, equalSignPos);
                var value = st.Substring(equalSignPos + 1, st.Length - equalSignPos - 1);
                if (_dict.ContainsKey(key))
                    _dict[key] = value;
                else
                    _dict.Add(key, value);
            }
		}
    }

	public void Clear()
	{
		_dict.Clear();
	}        

	// 写入一个值
	public void WriteValue(string key, object value)
	{
        if(string.IsNullOrEmpty(_filePath))
            UnityEngine.Debug.LogError("<ConfigHandler> 写入的文件的路径为空！");

		if (_dict.ContainsKey(key))
			_dict[key] = value.ToString();
		else
			_dict.Add(key, value.ToString());

		string content="";
		foreach ( var item in _dict )
		{
			content = content + item.Key + "=" + item.Value + "\r\n";
		}
	    File.WriteAllText(_filePath, content);
	}

	// 读取一个值
	public string ReadValue(string key, string defaultv = "")
	{
		if (_dict.ContainsKey(key))
			return _dict[key];
		else
			return defaultv;
	}
}


using System;
using System.Collections.Generic;
using System.IO;
/// 配置txt文件读取工具
public class ConfigHandler
{
	/// <summary>
	/// 节点值
	/// </summary>
	private Dictionary<string, string> _dict;

    /// <summary>
    /// 构造
    /// </summary>
    public ConfigHandler()
    {
		_dict = new Dictionary<string, string>();
    }

    /// <summary>
    /// 打开TXT文
    /// </summary>
    public static ConfigHandler Open( string filePath )
    {
        ConfigHandler handler = new ConfigHandler();

        //todo 考虑增加从ab包中获取配置文件

        //直接按照路径获取
        string allText = File.ReadAllText(filePath);
        if (!string.IsNullOrEmpty(allText))
        {
            handler.Parser(allText);
        }
        return handler;
    }

    public void Parser(byte[] data)
    {
        Parser(data.ToString());
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
//	public void WriteValue(string key, object value)
//	{
//		if (_dict.ContainsKey(key))
//			_dict[key] = value.ToString();
//		else
//			_dict.Add(key, value.ToString());
//
//		string IniText="";
//		foreach ( var item in _dict )
//		{
//			IniText = IniText + item.Key + "=" + item.Value + "\r\n";
//		}
//		FileUtil.WriteFile( _fileName, IniText );
//	}

	// 读取一个值
	public string ReadValue(string key, string defaultv = "")
	{
		if (_dict.ContainsKey(key))
			return _dict[key];
		else
			return defaultv;
	}
}


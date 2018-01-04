using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Xml;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Object = UnityEngine.Object;


public class CustomFontMaker : EditorWindow
{
//    private Font _font;
//    private TextAsset _xmlText;
//
////    [MenuItem("Window/CustomFontMaker")]
//    static void AddWindow()
//    {
//        //创建窗口
//        Rect wr = new Rect(0, 0, 500, 500);
//        CustomFontMaker window = (CustomFontMaker)EditorWindow.GetWindowWithRect(typeof(CustomFontMaker), wr, true, "自定义字体");
//        window.Show();
//
//    }
//
//    void OnGUI()
//    {
//        _font = EditorGUILayout.ObjectField("字体", _font, typeof(Font), true) as Font;
//        _xmlText = EditorGUILayout.ObjectField("文字XML配置", _xmlText, typeof(TextAsset), true) as TextAsset;
//        Debug.Log(_xmlText);
//
//        if (GUILayout.Button("创建字体", GUILayout.Width(200)))
//        {
////            this.CreateFont();
//        }
//    }


    [MenuItem("Assets/CreateBMFont")]
    static void CreateFont()
    {
        Object obj = Selection.activeObject;
        string fntPath = AssetDatabase.GetAssetPath(obj);
        if (!fntPath.EndsWith(".fnt"))
        {
            // 不是字体文件 
            Debug.LogError("The Selected Object Is Not A .fnt file!");
            return;
        }

        string customFontPath = fntPath.Replace(".fnt", ".fontsettings");
        if (!File.Exists(customFontPath))
        {
            Debug.LogError("The fontsettings Is Not A  file!");
            return;
        }

        Debug.Log(fntPath);
        StreamReader reader = new StreamReader(new FileStream(fntPath, FileMode.Open));

        List<CharacterInfo> charList = new List<CharacterInfo>();

        Regex reg = new Regex(@"char id=(?<id>\d+)\s+x=(?<x>\d+)\s+y=(?<y>\d+)\s+width=(?<width>\d+)\s+height=(?<height>\d+)\s+xoffset=(?<xoffset>(-|\d)+)\s+yoffset=(?<yoffset>(-|\d)+)\s+xadvance=(?<xadvance>\d+)\s+");
        string line = reader.ReadLine();
        int scaleW = 512;
        int scaleH = 512;

        while (line != null)
        {
            if (line.IndexOf("char id=") != -1)
            {
                Match match = reg.Match(line);
                if (match != Match.Empty)
                {

                    var id = System.Convert.ToInt32(match.Groups["id"].Value);
                    var x = System.Convert.ToInt32(match.Groups["x"].Value);
                    var y = System.Convert.ToInt32(match.Groups["y"].Value);
                    var width = System.Convert.ToInt32(match.Groups["width"].Value);
                    var height = System.Convert.ToInt32(match.Groups["height"].Value);
                    var xoffset = System.Convert.ToInt32(match.Groups["xoffset"].Value);
                    var yoffset = System.Convert.ToInt32(match.Groups["yoffset"].Value);
                    var xadvance = System.Convert.ToInt32(match.Groups["xadvance"].Value);
                    Debug.Log("ID" + id);


                    CharacterInfo info = new CharacterInfo();
                    info.index = id;

                    float uvx = 1f* x / scaleW;  
                    float uvy = 1f * y / scaleH;  
                    float uvw = 1f * width / scaleW;  
                    float uvh = 1f * height / scaleH;  
                    
                    info.uvBottomLeft = new Vector2(uvx, uvy);  
                    info.uvBottomRight = new Vector2(uvx + uvw, uvy);  
                    info.uvTopLeft = new Vector2(uvx, uvy + uvh);  
                    info.uvTopRight = new Vector2(uvx + uvw, uvy + uvh);  

                    info.minX = xoffset;  
                    info.minY = yoffset;   // 这样调出来的效果是ok的，原理未知  
                    info.maxX = width;  
                    info.maxY = -height; // 同上，不知道为什么要用负的，可能跟unity纹理uv有关  
                    info.advance = xadvance;  
                    charList.Add(info);
                }
            }
            else if (line.IndexOf("scaleW=") != -1)
            {
                Regex reg2 = new Regex(@"common lineHeight=(?<lineHeight>\d+)\s+.*scaleW=(?<scaleW>\d+)\s+scaleH=(?<scaleH>\d+)");
                Match match = reg2.Match(line);
                if (match != Match.Empty)
                {
                    scaleW = System.Convert.ToInt32(match.Groups["scaleW"].Value);
                    scaleH = System.Convert.ToInt32(match.Groups["scaleH"].Value);
                }
            }
            line = reader.ReadLine();
        }

        Font customFont = AssetDatabase.LoadAssetAtPath<Font>(customFontPath);
        customFont.characterInfo = charList.ToArray();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log(customFont);
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

//将lua转换成字节码
public class LuaEncode
{
    const string LuaDir = "/Lua";
    const string TmpLuaDir = "/TmpLua";
    const bool IsEncode = false;

    //在打包时，将lua字节码化
    public static void StartEncode(BuildTarget target)
    {
        string luaPath = Application.dataPath + LuaDir;
        //查找到所有的lua文件
        string[] files = Directory.GetFiles(luaPath, "*.lua", SearchOption.AllDirectories);
        string tmpLuaPath = System.Environment.CurrentDirectory + TmpLuaDir;
        for(int i = 0; i< files.Length; i++)
        {
            string filePath = files[i].Replace('\\','/');
            int lastSplit = filePath.LastIndexOf('/');
            string newDir = filePath.Remove(lastSplit).Replace(luaPath, tmpLuaPath);
            Directory.CreateDirectory(newDir);
            string newLuaPath = files[i].Replace(luaPath, tmpLuaPath);
            if (File.Exists(newLuaPath)) File.Delete(newLuaPath); // 删除原来的加密文件
            UnityEditor.FileUtil.MoveFileOrDirectory(files[i], newLuaPath); //将lua文件移动到临时文件夹
            string encodeFilePath = filePath + ".bytes";
            if (File.Exists(encodeFilePath)) File.Delete(encodeFilePath); // 删除原来的加密文件
            EncodeLuaFile(target, newLuaPath, encodeFilePath);// 开始加密
        }
        AssetDatabase.Refresh();
    }

    public static void EndEncode()
    {
        string luaPath = Application.dataPath + LuaDir;
        UnityEditor.FileUtil.DeleteFileOrDirectory(luaPath);
        string tmpLuaPath = System.Environment.CurrentDirectory + TmpLuaDir;
        UnityEditor.FileUtil.MoveFileOrDirectory(tmpLuaPath, luaPath);
        AssetDatabase.Refresh();
    }

    static void EncodeLuaFile(BuildTarget target, string srcFile, string outFile)
    {
        if(!IsEncode || target == BuildTarget.StandaloneOSX) //  mac上单独处理：不加密
        {
            UnityEditor.FileUtil.CopyFileOrDirectory(srcFile, outFile);
            return;
        }

        string currDir = Directory.GetCurrentDirectory();
        bool isWin = true;
        string luaexe = string.Empty;
        string exedir = currDir+ "/LuaEncoder/";
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            isWin = true;
            luaexe = "luajit.exe";
            if (target == BuildTarget.Android)
                exedir += "win32/";
            else
                exedir += "win64/";
        }
        else if (Application.platform == RuntimePlatform.OSXEditor)
        {
            isWin = false;
            if (target == BuildTarget.Android)
                luaexe = "luajit-32";
            else
                luaexe = "luajit-64";
        }
        Directory.SetCurrentDirectory(exedir);//windows平台需要切换到对应目录
        ProcessStartInfo info = new ProcessStartInfo();
        info.FileName = luaexe;
        info.Arguments = "-b -g " + srcFile + " " + outFile;
        info.WindowStyle = ProcessWindowStyle.Hidden;
        info.UseShellExecute = isWin;
        info.ErrorDialog = true;

        Process pro = Process.Start(info);
        pro.WaitForExit();
        pro.Dispose();
        Directory.SetCurrentDirectory(currDir);
    }
}

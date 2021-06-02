using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public struct EncodeInfo
{
    public bool encode;
    public bool isWin;
    public string exe;
    public string extDir;
    public string srcFile;
    public string outFile;

}

//将lua转换成字节码
public class LuaEncode
{
    const string LuaDir = "/Lua";
    const string TmpLuaDir = "/TmpLua";
    private const string BackupMetaDir = "/BackupLuaMeta";

    static void CopyFileOrDirectory(string src, string dest)
    {
        string dir = Path.GetDirectoryName(dest);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        UnityEditor.FileUtil.CopyFileOrDirectory(src, dest);
    }

//    [MenuItem("LuaEncode/加密")]
//    public static void StartEncode()
//    {
//        StartEncode(BuildTarget.Android);
//    }

    public static void StartEncode(BuildTarget target)
    {
        string luaPath = Application.dataPath + LuaDir;
        // 删除原来的加密文件
        if (Directory.Exists(luaPath+"/32"))
            UnityEditor.FileUtil.DeleteFileOrDirectory(luaPath + "/32");
        if (Directory.Exists(luaPath + "/64"))
            UnityEditor.FileUtil.DeleteFileOrDirectory(luaPath + "/64");
        string tmpLuaPath = System.Environment.CurrentDirectory + TmpLuaDir;
        //备份源文件
        if (Directory.Exists(tmpLuaPath))
            UnityEditor.FileUtil.DeleteFileOrDirectory(tmpLuaPath);
        UnityEditor.FileUtil.MoveFileOrDirectory(luaPath, tmpLuaPath);

        //生成加密用源文件
        if (LuaMgr.IsEncode && target == BuildTarget.Android)
        {
            CopyFileOrDirectory(tmpLuaPath, luaPath + "/32"); //生成32位lua文件
            CopyFileOrDirectory(tmpLuaPath, luaPath + "/64"); //64位lua文件
        }
        else if (LuaMgr.IsEncode && target == BuildTarget.iOS)
        {
            CopyFileOrDirectory(tmpLuaPath, luaPath + "/64"); //64位lua文件
        }
        else
        {
            CopyFileOrDirectory(tmpLuaPath, luaPath + "/32"); //生成32位lua文件
        }
        AssetDatabase.Refresh();

        if (LuaMgr.IsEncode && target == BuildTarget.Android) //同时存在32位和64位，需要备份64的meta信息
        {
            string[] metaList = Directory.GetFiles(luaPath + "/64", "*.meta", SearchOption.AllDirectories);
            for (int i = 0; i < metaList.Length; i++)
            {
                string src = metaList[i].Replace('\\', '/');
                string dest = src.Replace("/Assets/Lua/64", BackupMetaDir);
                if (!File.Exists(dest))
                {
                    CopyFileOrDirectory(src, dest);
                }
            }
        }

        //lua文件
        if (LuaMgr.IsEncode)
        {
            if (target == BuildTarget.Android)
            {
                DealLuaFile(luaPath+"/32", 1);
                DealLuaFile(luaPath+"/64", 2);
            }
            else if (target == BuildTarget.iOS)
            {
                DealLuaFile(luaPath + "/64", 4);
            }
            else if(target == BuildTarget.StandaloneOSX)
            {
                DealLuaFile(luaPath + "/32", 0);
            }
            else if( target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64)
            {
                DealLuaFile(luaPath + "/32", 3);
            }
        }
        else
        {
            DealLuaFile(luaPath + "/32", 0);
        }

        //meta信息
        if (LuaMgr.IsEncode && target == BuildTarget.Android)
        {
            DealDefaultMeta(luaPath + "/32"); //重命名

            //从备份文件夹中拷贝
            string[] metas = Directory.GetFiles(luaPath + "/64", "*.meta", SearchOption.AllDirectories);
            for (int i = 0; i < metas.Length; i++)
            {
                string srcFile = metas[i];
                if (srcFile.EndsWith(".lua.meta"))
                {
                    UnityEditor.FileUtil.DeleteFileOrDirectory(srcFile);
                    CopyFileOrDirectory(
                        srcFile.Replace("/Assets/Lua/64", BackupMetaDir), srcFile.Replace(".meta", ".bytes.meta"));
                }
                else
                {
                    UnityEditor.FileUtil.ReplaceFile(srcFile.Replace("/Assets/Lua/64", BackupMetaDir), srcFile);
                }
            }
        }
        else if (LuaMgr.IsEncode && target == BuildTarget.iOS)
        {
            DealDefaultMeta(luaPath + "/64"); //重命名
        }
        else
        {
            DealDefaultMeta(luaPath + "/32"); //重命名
        }
        AssetDatabase.Refresh();
    }

//    [MenuItem("LuaEncode/还原")]
    public static void EndEncode()
    {
        string luaPath = Application.dataPath + LuaDir;
        UnityEditor.FileUtil.DeleteFileOrDirectory(luaPath);
        string tmpLuaPath = System.Environment.CurrentDirectory + TmpLuaDir;
        UnityEditor.FileUtil.MoveFileOrDirectory(tmpLuaPath, luaPath);
        AssetDatabase.Refresh();
    }

    static void DealLuaFile(string dir, int type)
    {
        string[] fileList = Directory.GetFiles(dir, "*.lua", SearchOption.AllDirectories);
        for (int i = 0; i < fileList.Length; i++)
        {
            EncodeInfo info = new EncodeInfo();
            info.srcFile = fileList[i].Replace('\\', '/');
            if (type == 0) // 不加密
            {
                info.encode = false;
                info.outFile = info.srcFile + ".bytes";
            }
            else
            {
                info.encode = true;
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    info.isWin = true;
                    if (type == 1) // Android 32位
                    {
                        info.exe = "luajit.exe";
                        info.extDir = System.Environment.CurrentDirectory + "/LuaEncoder/win/android/32";
                        info.outFile = info.srcFile + ".bytes";
                    }
                    else if (type == 2) //Android 64位
                    {
                        info.exe = "luajit64.exe";
                        info.extDir = System.Environment.CurrentDirectory + "/LuaEncoder/win/android/64";
                        info.outFile = info.srcFile + ".bytes";
                    }
                    else if (type == 3) //win pc
                    {
                        info.exe = "luajit.exe";
                        info.extDir = System.Environment.CurrentDirectory + "/LuaEncoder/win/pc";
                        info.outFile = info.srcFile + ".bytes";
                    }
                }
                else if (Application.platform == RuntimePlatform.OSXEditor)
                {
                    info.isWin = false;
                    if (type == 1) // Android 32位
                    {
                        info.exe = "luajit-32";
                        info.extDir = System.Environment.CurrentDirectory + "/LuaEncoder/macos/android/32";
                        info.outFile = info.srcFile + ".bytes";
                    }
                    else if (type == 2) //Android 64位
                    {
                        info.exe = "luajit-64";
                        info.extDir = System.Environment.CurrentDirectory + "/LuaEncoder/macos/android/64";
                        info.outFile = info.srcFile + ".bytes";

                    }
                    else if (type == 4) // ios
                    {
                        info.exe = "luajit-64";
                        info.extDir = System.Environment.CurrentDirectory + "/LuaEncoder/macos/ios";
                        info.outFile = info.srcFile + ".bytes";
                    }
                }
            }
            EncodeLuaFile(info);
            UnityEditor.FileUtil.DeleteFileOrDirectory(info.srcFile);

        }
    }

    static void EncodeLuaFile(EncodeInfo encodeInfo)
    {
        if(!encodeInfo.encode) // 不加密
        {
            UnityEditor.FileUtil.CopyFileOrDirectory(encodeInfo.srcFile, encodeInfo.outFile);
            return;
        }

        string currentDirectory = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(encodeInfo.extDir);
        ProcessStartInfo info = new ProcessStartInfo
        {
            FileName = encodeInfo.exe,
            Arguments = "-b -g " + encodeInfo.srcFile + " " + encodeInfo.outFile,
            WindowStyle = ProcessWindowStyle.Hidden,
            UseShellExecute = encodeInfo.isWin,
            ErrorDialog = true
        };

        Process pro = Process.Start(info);
        pro.WaitForExit();
        pro.Dispose();
        Directory.SetCurrentDirectory(currentDirectory);
    }

    static void DealDefaultMeta(string dir)
    {
        string tmpLuaDir = Directory.GetCurrentDirectory()+TmpLuaDir;
        string[] metas = Directory.GetFiles(dir, "*.meta", SearchOption.AllDirectories);
        for (int i = 0; i < metas.Length; i++)
        {
            string srcFile = metas[i];
            srcFile = srcFile.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            File.Delete(srcFile);
            if (srcFile.EndsWith(".lua.meta"))
            {
                UnityEditor.FileUtil.CopyFileOrDirectory(srcFile.Replace(dir, tmpLuaDir), srcFile.Replace(".meta", ".bytes.meta"));
            }
            else
            {
                UnityEditor.FileUtil.CopyFileOrDirectory(srcFile.Replace(dir, tmpLuaDir), srcFile);
            }
        }
    }

//    [MenuItem("LuaEncode/Lua打包", false, 102)]
//    static void BuildLua()
//    {
//        string ABPATH = "Assets/StreamingAssets";
//        BuildPipeline.BuildAssetBundles(ABPATH, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
//    }
}

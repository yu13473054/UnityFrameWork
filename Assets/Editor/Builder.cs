using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;

public class Builder
{
    const string RES_FOLDER = "";
    const string ABPATH = "Assets/StreamingAssets/assetbundle";

    [MenuItem("打包/资源出包(打包+上传)", false, 1)]
    static void Build_Upload()
    {
        BuildAll();
        Upload();
    }
    [MenuItem("打包/资源打包(不上传)", false, 101)]
    static void Build_NotUpload()
    {
        BuildAll();
    }

    [MenuItem("打包/Lua打包(不上传)", false, 102)]
    static void BuildLua()
    {
        // 创建文件夹
        Directory.CreateDirectory(ABPATH);

        // 重命名assetbundle
        FileInfo oldFile = new FileInfo( ABPATH + "/assetbundle" );
        if (oldFile.Exists)
            oldFile.MoveTo(ABPATH + "/Tmp");
        else
            oldFile = null;
        DeleteAB("lua_");
        AssetBundleNameAuto.ClearAllABNames();

        EditorUtility.DisplayProgressBar("打包前准备", "正在处理Lua文件...", 0.1f);
        LuaEncode.StartEncode(EditorUserBuildSettings.activeBuildTarget);
        AssetBundleNameAuto.SetLuaABNames();

        EditorUtility.DisplayProgressBar("打包", "正在打包Lua文件...", 0.5f);
        BuildPipeline.BuildAssetBundles(ABPATH, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);

        EditorUtility.DisplayProgressBar("收尾", "Lua还原中...", 0.7f);
        LuaEncode.EndEncode();
        AssetBundleNameAuto.ClearLuaABNames();

        EditorUtility.DisplayProgressBar("收尾", "Lua加密中...", 0.9f);
        Encrypt("lua_");
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();

        // 删除新assetbundle
        UnityEditor.FileUtil.DeleteFileOrDirectory( ABPATH + "/assetbundle" );
        // 还原文件
        if( oldFile != null )
            oldFile.MoveTo( ABPATH + "/assetbundle" );
    }

    [MenuItem("打包/配置打包(不上传)", false, 103)]
    static void BuildData()
    {
        // 创建文件夹
        Directory.CreateDirectory(ABPATH);

        // 重命名assetbundle
        FileInfo oldFile = new FileInfo( ABPATH + "/assetbundle" );
        if( oldFile != null )
            oldFile.MoveTo( ABPATH + "/Tmp" );

        DeleteAB("data");

        AssetBundleNameAuto.ClearAllABNames();
        EditorUtility.DisplayProgressBar("打包前准备", "正在生成resmap文件...", 0.1f);
        //重新生成Resmap文件
        AutoResMap.AddAllAsset();
        EditorUtility.DisplayProgressBar("打包前准备", "正在处理Data文件...", 0.3f);
        AssetBundleNameAuto.SetDataABNames();

        EditorUtility.DisplayProgressBar("打包", "正在打包Data文件...", 0.8f);
        BuildPipeline.BuildAssetBundles(ABPATH, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
        AssetBundleNameAuto.ClearDataABNames();
        EditorUtility.DisplayProgressBar("收尾", "Data加密中...", 0.9f);
        Encrypt("data");
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();

        // 删除新assetbundle
        UnityEditor.FileUtil.DeleteFileOrDirectory( ABPATH + "/assetbundle" );
        // 还原文件
        if( oldFile != null )
            oldFile.MoveTo( ABPATH + "/assetbundle" );
    }

    [MenuItem("打包/上传当前版本", false, 104)]
    static void Upload()
    {
        EditorUtility.DisplayProgressBar("准备", "删除Manifest文件...", 0.1f);
        RemoveManifest();
        EditorUtility.DisplayProgressBar("准备", "增加版本号...", 0.3f);
        // 版本号处理 ：修改app配置的版本号，每次打包+1
        ConfigHandlerEditor resverIni = ConfigHandlerEditor.Open(Application.streamingAssetsPath + "/version.txt");
        int new_resource_version = int.Parse(resverIni.ReadValue("Resource_Version", "0").ToString()) + 1;
        resverIni.WriteValue("Resource_Version", new_resource_version);

        // 上传ab
        EditorUtility.DisplayProgressBar("上传", "上传ab...", 0.8f);
        UploadDirectory(ABPATH, _ftpServerIP + "/" + RES_FOLDER + "/" + GameMain.platformName + "/", new_resource_version.ToString());

        EditorUtility.DisplayProgressBar("上传", "上传版本文件...", 0.1f);
        // 上传版本文件
        UploadFile("Assets/StreamingAssets/version.txt", _ftpServerIP + "/" + RES_FOLDER + "/" + GameMain.platformName + "/version.txt");
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("打包/删除打包的AB文件", false, 401)]
    static void RemoveBuildFile()
    {
        UnityEditor.FileUtil.DeleteFileOrDirectory(ABPATH);
        AssetDatabase.Refresh();
    }

    [MenuItem("打包/删除manifest文件", false, 402)]
    static void RemoveManifest()
    {
        DirectoryInfo dir = new DirectoryInfo(ABPATH);
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string path = file.FullName.Replace("\\", "/");
            string filename = Path.GetFileName(path);
            if (Path.GetExtension(filename).Equals(".manifest"))
                UnityEditor.FileUtil.DeleteFileOrDirectory(path);
        }
        AssetDatabase.Refresh();
    }

    static void BuildAll()
    {
        AssetBundleNameAuto.ClearAllABNames();
        // 删除构建的文件
        RemoveBuildFile();
        // 创建文件夹
        Directory.CreateDirectory(ABPATH);

        AppConst.Init();

        //重新生成Resmap文件
        EditorUtility.DisplayProgressBar("打包前准备", "正在重新生成Resmap文件...", 0.1f);
        AutoResMap.AddAllAsset();
        //打断资源引用
        EditorUtility.DisplayProgressBar("打包前准备", "正在打断多语言资源的引用...", 0.2f);
        MultiLngResDetach.Done();

        // 自动设置AB名
        EditorUtility.DisplayProgressBar("打包前准备", "正在自动设置AB名...", 0.3f);
        AssetBundleNameAuto.SetResABNames();
        AssetBundleNameAuto.SetDataABNames();
        //lua打包特殊处理
        EditorUtility.DisplayProgressBar("打包前准备", "处理Lua文件的AB名...", 0.4f);
        LuaEncode.StartEncode(EditorUserBuildSettings.activeBuildTarget);
        AssetBundleNameAuto.SetLuaABNames();
        // 打包
        EditorUtility.DisplayProgressBar("打包", "AB打包中...", 0.7f);
        BuildPipeline.BuildAssetBundles(ABPATH, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);

        EditorUtility.DisplayProgressBar("收尾", "Lua还原中...", 0.8f);
        LuaEncode.EndEncode();

        // 创建文件列表
        EditorUtility.DisplayProgressBar( "统计", "生成FileList...", 0.9f );
        BuildFileList();
        AssetBundleNameAuto.ClearAllABNames();

        EditorUtility.DisplayProgressBar("统计", "ab加密中...", 1f);
        Encrypt();
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();

    }

    // 生成文件列表
    [MenuItem( "打包/生成文件列表", false, 501 )]
    static void BuildFileList()
    {
        // 创建文件列表，并打包成ab
        AssetDatabase.CreateFolder( "Assets", "Temp" );
        string[] files = Directory.GetFiles( ABPATH );
        using( StreamWriter sw = new StreamWriter( "Assets/Temp/filelist.txt", false ) )
        {
            string fileList = "";
            for( int i = 0; i < files.Length; i++ )
            {
                string file = files[i];
                string ext = Path.GetExtension( file );
                string fileName = Path.GetFileName( file );
                if( ext.Equals( ".meta" ) || ext.Equals( ".manifest" ) )
                    continue;

                // md5 值
                string md5 = CommonUtils.Md5file( file );
                // 文件大小
                FileInfo fileInfo = new FileInfo( file );
                long size = fileInfo.Length;
                fileList += fileName + "|" + md5 + "|" + size + "\n";
            }
            fileList = fileList.TrimEnd( '\n' );
            sw.Write( fileList );
            sw.Close();
        }
        AssetDatabase.Refresh();

        // 把filelist打ab压缩
        AssetBundleBuild filelistAB = new AssetBundleBuild();
        filelistAB.assetBundleName = "filelist";
        filelistAB.assetNames = new string[] { "Assets/Temp/filelist.txt" };
        BuildPipeline.BuildAssetBundles( "Assets/Temp", new AssetBundleBuild[] { filelistAB }, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget );
        File.Copy( "Assets/Temp/filelist", ABPATH + "/filelist", true );
        UnityEditor.FileUtil.DeleteFileOrDirectory( "Assets/Temp" );
    }

    // 复制Lua，改后缀以打包
    static void LuaPrepare(bool isProcess)
    {
        DirectoryInfo dir = new DirectoryInfo("Assets/Lua");
        FileInfo[] files = dir.GetFiles("*.lua", SearchOption.AllDirectories);
        if (isProcess)
        {
            // 生成新的文件
            foreach (FileInfo file in files)
            {
                string path = file.FullName.Replace("\\", "/");
                UnityEditor.FileUtil.CopyFileOrDirectory(path, path + ".bytes");
                UnityEditor.FileUtil.DeleteFileOrDirectory(path);
            }
            AssetDatabase.Refresh();
        }
        else
        {
            // 删除生成文件
            files = dir.GetFiles("*.bytes", SearchOption.AllDirectories);
            foreach (FileInfo file in files)
            {
                string path = file.FullName.Replace("\\", "/");
                UnityEditor.FileUtil.CopyFileOrDirectory(path, path.TrimEnd(".bytes".ToCharArray()));
                UnityEditor.FileUtil.DeleteFileOrDirectory(path);
            }
            AssetDatabase.Refresh();
        }
    }

    //删除ab包
    static void DeleteAB(string filter)
    {
        string[] files = Directory.GetFiles(ABPATH);
        for (int i = 0; i < files.Length; i++)
        {
            string path = files[i];
            string fileName = Path.GetFileName(path);
            if (fileName.Contains(filter))
            {
                File.Delete(path);
            }
        }
    }

    #region 加密
    //10位
    private static byte[] headBytes = { 1, 2, 3, 4, 5, 6, 12, 32, 3, 45};
    static void Encrypt(string filter = "")
    {
        List<byte> result = new List<byte>();
        string[] files = Directory.GetFiles(ABPATH);
        for(int i =0; i< files.Length; i++)
        {
            string path = files[i];
            string fileName = Path.GetFileName(path);
            if (fileName.Contains(".meta") || fileName.Contains(".manifest")) continue;
            if (string.IsNullOrEmpty(filter) || fileName.Contains(filter))
            {
                result.Clear();
                result.AddRange(headBytes);
                result.AddRange(File.ReadAllBytes(path));
                File.WriteAllBytes(path, result.ToArray());
            }
        }
    }
    #endregion

    /************************************************************************/
    // FTP
    /************************************************************************/
    static string _ftpServerIP = "ftp://172.16.200.45/";//服务器ip  
    static string _ftpUserID = "npc";//用户名  
    static string _ftpPassword = "npc!@#$";//密码

    #region 上传文件
    // 上传文件  
    public static void UploadFile(string localFile, string ftpPath)
    {
        if (!File.Exists(localFile))
        {
            UnityEngine.Debug.LogError("文件：“" + localFile + "” 不存在！");
            return;
        }
        FileInfo fileInf = new FileInfo(localFile);
        FtpWebRequest reqFTP;

        reqFTP = (FtpWebRequest)FtpWebRequest.Create(ftpPath);// 根据uri创建FtpWebRequest对象   
        reqFTP.Credentials = new NetworkCredential(_ftpUserID, _ftpPassword);// ftp用户名和密码  
        reqFTP.KeepAlive = false;// 默认为true，连接不会被关闭 // 在一个命令之后被执行  
        reqFTP.Method = WebRequestMethods.Ftp.UploadFile;// 指定执行什么命令  
        reqFTP.UseBinary = true;// 指定数据传输类型  
        reqFTP.ContentLength = fileInf.Length;// 上传文件时通知服务器文件的大小  
        int buffLength = 2048;// 缓冲大小设置为2kb  
        byte[] buff = new byte[buffLength];
        int contentLen;

        // 打开一个文件流 (System.IO.FileStream) 去读上传的文件  
        FileStream fs = fileInf.OpenRead();
        try
        {
            Stream strm = reqFTP.GetRequestStream();// 把上传的文件写入流  
            contentLen = fs.Read(buff, 0, buffLength);// 每次读文件流的2kb  

            while (contentLen != 0)// 流内容没有结束  
            {
                // 把内容从file stream 写入 upload stream  
                strm.Write(buff, 0, contentLen);
                contentLen = fs.Read(buff, 0, buffLength);
            }
            // 关闭两个流  
            strm.Close();
            fs.Close();
            UnityEngine.Debug.Log("文件【" + ftpPath + "】上传成功！");
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("上传文件【" + ftpPath + "】时，发生错误：" + ex.Message);
        }
    }
    #endregion

    #region 上传文件夹
    // 上传整个目录  
    public static void UploadDirectory(string localDir, string ftpPath, string dirName)
    {
        localDir += "/";
        //检测本地目录是否存在  
        if (!Directory.Exists(localDir))
        {
            UnityEngine.Debug.LogError("本地目录：“" + localDir + "” 不存在！");
            return;
        }

        string uri = ftpPath + dirName;
        //检测FTP的目录路径是否存在  
        if (!CheckDirectoryExist(ftpPath, dirName))
        {
            MakeDir(ftpPath, dirName);//不存在，则创建此文件夹  
        }
        List<List<string>> infos = GetDirDetails(localDir); //获取当前目录下的所有文件和文件夹  

        //先上传文件
        for (int i = 0; i < infos[0].Count; i++)
        {
            string ext = Path.GetExtension(infos[0][i]);
            string fileName = Path.GetFileName(infos[0][i]);
            if (ext.Equals(".meta") || ext.Equals(".manifest") || fileName == "app.txt" || fileName == "version.txt")
                continue;

            UploadFile(localDir + fileName, uri + "/" + fileName);
            EditorUtility.DisplayProgressBar("上传", "上传中...", (float)i / (float)infos[0].Count);
        }
        //再处理文件夹  
        for (int i = 0; i < infos[1].Count; i++)
        {
            UploadDirectory(localDir, uri + "/", infos[1][i]);
        }

        UnityEngine.Debug.Log("资源版本：【" + dirName + "】上传完毕！");
        EditorUtility.ClearProgressBar();
    }

    // 判断ftp服务器上该目录是否存在  
    private static bool CheckDirectoryExist(string ftpPath, string dirName)
    {
        bool flag = true;
        try
        {
            string uri = ftpPath + dirName + "/";
            //实例化FTP连接  
            FtpWebRequest ftp = (FtpWebRequest)FtpWebRequest.Create(uri);
            ftp.Credentials = new NetworkCredential(_ftpUserID, _ftpPassword);
            ftp.Method = WebRequestMethods.Ftp.ListDirectory;
            FtpWebResponse response = (FtpWebResponse)ftp.GetResponse();
            response.Close();
        }
        catch (Exception)
        {
            flag = false;
        }
        return flag;
    }

    // 创建文件夹
    public static void MakeDir(string ftpPath, string dirName)
    {
        FtpWebRequest reqFTP;
        try
        {
            string uri = ftpPath + dirName;
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(uri);
            reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
            reqFTP.UseBinary = true;
            reqFTP.Credentials = new NetworkCredential(_ftpUserID, _ftpPassword);
            FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
            Stream ftpStream = response.GetResponseStream();
            ftpStream.Close();
            response.Close();
            UnityEngine.Debug.Log("文件夹【" + dirName + "】创建成功！");
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("新建文件夹【" + dirName + "】时，发生错误：" + ex.Message);
        }

    }

    // 获取目录下的详细信息  
    static List<List<string>> GetDirDetails(string localDir)
    {
        List<List<string>> infos = new List<List<string>>();
        try
        {
            infos.Add(Directory.GetFiles(localDir).ToList()); //获取当前目录的文件  

            infos.Add(Directory.GetDirectories(localDir).ToList()); //获取当前目录的目录  

            for (int i = 0; i < infos[0].Count; i++)
            {
                int index = infos[0][i].LastIndexOf(@"\");
                infos[0][i] = infos[0][i].Substring(index + 1);
            }
            for (int i = 0; i < infos[1].Count; i++)
            {
                int index = infos[1][i].LastIndexOf(@"\");
                infos[1][i] = infos[1][i].Substring(index + 1);
            }
        }
        catch (Exception ex)
        {
            ex.ToString();
        }
        return infos;
    }
    #endregion
}
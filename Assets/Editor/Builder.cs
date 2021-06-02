using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using AssetDanshari;
using ICSharpCode.SharpZipLib.Zip;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

public class Builder : IPostprocessBuildWithReport
{
    public struct ABInfo
    {
        public string abName;
        public string path;
        public bool isValid;
    }


    const string ABPATH             = "Assets/StreamingAssets/assetbundle";
    const string ORIGINABPATH       = "Assets/OriginAB/assetbundle";

    private static List<string> googleAB = new List<string>()
    {
        "assetbundle",
        "res_AssetsUpdate",
        "res_shader",
        "Localization_CN_data_Excel",
        "Localization_CN_data_txt",
        "Localization_EN_data_Excel",
        "Localization_EN_data_txt",
        "Localization_JP_data_Excel",
        "Localization_JP_data_txt",
        "Data_Data"
    };


    [MenuItem("打包/资源出包(打包+上传)", false, 1)]
    static void Build_Upload()
    {
        Build_NotUpload();
        Upload();
    }
    [MenuItem("打包/资源打包(不上传)", false, 101)]
    static void Build_NotUpload()
    {
        BuildAll();
        EncryptAB(); //加密
        BuildFileList();
    }

    [MenuItem("打包/Lua打包(不上传)", false, 102)]
    static void BuildLua()
    {
        ResmapUtility.canAuto2Resmap = false; //不自动导入到resmap中

        // 创建文件夹
        Directory.CreateDirectory(ORIGINABPATH);

        // 重命名assetbundle
        FileInfo oldFile = new FileInfo(ORIGINABPATH + "/assetbundle" );
        if( oldFile.Exists )
            oldFile.MoveTo(ORIGINABPATH + "/Tmp" );

        ABNameUtility.ClearAllABNames();
        DeleteFiles("lua_");

        EditorUtility.DisplayProgressBar("打包前准备", "正在处理Lua文件...", 0.1f);
        LuaEncode.StartEncode(EditorUserBuildSettings.activeBuildTarget);
        ABNameUtility.SetLuaABNames();

        EditorUtility.DisplayProgressBar("打包", "正在打包Lua文件...", 0.5f);
        BuildPipeline.BuildAssetBundles(ORIGINABPATH, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);

        LuaEncode.EndEncode();
        EditorUtility.DisplayProgressBar("收尾", "Lua还原中...", 0.7f);

        ABNameUtility.ClearLuaABNames();
        EditorUtility.DisplayProgressBar("收尾", "Lua加密中...", 0.9f);
        EncryptAB("lua_");
        EditorUtility.ClearProgressBar();
        // 删除新assetbundle
        UnityEditor.FileUtil.DeleteFileOrDirectory(ORIGINABPATH + "/assetbundle" );
        // 还原文件
        if( oldFile.Exists )
            oldFile.MoveTo(ORIGINABPATH + "/assetbundle" );
        AssetDatabase.Refresh();
    }

    [MenuItem("打包/配置打包(不上传)", false, 103)]
    static void BuildData()
    {
        ResmapUtility.canAuto2Resmap = false; //不自动导入到resmap中

        // 创建文件夹
        Directory.CreateDirectory(ORIGINABPATH);
        DeleteFiles("data_"); //删除旧文件

        // 重命名assetbundle
        FileInfo oldFile = new FileInfo(ORIGINABPATH + "/assetbundle" );
        if( oldFile.Exists )
            oldFile.MoveTo(ORIGINABPATH + "/Tmp" );

        ABNameUtility.ClearAllABNames();
        EditorUtility.DisplayProgressBar("打包前准备", "正在生成resmap文件...", 0.1f);
        //重新生成Resmap文件
        ResmapUtility.AddAllAsset();
        EditorUtility.DisplayProgressBar("打包前准备", "正在处理Data文件...", 0.3f);
        ABNameUtility.SetDataABNames();

        EditorUtility.DisplayProgressBar("打包", "正在打包Data文件...", 0.8f);
        FileEncrypt.Encrypt();//加密Txt
        AssetDatabase.Refresh();
        BuildPipeline.BuildAssetBundles(ORIGINABPATH, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);

        AssetDatabase.Refresh();
        ABNameUtility.ClearDataABNames();
        EditorUtility.DisplayProgressBar("打包", "Data文件加密中...", 0.9f);
        EncryptAB("data_");
        FileEncrypt.Decrypt();//还原Txt
        EditorUtility.ClearProgressBar();
        // 删除新assetbundle
        UnityEditor.FileUtil.DeleteFileOrDirectory(ORIGINABPATH + "/assetbundle" );
        // 还原文件
        if( oldFile.Exists )
            oldFile.MoveTo(ORIGINABPATH + "/assetbundle" );
        AssetDatabase.Refresh();
    }

    static void Upload()
    {
        // 压缩
        EditorUtility.DisplayProgressBar( "准备", "压缩中...", 0.5f );
        Package();
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("打包/删除打包的AB文件", false, 401)]
    static void RemoveBuildFile()
    {
        UnityEditor.FileUtil.DeleteFileOrDirectory(ORIGINABPATH);
        UnityEditor.FileUtil.DeleteFileOrDirectory(ABPATH);
        AssetDatabase.Refresh();
    }

    [MenuItem("打包/删除manifest文件", false, 402)]
    static void RemoveManifest()
    {
        DirectoryInfo dir = new DirectoryInfo(ORIGINABPATH);
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

    //保留相关的依赖包
    static void LoadDependencies(Dictionary<string, bool> abDic, AssetBundleManifest manifest, string abName)
    {
        string[] dependencies = manifest.GetAllDependencies(abName);
        for (int i = 0; i < dependencies.Length; i++)
        {
            string depName = dependencies[i];
            string abHash = depName.GetHashCode().ToString();
            if (abDic.ContainsKey(abHash)) continue;
            abDic.Add(abHash, true);
            LoadDependencies(abDic, manifest, depName);
        }
    }
    //生成google包
    static void BuildGoogle()
    {
        //********生成小包资源********
        AssetBundle.UnloadAllAssetBundles(true);
        EditorUtility.DisplayProgressBar("整理", "统计需保留的资源名...", 0.1f);
        Dictionary<string, bool> abDic = new Dictionary<string, bool>();
        AssetBundle rootAB = AssetBundle.LoadFromFile(ORIGINABPATH+"/assetbundle", 0, 18);
        AssetBundleManifest manifest = rootAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        rootAB.Unload(false);
        for (int i = 0; i < googleAB.Count; i++)
        {
            string abName = googleAB[i].ToLower();
            string abHash = abName.GetHashCode().ToString();
            if (abDic.ContainsKey(abHash)) continue;
            abDic.Add(abHash, true);
            LoadDependencies(abDic, manifest, abName);
        }

        EditorUtility.DisplayProgressBar("整理", "生成小包资源...", 0.3f);
        string[] files = Directory.GetFiles(ABPATH);
        for (int i = files.Length - 1; i >=0 ; i--)
        {
            string file = files[i];
            string fileName = Path.GetFileNameWithoutExtension(file);
            if(abDic.ContainsKey(fileName))
                continue;
            UnityEditor.FileUtil.DeleteFileOrDirectory(file);
        }
        AssetDatabase.Refresh();
        EditorUtility.DisplayProgressBar("整理", "重新生成FileList...", 0.9f);
        BuildFileList();
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
        ResmapUtility.canAutoToResmap = true;
    }

    static void BuildAll()
    {
        ResmapUtility.canAuto2Resmap = false; //不自动导入到resmap中
        // 创建文件夹
        Directory.CreateDirectory(ORIGINABPATH);
        if (Directory.Exists(ABPATH))
            FileUtil.DeleteFileOrDirectory(ABPATH);
        //记录所有的ab包，打包后，对无用的资源进行删除
        EditorUtility.DisplayProgressBar("打包前准备", "收集已有AB资源...", 0.05f);
        Dictionary<string, ABInfo> _infoDic = new Dictionary<string, ABInfo>();
        string[] files = Directory.GetFiles(ORIGINABPATH);
        for (int i = 0; i < files.Length; i++)
        {
            string path = files[i];
            if (path.EndsWith(".meta") || path.EndsWith(".manifest"))
                continue;
            string fileName = Path.GetFileNameWithoutExtension(path);
            if (fileName.Equals("assetbundle"))
                continue;
            _infoDic.Add(fileName, new ABInfo()
            {
                abName = fileName,
                path = path
            });
        }
        ABNameUtility.ClearAllABNames();
        EditorUtility.DisplayProgressBar("打包前准备", "开始处理多语言资源...", 0.1f);
        MultiLngResDetach.StartDeal();
        EditorUtility.DisplayProgressBar("打包前准备", "正在重新生成Resmap文件...", 0.2f);
        ResmapUtility.AddAllAsset();
        EditorUtility.DisplayProgressBar("打包前准备", "Lua文件加密中...", 0.25f);
        LuaEncode.StartEncode(EditorUserBuildSettings.activeBuildTarget);
        EditorUtility.DisplayProgressBar("打包前准备", "配置文件加密中...", 0.3f);
        FileEncrypt.Encrypt();
        AssetDatabase.Refresh();

        EditorUtility.DisplayProgressBar( "开始打包", "正在自动设置AB名...", 0.4f );
        ABNameUtility.SetAllABNames();
        // 打包
        EditorUtility.DisplayProgressBar("打包", "AB打包中...", 0.6f);
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(ORIGINABPATH, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget );
        string[] bundles = manifest.GetAllAssetBundles();
        for (int i = 0; i < bundles.Length; i++)
        {
            if (_infoDic.ContainsKey(bundles[i]))
            {
                _infoDic.Remove(bundles[i]);
            }
        }
        //删除老旧的ab包
        foreach (KeyValuePair<string, ABInfo> pair in _infoDic)
        {
            ABInfo info = pair.Value;
            UnityEditor.FileUtil.DeleteFileOrDirectory(info.path);
            UnityEditor.FileUtil.DeleteFileOrDirectory(info.path + ".manifest");
        }
        EditorUtility.DisplayProgressBar("收尾", "配置文件还原中...", 0.75f);
        FileEncrypt.Decrypt();//还原txt
        EditorUtility.DisplayProgressBar("收尾", "Lua文件还原中...", 0.8f);
        LuaEncode.EndEncode();
        EditorUtility.DisplayProgressBar("收尾", "多语言资源还原中...", 0.85f);
        MultiLngResDetach.Revert();
        EditorUtility.DisplayProgressBar("收尾", "清除AB名...", 0.9f);
        ABNameUtility.ClearAllABNames();
        EditorUtility.ClearProgressBar();
        
        AssetDatabase.Refresh();
        ResmapUtility.canAutoToResmap = true;
    }

    // 生成文件列表
    [MenuItem( "打包/生成文件列表", false, 351 )]
    static void BuildFileList()
    {
        ResmapUtility.canAuto2Resmap = false; //不自动导入到resmap中

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
                if( ext.Equals( ".meta" ) || ext.Equals( ".manifest" ) || ext.Equals(".DS_Store") || fileName.Equals("filelist"))
                    continue;

                // md5 值
                string md5 = CommonUtils.Md5file(file);
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

    //删除文件
    static void DeleteFiles(string pattern)
    {
        string[] files = Directory.GetFiles(ORIGINABPATH);
        for (int i = 0; i < files.Length; i++)
        {
            if(files[i].ToLower().Contains(pattern))
                File.Delete(files[i]);
        }
    }

    static void PackageAndroid()
    {
        EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ETC2; //ÉèÖÃÍ¼Æ¬Ñ¹ËõÄ£Ê½
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Internal;
    }


    //18位
    private static byte[] headBytes = {1, 2, 7, 6, 38, 1, 6, 35, 3, 5, 1, 45, 78, 20, 30, 66, 47, 45};
    //加密
    static void EncryptAB(string pattern ="")
    {
        Directory.CreateDirectory(ABPATH);
        List<byte> result = new List<byte>();
        string[] files = Directory.GetFiles(ORIGINABPATH);
        for (int i = 0; i < files.Length; i++)
        {
            string path = files[i];
            if(path.Contains(".meta") || path.Contains(".manifest")) continue;
            string fileName = Path.GetFileNameWithoutExtension(path);
            if (string.IsNullOrEmpty(pattern) || fileName.Contains(pattern))
            {
                string destPath = ABPATH + "/" + fileName.GetHashCode() + Path.GetExtension(path);
                if (File.Exists(destPath))
                    File.Delete(destPath);

                if (ResMgr.IsEncrypt(fileName)) // 加密的文件
                {
                    result.Clear();
                    result.AddRange(headBytes);
                    result.AddRange(File.ReadAllBytes(path));
                    File.WriteAllBytes(destPath, result.ToArray());
                }
                else
                {
                    File.Copy(path, destPath);
                }
            }
        }
    }

    //海外版备份并替换SDK
    static void BackupSDKOnOversea(bool isAndroid)
    {
        string tmpPath = System.Environment.CurrentDirectory + "/TmpSDK";
        if (Directory.Exists(tmpPath))
            UnityEditor.FileUtil.DeleteFileOrDirectory(tmpPath);
        Directory.CreateDirectory(tmpPath);
        if (isAndroid)
        {
            //备份
            UnityEditor.FileUtil.MoveFileOrDirectory(Application.dataPath+ "/Plugins/Android/AndroidManifest.xml", tmpPath+ "/AndroidManifest.xml");
            UnityEditor.FileUtil.MoveFileOrDirectory(Application.dataPath+ "/Plugins/Android/ltbasesdk.aar", tmpPath+ "/ltbasesdk.aar");
            UnityEditor.FileUtil.MoveFileOrDirectory(Application.dataPath+ "/Plugins/Android/assets", tmpPath+ "/assets");
            UnityEditor.FileUtil.MoveFileOrDirectory(Application.dataPath+ "/Plugins/Android/jpush.aar", tmpPath+ "/jpush.aar");
            UnityEditor.FileUtil.MoveFileOrDirectory(Application.dataPath+ "/Plugins/BuglyPlugins/Android", tmpPath+ "/BuglyAndroid");

            //替换
            UnityEditor.FileUtil.CopyFileOrDirectory(System.Environment.CurrentDirectory + "/LTBaseSDK_Oversea/Android/AndroidManifest.xml", Application.dataPath + "/Plugins/Android/AndroidManifest.xml");
            UnityEditor.FileUtil.CopyFileOrDirectory(System.Environment.CurrentDirectory + "/LTBaseSDK_Oversea/Android/ltbasesdk.aar", Application.dataPath + "/Plugins/Android/ltbasesdk.aar");
            UnityEditor.FileUtil.CopyFileOrDirectory(System.Environment.CurrentDirectory + "/LTBaseSDK_Oversea/Android/assets", Application.dataPath + "/Plugins/Android/assets");
        }
        else
        {
            //备份
            UnityEditor.FileUtil.MoveFileOrDirectory(Application.dataPath + "/Plugins/IOS/LTSDK", tmpPath + "/LTSDK");
            //替换
            UnityEditor.FileUtil.CopyFileOrDirectory(System.Environment.CurrentDirectory + "/LTBaseSDK_Oversea/IOS/LTSDK", Application.dataPath + "/Plugins/IOS/LTSDK");
        }
        AssetDatabase.Refresh();
    }

    //海外版还原SDK
    static void RevertSDKOnOversea(bool isAndroid)
    {
        string tmpPath = System.Environment.CurrentDirectory + "/TmpSDK";
        if (isAndroid)
        {
            UnityEditor.FileUtil.ReplaceFile(tmpPath + "/AndroidManifest.xml", Application.dataPath + "/Plugins/Android/AndroidManifest.xml");
            UnityEditor.FileUtil.ReplaceFile(tmpPath + "/ltbasesdk.aar", Application.dataPath + "/Plugins/Android/ltbasesdk.aar");
            UnityEditor.FileUtil.ReplaceDirectory(tmpPath + "/assets", Application.dataPath + "/Plugins/Android/assets");
            UnityEditor.FileUtil.ReplaceFile(tmpPath + "/jpush.aar", Application.dataPath + "/Plugins/Android/jpush.aar");
            UnityEditor.FileUtil.ReplaceDirectory(tmpPath + "/BuglyAndroid", Application.dataPath + "/Plugins/BuglyPlugins/Android");
        }
        else
        {
            UnityEditor.FileUtil.ReplaceDirectory(tmpPath + "/LTSDK", Application.dataPath + "/Plugins/IOS/LTSDK");
        }
        UnityEditor.FileUtil.DeleteFileOrDirectory(tmpPath);
    }


    #region 压缩
    /************************************************************************/
    // 压缩
    /************************************************************************/

    [MenuItem( "打包/压缩ZIP", false, 352 )]
    static string Package()
    {
        // 版本号
        ConfigHandler resverIni = ConfigHandler.Open( Application.streamingAssetsPath + "/version.txt" );
        int resver = int.Parse( resverIni.ReadValue( "Resource_Version", "0" ).ToString() );

        DateTime dt = System.DateTime.Now;
        string fileName = "Patcher_" + GameMain.Inst.platformName + "_" + dt.ToString( "yyyyMMddHHmm" ) + "_" + resver + ".zip";

        using( ZipFile zip = ZipFile.Create( fileName ) )
        {
            zip.BeginUpdate();
            zip.Add( "Assets/StreamingAssets/version.txt", "version.txt" );
            FileInfo[] fileList = new DirectoryInfo( ABPATH ).GetFiles();
            for( int i = 0; i < fileList.Length; i++ )
            {
                string ext = Path.GetExtension( fileList[i].Name );
                if ( ext.Equals( ".meta" ) || ext.Equals( ".manifest" ) || fileName == "app.txt" || fileName == "version.txt" )
                    continue;
                zip.Add( fileList[i].FullName, "assetbundle/" + fileList[i].Name );
            }
            zip.CommitUpdate();
        }

        return fileName;
    }
#endregion

#region 脚本打包方法
    /// 获取添加场景
    static string[] GetBuildScenes()
    {
        List<string> names = new List<string>();
        foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
        {
            if (e == null)
                continue;
            if (e.enabled)
                names.Add(e.path);
        }
        return names.ToArray();
    }

    private static void ProjectSetting(string[] args)
    {
        ConfigHandler appHandler = ConfigHandler.Open(Application.streamingAssetsPath + "/config.txt");
        //server
        string str = args[args.Length - 1].Split('：')[1];
        appHandler.WriteValue("SDKType", str, true);
        appHandler.Flush(); //写入文件

        //版本号
        str = args[args.Length - 4];
        PlayerSettings.bundleVersion = str; //设置包的版本号
        ConfigHandler handler = ConfigHandler.Open(System.Environment.CurrentDirectory + "/Config.txt");
        handler.WriteValue("Force_Version", str, true);
        int versionCode = int.Parse(handler.ReadValue("Resource_Version", ""));
        versionCode++; //小版本号增加1
        handler.WriteValue("Resource_Version", versionCode, true);
        int packetCode = int.Parse(handler.ReadValue("Packet_Version", "0"));
        packetCode++; //包版本号增加1
        handler.WriteValue("Packet_Version", packetCode, true);
        handler.Flush(); //写入文件

        ConfigHandler versionHandler = ConfigHandler.Open(Application.streamingAssetsPath + "/version.txt");
        versionHandler.WriteValue("Force_Version", str, true);
        versionHandler.WriteValue("Resource_Version", versionCode, true);
        versionHandler.WriteValue("Packet_Version", packetCode, true);
        versionHandler.Flush(); //写入文件

        //开始打包
        str = args[args.Length - 5].Split('：')[1];
        if (str.Equals("0")) //全量包
        {
            BuildAll();
            EncryptAB(); //加密
            BuildFileList();
            Upload();
        }
        else if (str.Equals("1")) //只打Lua包
        {
            BuildLua();
            BuildFileList();
            Upload();
        }
        else if (str.Equals("2")) //只打Data包
        {
            BuildData();
            BuildFileList();
            Upload();
        }
        else
        {
        }
    }

    public static void BuildAndroid()
    {
        //获取打包参数
        string[] args = System.Environment.GetCommandLineArgs();
        PackageAndroid();
        ProjectSetting(args); //工程设置并打AB包

        PlayerSettings.SplashScreen.show = false;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.Auto;
        PlayerSettings.Android.keystoreName = System.Environment.CurrentDirectory + "/SDKCommon/user.keystore";
        PlayerSettings.Android.keystorePass = "328256";
        PlayerSettings.Android.keyaliasName = "520082";
        PlayerSettings.Android.keyaliasPass = "328256";
        if (bool.Parse(args[args.Length - 10])) //使用mono
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
        else
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);

        ConfigHandler versionHandler = ConfigHandler.Open(Application.streamingAssetsPath + "/version.txt");
        int pVersion = int.Parse(versionHandler.ReadValue("Packet_Version", "0"));
        PlayerSettings.Android.bundleVersionCode = pVersion;

        bool googlePackage = bool.Parse(args[args.Length - 13]);
        string oldResVer = null;
        if (googlePackage) //是否打google包
        {
            BuildGoogle();
            //降低资源版本号
            oldResVer = versionHandler.ReadValue("Resource_Version", "0");
            versionHandler.WriteValue("Resource_Version", "0");
        }

        BuildPlayerOptions opts = new BuildPlayerOptions();
        string apkName = string.Format("TheShieldHERO_Android_{0:yyyyMMddHHmm}_v{1}.{2}.{5}_{3}_{4}.apk", DateTime.Now, pVersion);
        opts.locationPathName = "Android/"+ apkName;
        opts.scenes = GetBuildScenes();
        opts.target = EditorUserBuildSettings.activeBuildTarget;
        opts.options = BuildOptions.Development;
        BuildPipeline.BuildPlayer(opts);

        if (googlePackage) //是否打google包
        {
            //还原资源版本号
            versionHandler.WriteValue("Resource_Version", oldResVer);
        }
    }

    public static void BuildIOS()
    {
        //获取打包参数
        string[] args = System.Environment.GetCommandLineArgs();
        ProjectSetting(args); //工程设置并打AB包

        PlayerSettings.SplashScreen.show = false;
        //删除旧的导出工程
        string exportPath = System.Environment.CurrentDirectory+ "/iOSBuild";
        if (Directory.Exists(exportPath))
            UnityEditor.FileUtil.DeleteFileOrDirectory(exportPath);

        PlayerSettings.iOS.hideHomeButton = true;
        PlayerSettings.iOS.appleEnableAutomaticSigning = false;

        //证书
        string str = args[args.Length - 14].Split('：')[1];
        PlayerSettings.iOS.iOSManualProvisioningProfileType = ProvisioningProfileType.Development;
        PlayerSettings.iOS.iOSManualProvisioningProfileID = "d04ceeaa-7b78-4aaf-a4b5-c12a3952d6af";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.ltgame.dzyzcml.tw");
        
        ConfigHandler versionHandler = ConfigHandler.Open(Application.streamingAssetsPath + "/version.txt");
        PlayerSettings.iOS.buildNumber = versionHandler.ReadValue("Packet_Version", "0");//IOS包每次都增加一个数字，提包需求

        BuildPlayerOptions opts = new BuildPlayerOptions();
        opts.scenes = GetBuildScenes();
        opts.locationPathName = "iOSBuild";
        opts.target = EditorUserBuildSettings.activeBuildTarget;
        opts.options = BuildOptions.Development;
        BuildPipeline.BuildPlayer(opts);
    }

#if UNITY_IOS
    //设置Capabilities
    void SetCapabilities(string pathToBuildProject)
    {
        string projPath = pathToBuildProject + "/Unity-iPhone.xcodeproj/project.pbxproj"; //项目路径，这个路径在mac上默认是不显示的，需要右键->显示包内容才能看到。unity到处的名字就是这个。
        UnityEditor.iOS.Xcode.PBXProject pbxProj = new UnityEditor.iOS.Xcode.PBXProject();//创建xcode project类
        pbxProj.ReadFromString(File.ReadAllText(projPath));//xcode project读入
        string targetGuid = pbxProj.TargetGuidByName(PBXProject.GetUnityTargetName());//获得Target名

        //设置BuildSetting
        pbxProj.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
        pbxProj.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", "-ObjC");
        pbxProj.SetBuildProperty(targetGuid, "DEBUG_INFORMATION_FORMAT", "dwarf-with-dsym"); //定位崩溃bug
        pbxProj.SetBuildProperty(targetGuid, "EXCLUDED_ARCHS", "armv7");

        pbxProj.AddFrameworkToProject(targetGuid, "MediaPlayer.framework", false);
        pbxProj.AddFrameworkToProject(targetGuid, "AdSupport.framework", true);
        //添加资源
        pbxProj.AddFileToBuild(targetGuid, pbxProj.AddFile(System.Environment.CurrentDirectory + "/LTBaseSDK_Oversea/ltgame.cfg", "Resource/ltgame.cfg", PBXSourceTree.Source));

        //修改编译方式
        string mmfile = pbxProj.FindFileGuidByProjectPath("Classes/UnityAppController.mm");
        var flags = pbxProj.GetCompileFlagsForFile(targetGuid, mmfile);
        flags.Add("-fno-objc-arc");
        pbxProj.SetCompileFlagsForFile(targetGuid, mmfile, flags);
        mmfile = pbxProj.FindFileGuidByProjectPath("Libraries/Plugins/IOS/LTSDK/LTSDKNPC.mm");
        flags = pbxProj.GetCompileFlagsForFile(targetGuid, mmfile);
        flags.Add("-fno-objc-arc");
        pbxProj.SetCompileFlagsForFile(targetGuid, mmfile, flags);
        pbxProj.WriteToFile(projPath);

        string[] splits = PlayerSettings.applicationIdentifier.Split('.');
        var capManager = new ProjectCapabilityManager(projPath, splits[splits.Length - 1] + ".entitlements", PBXProject.GetUnityTargetName());//创建设置Capability类
        if (PlayerSettings.applicationIdentifier.Equals("com.longtugame.dzyz.longtu"))
        {
            //正式包，增加计费
            capManager.AddInAppPurchase();
        }
        capManager.AddAssociatedDomains(new[]
        {
            "applinks:dy.longtugame.com"
        });
        capManager.WriteToFile();//写入文件保存
    }

    //设置info.plist文件
    void SetInfo(string pathToBuildProject)
    {
        string plistPath = pathToBuildProject + "/Info.plist";
        PlistDocument plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));
        PlistElementDict plistRoot = plist.root;

        //指定打包时使用的ProvisioningProfile
        PlistElementDict dict = plistRoot.CreateDict("provisioningProfiles");
        dict.SetString(PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS), PlayerSettings.iOS.iOSManualProvisioningProfileID);
        plistRoot.SetString("method", PlayerSettings.iOS.iOSManualProvisioningProfileType == ProvisioningProfileType.Development ? "development" : "app-store");

        plistRoot.SetString("NSCameraUsageDescription", "需要使用相机");
        plistRoot.SetString("NSCalendarsUsageDescription", "需要使用日历");
        plistRoot.SetString("NSPhotoLibraryUsageDescription", "需要使用相册");
        plistRoot.SetString("NSLocationWhenInUseUsageDescription", "需要访问地理位置");

        //龙图SDK设置URL Schemes
        PlistElementArray URLArray = plistRoot.CreateArray("CFBundleURLTypes");
        PlistElementDict elementDict = URLArray.AddDict(); ;
        elementDict.SetString("CFBundleURLName", "GoogleID");
        elementDict.CreateArray("CFBundleURLSchemes").AddString("com.googleusercontent.apps.837194912770-vt71nif9hiifrdb2hcg2l2ejc94rnloq");
        File.WriteAllText(plistPath, plist.WriteToString());
    }

    void OnPostprocessBuildIOS(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.iOS)
            return;
        string pathToBuildProject = report.summary.outputPath;
        SetCapabilities(pathToBuildProject);
        SetInfo(pathToBuildProject);
        //替换mm文件
        string targetPath = pathToBuildProject + "/Classes/UnityAppController.mm";
        if (File.Exists(targetPath)) UnityEditor.FileUtil.DeleteFileOrDirectory(targetPath);
        UnityEditor.FileUtil.CopyFileOrDirectory(System.Environment.CurrentDirectory + "/LTBaseSDK_Oversea/UnityAppController.mm", targetPath);
    }

#endif

    public void OnPostprocessBuild(BuildReport report)
    {
#if UNITY_IOS
        OnPostprocessBuildIOS(report);
#endif
    }
    public int callbackOrder
    {
        get { return 1; }
    }

#endregion

#region 上传文件
    /************************************************************************/
    // FTP
    /************************************************************************/
    static string _ftpServerIP = "ftp://192.168.101.167";
    static string _ftpUserID = "longxiang-n";
    static string _ftpPassword = "5bk2L9TZ";
    // 上传文件  
    public static void UploadFile(string localFile, string ftpPath)
    {
        if ( !File.Exists( localFile ) )
        {
            UnityEngine.Debug.LogError( "文件：“" + localFile + "” 不存在！" );
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
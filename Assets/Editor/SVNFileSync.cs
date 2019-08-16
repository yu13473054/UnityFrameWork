using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

public class SVNFileSync
{
    private static Dictionary<string, PathConfig> _configs = new Dictionary<string, PathConfig>();

    private const string ClientPath = "/amber_client/NPC";
    //根路径
    private static string _rootPath;
    private static bool isInit = false;

    public static List<OptCmdProp> cmdPropList = new List<OptCmdProp>();

    static void Init()
    {
        if(isInit) return;
        isInit = true;;
        //同步的文件路径：路径包含项目名称，需要在同一个目录中
        //同步过来后，文件夹的名称。例如把源/bb中所有的文件都同步到Test/目标/aa文件夹中
        string fullName = Directory.GetParent(Environment.CurrentDirectory).FullName;
        fullName = fullName.Replace('\\', '/');
        _rootPath = fullName.Replace(ClientPath, "") + Path.DirectorySeparatorChar;
        _configs.Add("Excel",new PathConfig("amber_doc/NPC/程序数据表/Excel表格", "amber_client/NPC/Trunk/配置表", false));
    }

    static void StartDeal(string srcDir, string dstDir, string txt)
    {
        CollectInfo(srcDir, dstDir);
        foreach (OptCmdProp cmdProp in cmdPropList)
        {
            switch (cmdProp.opt)
            {
                case Opt.New:
                    string dstPath = cmdProp.dstPath;
                    if (!cmdProp.isDir)//说明是文件
                    {
                        string dir = Path.GetDirectoryName(dstPath);
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }
                        File.Copy(cmdProp.srcPath, dstPath);
                        Debug.Log(txt + "新增文件，" + Path.GetFullPath(dstPath));
                    }
                    else
                    {
                        Directory.CreateDirectory(dstPath);
                        Debug.Log(txt + "新增文件夹，" + Path.GetFullPath(dstPath));
                    }
                    break;
                case Opt.Modify:
                    //只有文件有修改操作
                    File.Copy(cmdProp.srcPath, cmdProp.dstPath, true);
                    Debug.Log(txt + "修改文件，" + Path.GetFullPath(cmdProp.dstPath));
                    break;
                case Opt.Delete:
                    dstPath = cmdProp.dstPath;
                    if (!cmdProp.isDir) //说明是文件
                    {
                        File.Delete(dstPath);
                        Debug.Log(txt + "删除文件，" + Path.GetFullPath(dstPath));
                    }
                    else
                    {
                        Directory.Delete(dstPath, true);
                        Debug.Log(txt + "删除文件夹，" + Path.GetFullPath(dstPath));
                    }
                    break;
            }
        }
    }

    //收集文件
    static void CollectInfo(string srcDir, string dstDir)
    {
        cmdPropList.Clear();

        //对比出新增和修改的文件
        CollectAddOrMod(srcDir, dstDir);
        //对比出删除的文件
        CollectDel(srcDir, dstDir);
    }

    //对比出删除的文件和文件夹
    private static void CollectDel(string srcDir, string dstDir)
    {
        string[] childDirPaths = Directory.GetDirectories(dstDir);
        string[] files = Directory.GetFiles(dstDir);

        //空文件夹
        if (files.Length == 0 && childDirPaths.Length == 0)
        {
            string srcPath = dstDir.Replace(dstDir, srcDir);
            if (!Directory.Exists(srcPath))
            {
                cmdPropList.Add(new OptCmdProp()
                {
                    dstPath = dstDir,
                    isDir = true,
                    opt = Opt.Delete
                });
            }
            return;
        }

        //优先遍历文件夹：文件夹删除了，中间的文件也不需要处理
        for (int i = 0; i < childDirPaths.Length; i++)
        {
            string childDirPath = childDirPaths[i];
            string srcPath = childDirPath.Replace(dstDir, srcDir);
            if (childDirPath.Contains(".svn")) continue;
            if (Directory.Exists(srcPath))
            {
                CollectDel(srcPath, childDirPath);
            }
            else
            {
                //删除文件夹
                cmdPropList.Add(new OptCmdProp()
                {
                    dstPath = childDirPath,
                    isDir = true,
                    opt = Opt.Delete
                });
            }
        }

        //文件遍历
        for (int i = 0; i < files.Length; i++)
        {
            string filePath = files[i];
            if (filePath.Contains(".svn"))
                continue;
            //目标文件的路径
            string srcPath = filePath.Replace(dstDir, srcDir);
            if (!File.Exists(srcPath))
            {
                //删除文件
                cmdPropList.Add(new OptCmdProp()
                {
                    dstPath = filePath,
                    opt = Opt.Delete
                });
            }
        }
    }

    //对比出新增和修改的文件和文件夹
    private static void CollectAddOrMod(string srcDir, string dstDir)
    {
        //文件遍历
        string[] files = Directory.GetFiles(srcDir);
        for (int i = 0; i < files.Length; i++)
        {
            string filePath = files[i];
            if (filePath.Contains(".svn"))
                continue;
            //目标文件的路径
            string dstPath = filePath.Replace(srcDir, dstDir);
            if (File.Exists(dstPath))
            {
                //修改的文件
                if (Md5file(filePath) != Md5file(dstPath))
                {
                    cmdPropList.Add(new OptCmdProp()
                    {
                        srcPath = filePath,
                        dstPath = dstPath,
                        opt = Opt.Modify
                    });
                }
            }
            else
            {
                //新增文件
                cmdPropList.Add(new OptCmdProp()
                {
                    srcPath = filePath,
                    dstPath = dstPath,
                    opt = Opt.New
                });
            }
        }
        //文件夹遍历
        string[] childDirPaths = Directory.GetDirectories(srcDir);
        for (int i = 0; i < childDirPaths.Length; i++)
        {
            string childDirPath = childDirPaths[i];
            string dstPath = childDirPath.Replace(srcDir, dstDir);
            if (childDirPath.Contains(".svn")) continue;
            CollectAddOrMod(childDirPath, dstPath);
        }
        //空文件夹
        if (files.Length == 0 && childDirPaths.Length == 0)
        {
            string dstPath = srcDir.Replace(srcDir, dstDir);
            if (!Directory.Exists(dstPath))
            {
                cmdPropList.Add(new OptCmdProp()
                {
                    dstPath = dstPath,
                    isDir = true,
                    opt = Opt.New
                });
            }
        }

    }

    //判断两个文件的MD5值是不是相同
    private static string Md5file(string file)
    {
        try
        {
            FileStream fs = new FileStream(file, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(fs);
            fs.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("md5file() fail, error:" + ex.Message);
        }
    }

    static void Sync(PathConfig config)
    {
        //源文件夹
        string srcDir = _rootPath + config.srcPath;
        if (!Directory.Exists(srcDir))
        {
            Debug.LogError("文件夹不存在：" + srcDir);
            return;
        }
        //目标文件夹
        string dstDir = _rootPath + config.dstPath;
        if (!Directory.Exists(dstDir)) Directory.CreateDirectory(dstDir);
        StartDeal(srcDir, dstDir, "同步：");
    }

    static void Commit(PathConfig config)
    {
        //源文件夹
        string srcDir = _rootPath + config.dstPath;
        if (!Directory.Exists(srcDir))
        {
            Debug.LogError("文件夹不存在：" + srcDir);
            return;
        }
        //目标文件夹
        string dstDir = _rootPath + config.srcPath;
        if (!Directory.Exists(dstDir)) Directory.CreateDirectory(dstDir);
        StartDeal(srcDir, dstDir, "反同步：");
    }

    static void SyncList(List<string> keyList)
    {
        Init();
        List<string> addList = new List<string>();
        foreach (string key in keyList)
        {
            PathConfig config = _configs[key];
            Sync(config);
            if (!config.toResmap) continue;
            //收集需要动态到resmap的文件
            foreach (OptCmdProp cmdProp in cmdPropList)
            {
                if(cmdProp.isDir) continue; //文件夹直接跳过
                switch (cmdProp.opt)
                {
                    case Opt.New:
                        addList.Add(cmdProp.dstPath);
                        break;
                }
            }
        }
        AssetDatabase.Refresh();
        if(addList.Count>0)
            AutoResMap.ToResmapExternal(addList);
    }

    static void CommitList(List<string> keyList)
    {
        Init();
        foreach (string key in keyList)
        {
            PathConfig config = _configs[key];
            Commit(config);
        }
    }

    //[MenuItem("资源/同步所有文件(请先更新doc和art文件夹)",false, 1)]
    //static void FileSync()
    //{
    //    SyncList(new List<string>(_configs.Keys));
    //}

    //[MenuItem("资源/同步表格", false, 101)]
    //public static void ExcelSync()
    //{
    //    string[] keys = { "Excel" };
    //    SyncList(new List<string>(keys));
    //}

    //[MenuItem("资源/反同步表格", false, 102)]
    //static void ExcelCommit()
    //{
    //    string[] keys = { "Excel" };
    //    CommitList(new List<string>(keys));
    //}
}

public class PathConfig
{
    public string srcPath;
    public string dstPath;
    public bool toResmap;

    public PathConfig(string srcPath, string dstPath, bool toResmap)
    {
        this.srcPath = srcPath;
        this.dstPath = dstPath;
        this.toResmap = toResmap;
    }
}

public class OptCmdProp
{
    public string key;
    public string srcPath;
    public string dstPath;
    public bool isDir;
    public Opt opt;
}

public enum Opt
{
    None,
    New,
    Delete,
    Modify
}

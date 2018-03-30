using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ResourceUpdate : MonoBehaviour
{
    public Action onShowUpdateUI;
    public Action onResourceInited;
    public Action onShowForceUpdate; //强更提示
    public Action<int,int> onUnZipRes;//释放本地资源提示

    const string VersionFileName = "version.txt";
    const string FileListName = "files.txt";

    private List<DownloadTaskInfo> _downloadFiles = new List<DownloadTaskInfo>();

    void Start()
    {
        StartCoroutine(BeginUpdate());    //启动释放协程
    }

    ///开始检查AB包
    IEnumerator BeginUpdate()
    {
        string resDir = AppConst.localABPath;  // 本地存储的ab目录

        if (!Directory.Exists(resDir))
            Directory.CreateDirectory(resDir);

        yield return CheckVersion();
    }

    ///进行版本对比
    private IEnumerator CheckVersion()
    {
        string localVerFile = AppConst.localABPath + VersionFileName;
        ConfigHandler localVerConfig;
        int localVer;

        string appVerFile = AppConst.appABPath + VersionFileName;
        if (!File.Exists(localVerFile)) //本地不存在版本文件：被删除或者第一次安装
        {
            //本地做一份保存
            File.Copy(appVerFile, localVerFile, true);

            localVerConfig = ConfigHandler.Open(localVerFile);
            localVer = int.Parse(localVerConfig.ReadValue("Resource_Version"));

            yield return UnZipAB();
        }
        else
        {
            localVerConfig = ConfigHandler.Open(localVerFile);
            localVer = int.Parse(localVerConfig.ReadValue("Resource_Version"));

            ConfigHandler appVerConfig = ConfigHandler.Open(appVerFile);
            int appVer = int.Parse(appVerConfig.ReadValue("Resource_Version"));
            //包内资源版本比本地高：开发者模式或者测试模式
            if (appVer > localVer)
            {
                Debug.LogError("<ResourceUpdate> 包内版本比本地版本高！");
                //本地重新做一份保存
                File.Copy(appVerFile, localVerFile, true);
                yield return UnZipAB();

                //重新赋值
                localVerConfig = appVerConfig;
                localVer = appVer;
            }
        }

        //判断网络
        if (!CommonUtils.NetAvailable)
        {
            //todo 可以考虑使用触发器,触发未联网提示，同时需要注册联网后重新走更新提示
            Debug.LogError("<ResourceUpdate> 检查版本前处于未联网状态！");
            yield break;
        }

        // 拿远端版本
        WWW www = new WWW(AppConst.updateHost + AppConst.platformName + "/version.txt");
        yield return www;
        if (www.error != null)
        {
            Debug.LogError("<ResourceUpdate> 无法获取远端资源版本：" + www.error);
            yield break;
        }
        ConfigHandler remoteVerConfig = new ConfigHandler();
        remoteVerConfig.Parser(www.bytes);
        // 资源版本
        int remoteVer = int.Parse(remoteVerConfig.ReadValue("Resource_Version"));

        // 是否要强更？
        float forceVer = float.Parse(remoteVerConfig.ReadValue("Force_Version"));
        float localForceVer = float.Parse(Application.version);
        if (forceVer > localForceVer)
        {
            Debug.Log("<ResourceUpdate> 去商店下载最新版本！强更程序版本：" + forceVer);
            if (onShowForceUpdate != null) onShowForceUpdate();
            yield break;
        }
        
        //远端版本低于本地版本的情况不存在

       if (remoteVer > localVer)
        {
            //本地做一份保存
            File.WriteAllBytes(AppConst.localABPath+VersionFileName,www.bytes);
        }

        //检查是否需要更新，防止更新进行到一半时，断网了
        yield return CheckOutUpdateFiles(forceVer, remoteVer);
    }

    /// <summary>
    /// 释放包内资源
    /// </summary>
    private IEnumerator UnZipAB()
    {
        Debug.Log("<ResourceUpdate> 开始释放app内部资源！");

        string localFileListPath = AppConst.localABPath + FileListName;
        string appFileListPath = AppConst.appABPath + FileListName;
        File.Copy(appFileListPath, localFileListPath,true);

        //释放所有文件到数据目录
        string[] fileList = File.ReadAllLines(localFileListPath);
        for (int i = 0; i < fileList.Length; i++)
        {
            if (onUnZipRes != null)
            {
                onUnZipRes(i, fileList.Length);
            }
            string fileDesc = fileList[i];
            string fileName = fileDesc.Split('|')[0];
            File.Copy(AppConst.appABPath+fileName, AppConst.localABPath+fileName, true);
            yield return null;
        }
        Debug.Log("<ResourceUpdate> 解包完成!!!");
    }

    /// 检查出需要更新的文件
    IEnumerator CheckOutUpdateFiles(float forceVer, int remoteVer)
    {
        // todo 更新过程中，每秒都需要进行网络检查
//        StartCoroutine(OnCheckNetwork());

        // 下载地址
        string downloadURL = AppConst.updateHost + AppConst.platformName + "/" + forceVer + remoteVer + "/";
        string resDir = AppConst.localABPath;

        string fileNameNoExt = FileListName.TrimEnd(".txt".ToCharArray());

        // 远端资源文件表
        // 获取服务器文件列表
        WWW fileListWWW = new WWW(downloadURL + fileNameNoExt);
        yield return fileListWWW;
        if (fileListWWW.error != null)
        {
            Debug.LogError("<Update> 无法获取远端文件列表：" + fileListWWW.error);
            yield break;
        }
        AssetBundle remoteFilelistAB = AssetBundle.LoadFromMemory(fileListWWW.bytes);
        //解析出那些资源需要更新
        string newFileText = remoteFilelistAB.LoadAsset<TextAsset>("filelist").text;
        remoteFilelistAB.Unload(true);

        string[] files = newFileText.Split('\n');
        for (int i = 0; i < files.Length; i++)
        {
            if (string.IsNullOrEmpty(files[i])) continue;
            string[] keyValue = files[i].Split('|');
            string newFileName = keyValue[0];
            string newFilePath = (resDir + newFileName).Trim();
            bool canUpdate = true;
            if (File.Exists(newFilePath))
            {//该文件已经在本地存在
                string remoteMd5 = keyValue[1].Trim();
                string localMd5 = CommonUtils.Md5file(newFilePath);
                canUpdate = !remoteMd5.Equals(localMd5);
            }
            if (canUpdate)
            {
                string newFileUrl = downloadURL+newFileName;
                _downloadFiles.Add(new DownloadTaskInfo()
                {
                    url = newFileUrl,
                    filePath = newFilePath
                });
            }
        }
        yield return UpdateFile();
    }

    IEnumerator UpdateFile()
    {
        for (int i = 0; i < _downloadFiles.Count; i++)
        {
            DownloadTaskInfo taskInfo = _downloadFiles[i];
            Debug.Log("downloading>>" + taskInfo.url);
            WWW www = new WWW(taskInfo.url); yield return www;
            if (www.error != null)
            {
//                OnUpdateFailed(taskInfo.filePath);
            }
            else
            {
                File.WriteAllBytes(taskInfo.filePath, www.bytes);
                if (onShowUpdateUI!=null)
                {
                    onShowUpdateUI();
                }
            }
            yield return null;
        }

        if (onResourceInited != null)
        {
            onResourceInited();
        }
    }

}
public class DownloadTaskInfo
{
    public string url;
    public string filePath;
}

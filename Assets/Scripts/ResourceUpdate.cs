using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class ResourceUpdate : MonoBehaviour
{
    public Action onShowUpdateUI;
    public Action onUpdateEnd;
    public Action onShowForceUpdate; //强更提示
    public Action<int, int> onUnZipRes;//释放本地资源提示

    const string VersionFileName = "version.txt";
    const string FileListName = "filelist";

    private List<DownloadTaskInfo> _downloadFiles = new List<DownloadTaskInfo>();

    void Start()
    {
        //-----开始检查AB包--------
        string resDir = GameMain.Inst.localABPath;  // 本地存储的ab目录

        if (!Directory.Exists(resDir))
            Directory.CreateDirectory(resDir);

        StartCoroutine(CheckVersion()); 
    }

    ///进行版本对比
    private IEnumerator CheckVersion()
    {
        string localVerFile = GameMain.Inst.localABPath + VersionFileName;
        ConfigHandler localVerConfig;
        int localVer;

        string appVerFile = GameMain.Inst.appABPath.Replace("/assetbundle", "") + VersionFileName;
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
        UnityWebRequest request = UnityWebRequest.Get(GameMain.Inst.updateHost + GameMain.Inst.platformName + "/version.txt");
        yield return request;
        if (request.error != null)
        {
            Debug.LogError("<ResourceUpdate> 无法获取远端资源版本：" + request.error);
            yield break;
        }
        ConfigHandler remoteVerConfig = new ConfigHandler();
        remoteVerConfig.Parser(request.downloadHandler.data);
        // 资源版本
        string remoteVerStr = remoteVerConfig.ReadValue("Resource_Version");
        int remoteVerInt = int.Parse(remoteVerStr);

        // 是否要强更？
        string remoteForceVerStr = remoteVerConfig.ReadValue("Force_Version");
        float remoteForceVerFloat = float.Parse(remoteForceVerStr);
        float localForceVer = float.Parse(Application.version);
        if (remoteForceVerFloat > localForceVer)
        {
            Debug.Log("<ResourceUpdate> 去商店下载最新版本！强更程序版本：" + remoteForceVerFloat);
            if (onShowForceUpdate != null) onShowForceUpdate();
            yield break;
        }

        //远端版本低于本地版本的情况不存在
        if (remoteVerInt > localVer)
        {
            //本地做一份保存
            File.WriteAllBytes(GameMain.Inst.localABPath + VersionFileName, request.downloadHandler.data);
        }

        //检查是否需要更新，防止更新进行到一半时，断网了
        yield return CheckOutUpdateFiles(remoteForceVerStr, remoteVerStr);

        //todo 优化：不用每次都从服务器端获取filelist文件，可以在版本号相同时，
        // todo  用本地的filelist文件进行判断，ab包和本地filelist中的是否匹配，如果不匹配，再从远端获取更新
    }

    /// <summary>
    /// 释放包内资源
    /// </summary>
    private IEnumerator UnZipAB()
    {
        Debug.Log("<ResourceUpdate> 开始释放app内部资源！");

        string appFileListPath = GameMain.Inst.appABPath + FileListName;
        //释放所有文件到数据目录
        AssetBundle localFileListAB = AssetBundle.LoadFromFile(appFileListPath);
        string[] fileList = localFileListAB
            .LoadAsset<TextAsset>(FileListName)
            .text.Split('\n');
        localFileListAB.Unload(true);
        for (int i = 0; i < fileList.Length; i++)
        {
            if (onUnZipRes != null)
            {
                onUnZipRes(i, fileList.Length);
            }
            string fileDesc = fileList[i];
            string fileName = fileDesc.Split('|')[0];
            File.Copy(GameMain.Inst.appABPath + fileName, GameMain.Inst.localABPath + fileName, true);
            yield return null;
        }
        Debug.Log("<ResourceUpdate> 解包完成!!!");
    }

    /// 检查出需要更新的文件
    IEnumerator CheckOutUpdateFiles(string forceVer, string remoteVer)
    {
        // todo 更新过程中，每秒都需要进行网络检查
        //        StartCoroutine(OnCheckNetwork());

        // 下载地址
        string downloadURL = GameMain.Inst.updateHost + GameMain.Inst.platformName + "/" + forceVer + "." + remoteVer + "/";
        string resDir = GameMain.Inst.localABPath;

        // 获取服务器文件列表
        UnityWebRequest request = UnityWebRequest.Get(downloadURL + FileListName);
        yield return request;
        if (request.error != null)
        {
            Debug.LogError("<Update> 无法获取远端文件列表：" + request.error);
            yield break;
        }
        AssetBundle remoteFilelistAB = AssetBundle.LoadFromMemory(request.downloadHandler.data);
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
                string newFileUrl = downloadURL + newFileName;
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
            UnityWebRequest request = UnityWebRequest.Get(taskInfo.url);
            yield return request;
            if (request.error != null)
            {
                //                OnUpdateFailed(taskInfo.filePath);
            }
            else
            {
                File.WriteAllBytes(taskInfo.filePath, request.downloadHandler.data);
                if (onShowUpdateUI != null)
                {
                    onShowUpdateUI();
                }
            }
            yield return null;
        }

        if (onUpdateEnd != null)
        {
            onUpdateEnd();
        }
    }

}
public class DownloadTaskInfo
{
    public string url;
    public string filePath;
}

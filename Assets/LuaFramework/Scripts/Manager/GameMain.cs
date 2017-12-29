using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace LuaFramework
{
    public class GameMain : MonoBehaviour
    {
        #region 初始化
        private static GameMain _inst;
        public static GameMain Inst
        {
            get { return _inst; }
        }
        #endregion

        #region 游戏属性
        [SerializeField]
        private int _targetFrameRate = AppConst.GameFrameRate;
        public bool _developMode = false;       
        #endregion

        private List<DownloadTaskInfo> _downloadFiles = new List<DownloadTaskInfo>();

        /// <summary>
        /// 初始化游戏管理器
        /// </summary>
        void Awake()
        {
            _inst = this;
            DontDestroyOnLoad(gameObject);  //防止销毁自己

            //游戏属性设置
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Application.targetFrameRate = _targetFrameRate;
        }

        void Start()
        {
            //开始游戏逻辑
            Boot();
        }

        void Boot()
        {
            CheckExtractResource(); //释放资源
        }

        public void ReBoot()
        {
            MgrRelease();
        }

        /// 释放资源
        public void CheckExtractResource()
        {
            if (!_developMode)
            {
                bool isExists = File.Exists(MyUtils.ResDir() + "files.txt");
                if (isExists)
                {
                    StartCoroutine(CheckOutUpdateFiles());
                    return;   //文件已经解压过了，自己可添加检查文件列表逻辑
                }
                StartCoroutine(OnExtractResource());    //启动释放协程
            }
            else
            {
                OnResourceInited();
            }
        }

        ///将游戏包中的ab包从StreamAssets中解压到DataPath中
        IEnumerator OnExtractResource()
        {
            string appReserveResDir = MyUtils.AppReserveResDir(); //打在游戏包中资源目录
            string resDir = MyUtils.ResDir();  //数据目录

            if (Directory.Exists(resDir)) Directory.Delete(resDir, true);
            Directory.CreateDirectory(resDir);

            string infile = appReserveResDir + "files.txt";
            string outfile = resDir + "files.txt";
            if (File.Exists(outfile)) File.Delete(outfile);

            Debug.Log("正在解包文件:>files.txt");//此时可以通知UI做显示
            if (Application.platform == RuntimePlatform.Android) {
                WWW www = new WWW(infile);
                yield return www;
                if (www.isDone) {
                    File.WriteAllBytes(outfile, www.bytes);
                }
            }
            else 
                File.Copy(infile, outfile, true);
            yield return new WaitForEndOfFrame();

            //释放所有文件到数据目录
            string[] files = File.ReadAllLines(outfile);
            foreach (var file in files)
            {
                string[] fs = file.Split('|');
                infile = appReserveResDir + fs[0];
                outfile = resDir + fs[0];
                Debug.Log("正在解包文件:>" + infile);//此时可以通知UI做显示

                //后续可以把ab资源也分文件夹管理，现在暂时全都直接存储在根目录下
                //                string dir = Path.GetDirectoryName(outfile);
                //                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                if (File.Exists(outfile))
                {
                    File.Delete(outfile);
                }
                if (Application.platform == RuntimePlatform.Android) {
                    WWW www = new WWW(infile);
                    yield return www;

                    if (www.isDone) {
                        File.WriteAllBytes(outfile, www.bytes);
                    }
                }
                else
                {
                    File.Copy(infile, outfile, true);
                }
                yield return new WaitForEndOfFrame();
            }
            Debug.Log("解包完成!!!");
            yield return new WaitForSeconds(0.1f);

            //释放完成，开始启动更新资源
            yield return CheckOutUpdateFiles();
        }

        /// 检查出需要更新的文件
        IEnumerator CheckOutUpdateFiles()
        {
            if (!AppConst.UpdateMode)
            {
                OnResourceInited();
                yield break;
            }
            string resDir = MyUtils.ResDir();  //数据目录
            string random = DateTime.Now.ToString("yyyymmddhhmmss");
            string listUrl = AppConst.WebUrl + "files.txt?v=" + random;//拼接地址
            Debug.LogWarning("LoadUpdate---->>>" + listUrl);

            WWW www = new WWW(listUrl); yield return www;
            if (www.error != null)
            {
                OnUpdateFailed("获取服务器端files.txt失败！");
                yield break;
            }
            File.WriteAllBytes(resDir + "files.txt", www.bytes);//写入新的file内容
            //解析出那些资源需要更新
            string newFileText = www.text;
            string[] files = newFileText.Split('\n');
            for (int i = 0; i < files.Length; i++)
            {
                if (string.IsNullOrEmpty(files[i])) continue;
                string[] keyValue = files[i].Split('|');
                string newFileName = keyValue[0];
                string newFilePath = (resDir + newFileName).Trim();
                string newFileDir = Path.GetDirectoryName(newFilePath);
                if (!Directory.Exists(newFileDir))
                {
                    Directory.CreateDirectory(newFileDir);
                }
                string newFileUrl = AppConst.WebUrl + newFileName + "?v=" + random;
                bool canUpdate = true;
                if (File.Exists(newFilePath))
                {//该文件已经在本地存在
                    string remoteMd5 = keyValue[1].Trim();
                    string localMd5 = MyUtils.md5file(newFilePath);
                    canUpdate = !remoteMd5.Equals(localMd5);
                    if (canUpdate) File.Delete(newFilePath);
                }
                if (canUpdate)
                {
                    _downloadFiles.Add(new DownloadTaskInfo()
                    {
                        _url = newFileUrl,
                        _filePath = newFilePath
                    });
                }
            }
            yield return UpdateFile();
        }

        IEnumerator UpdateFile()
        {
            BeginDownload();
            for (int i = 0; i < _downloadFiles.Count; i++)
            {
                DownloadTaskInfo taskInfo = _downloadFiles[i];
                Debug.Log("downloading>>" + taskInfo._url);
                WWW www = new WWW(taskInfo._url); yield return www;
                if (www.error != null)
                {
                    OnUpdateFailed(taskInfo._filePath);
                }
                else
                {
                    File.WriteAllBytes(taskInfo._filePath, www.bytes);
                    OnSingleDownloadCompleted();
                }
                yield return null;
            }
            OnAllDownloadCompleted();
            OnResourceInited();
        }

        void OnUpdateFailed(string hintText)
        {
            Debug.Log("更新失败!>" + hintText);
        }

        /// 开始下载
        void BeginDownload()
        {

        }

        void OnSingleDownloadCompleted()
        {

        }

        /// 所有文件下载完成
        void OnAllDownloadCompleted()
        {
            Debug.Log("更新完成!!");
        }

        /// 资源初始化结束
        public void OnResourceInited()
        {
            MgrInit();
            OnInitialize();
        }

        private void MgrInit()
        {
            PrefMgr.Init();
            ResMgr.Init();

            DatabaseMgr.Init();
            SoundMgr.Init();
            TimerMgr.Init();
            NetworkMgr.Init();
            PoolMgr.Init();

            LuaMgr.Init();
        }

        void MgrRelease()
        {
            if (DatabaseMgr.Inst != null) DestroyImmediate(DatabaseMgr.Inst);
            if (TimerMgr.Inst != null) DestroyImmediate(TimerMgr.Inst);
            if (SoundMgr.Inst != null) DestroyImmediate(SoundMgr.Inst);
            if (PrefMgr.Inst != null) DestroyImmediate(PrefMgr.Inst);
            if (ResMgr.Inst != null) DestroyImmediate(ResMgr.Inst);
            if (NetworkMgr.Inst != null) DestroyImmediate(NetworkMgr.Inst);
            if (PoolMgr.Inst != null) DestroyImmediate(PoolMgr.Inst);
            if (LuaMgr.Inst != null) DestroyImmediate(LuaMgr.Inst);
        }

        void OnInitialize()
        {
            LuaMgr.Inst.InitStart();
//            LuaMgr.Inst.DoFile("Logic/Game");         //加载游戏
//            LuaMgr.Inst.DoFile("Logic/Network");      //加载网络
//            NetworkMgr.Inst.OnInit();                     //初始化网络
//
//
//            MyUtils.CallMethod("Game", "OnInitOK");     //初始化完成
        }

        /// 析构函数
        void OnDestroy()
        {
            if (NetworkMgr.Inst != null)
            {
                NetworkMgr.Inst.Unload();
            }
        }
    }
    public class DownloadTaskInfo
    {
        public string _url;
        public string _filePath;
    }


}
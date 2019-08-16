using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace AssetDanshari
{
    public class AssetModel
    {
        public class AssetInfo
        {
            public int id;
            public string fileRelativePath;
            public string displayName;
            public bool isRst;
            public object bindObj;

            public List<AssetInfo> children;

            public AssetInfo(int id, string fileRelativePath, string displayName)
            {
                this.id = id;
                this.fileRelativePath = fileRelativePath;
                this.displayName = displayName;
            }

            public bool hasChildren
            {
                get
                {
                    return children != null && children.Count > 0;
                }
            }

            public void AddChild(AssetInfo info)
            {
                if (children == null)
                {
                    children = new List<AssetInfo>();
                }
                children.Add(info);
            }
        }

        /// <summary>
        /// 数据根
        /// </summary>
        public List<AssetInfo> data { get; protected set; }

        private int _id = 0;

        protected string[] resDir = {
            "Assets/Localization",
            "Assets/Res",
        };
        /// <summary>
        /// 获取Asset下所有的文件
        /// </summary>
        protected List<string> GetAllFile(bool hasResFile = true)
        {
            List<string> fileList = new List<string>();
            for (int i = 0; i < resDir.Length; i++)
            {
                fileList.AddRange(AssetDanshariUtility.GetFileList(resDir[i], AssetDanshariUtility.ValidFile));
            }
            if (hasResFile)
            {
                fileList.Add(AssetDanshariUtility.Res_Obj);
                fileList.Add(AssetDanshariUtility.Res_Pref);
                fileList.Add(AssetDanshariUtility.Res_Sprite);
            }
            return fileList;
        }

        protected int GetAutoId()
        {
            return _id++;
        }

        /// <summary>
        /// 是否存在数据
        /// </summary>
        /// <returns></returns>
        public bool HasData()
        {
            return data != null;
        }

        public virtual void SetDataPaths(List<string> queryPaths)
        {
            data = null;
            _id = 0;
        }

        public AssetInfo GenAssetInfo(string assetPath)
        {
            AssetInfo info = new AssetInfo(GetAutoId(), assetPath, Path.GetFileName(assetPath));
            return info;
        }

        #region 继承的方法

        /// <summary>
        /// 再把文件转成信息列表
        /// </summary>
        protected List<AssetInfo> FileListToAssetInfos(List<string> fileList)
        {
            var assetInfos = new List<AssetInfo>();
            foreach (var file in fileList)
            {
                AssetInfo info = GenAssetInfo(file);
                assetInfos.Add(info);
            }

            return assetInfos;
        }

        #endregion

        #region  多线程执行

        private class JobFileTextSearchReplace
        {
            private string _filePath;
            private List<string> _depList;
            private Dictionary<string, int> _guidDic;
            private List<string> _pattenList;
            private bool[] _rsts;

            public ManualResetEvent doneEvent;
            public string exception;

            public JobFileTextSearchReplace(string path, List<string> pattenList, bool[] rsts)
            {
                _filePath = path;
                _pattenList = pattenList;
                _rsts = rsts;
                doneEvent = new ManualResetEvent(false);
            }

            public JobFileTextSearchReplace(string path, Dictionary<string, int> guidDic, bool[] rsts)
            {
                _filePath = path;
                _guidDic = guidDic;
                _depList = new List<string>();
                string[] depArray = AssetDatabase.GetDependencies(_filePath);
                for (int i = 0; i < depArray.Length; i++)
                {
                    if (depArray[i].Equals(_filePath)) continue;
                    _depList.Add(AssetDatabase.AssetPathToGUID(depArray[i]));
                }
                _rsts = rsts;
                doneEvent = new ManualResetEvent(false);
            }

            public void ThreadPoolCallback(System.Object threadContext)
            {
                try
                {
                    if(_pattenList != null) //resmap查询
                    {
                        string text = File.ReadAllText(_filePath);
                        for (int i = 0; i < _pattenList.Count; i++)
                        {
                            if (text.Contains(_pattenList[i]))
                            {
                                _rsts[i] = true;
                            }
                        }
                    }
                    else //常规资源依赖查询
                    {
                        for(int i = 0; i < _depList.Count; i++)
                        {
                            string key = _depList[i];
                            if(_guidDic.ContainsKey(key))
                            {
                                _rsts[_guidDic[key]] = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    exception = _filePath + "\n" + ex.Message;
                }

                doneEvent.Set();
            }
        }

        protected void ThreadDoFilesTextSearchReplace(List<string> fileList, List<string> depList, bool[][] rstList)
        {
            List<JobFileTextSearchReplace> jobList = new List<JobFileTextSearchReplace>();
            List<ManualResetEvent> eventList = new List<ManualResetEvent>();

            //生成guid字典，快速查询
            Dictionary<string, int> guidDic = new Dictionary<string, int>();
            List<string> guidList = AssetDanshariUtility.GuidFromFileList(depList);
            for(int i = 0; i < guidList.Count; i++)
            {
                guidDic[guidList[i]] = i;
            }

            Stopwatch sw = Stopwatch.StartNew();
            int numFiles = fileList.Count;
            int dealNum =0;
            for (int i = 0; i< numFiles; i++)
            {
                string path = fileList[i];
                JobFileTextSearchReplace job;
                if (path == AssetDanshariUtility.Res_Obj || path == AssetDanshariUtility.Res_Pref ||
                    path == AssetDanshariUtility.Res_Sprite)
                {
                    //如果是资源表，直接使用路径进行匹配
                    job = new JobFileTextSearchReplace(path, depList, rstList[i]);
                }
                else
                {
                    if (path.EndsWith(".txt") || path.EndsWith(".png") || path.EndsWith(".anim")
                        || path.EndsWith(".mp3") || path.EndsWith(".wav") || path.EndsWith(".shader")
                        || path.EndsWith(".tga") || path.EndsWith(".jpg"))
                    {
                        continue;
                    }
                    job = new JobFileTextSearchReplace(path, guidDic, rstList[i]);
                }
                jobList.Add(job);
                eventList.Add(job.doneEvent);
                ThreadPool.QueueUserWorkItem(job.ThreadPoolCallback);

                if (eventList.Count >= Environment.ProcessorCount || (i == numFiles-1 && eventList.Count>0))
                {
                    dealNum++;
                    WaitHandle.WaitAll(eventList.ToArray());
                    eventList.Clear();
                    AssetDanshariUtility.DisplayThreadProgressBar(numFiles, i);
                }
            }
            Debug.Log("处理次数："+dealNum+"，耗时："+sw.ElapsedMilliseconds/1000f);
            foreach (var job in jobList)
            {
                if (!string.IsNullOrEmpty(job.exception))
                {
                    Debug.LogError(job.exception);
                }
            }
        }

        #endregion
    }
}
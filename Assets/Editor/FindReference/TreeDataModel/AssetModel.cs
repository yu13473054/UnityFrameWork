using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.Security.Cryptography;
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
            public bool deleted;

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
            return data != null && data.Count > 0;
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

        private class JobFileTextSearch
        {
            private string _filePath;
            private List<string> _depList;
            private Dictionary<string, int> _guidDic;
            private List<string> _pattenList;
            private bool[] _rsts;

            public ManualResetEvent doneEvent;
            public string exception;

            public JobFileTextSearch(string path, List<string> pattenList, bool[] rsts)
            {
                _filePath = path;
                _pattenList = pattenList;
                _rsts = rsts;
                doneEvent = new ManualResetEvent(false);
            }

            public JobFileTextSearch(string path, Dictionary<string, int> guidDic, bool[] rsts)
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

        protected void ThreadDoFilesTextSearch(List<string> fileList, List<string> depList, bool[][] rstList)
        {
            List<JobFileTextSearch> jobList = new List<JobFileTextSearch>();
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
                JobFileTextSearch job;
                if (path == AssetDanshariUtility.Res_Obj || path == AssetDanshariUtility.Res_Pref ||
                    path == AssetDanshariUtility.Res_Sprite)
                {
                    //如果是资源表，直接使用路径进行匹配
                    job = new JobFileTextSearch(path, depList, rstList[i]);
                }
                else
                {
                    if (!AssetDanshariUtility.ValidFileHasRef(path))
                        continue;
                    job = new JobFileTextSearch(path, guidDic, rstList[i]);
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

        private class JobFileTextReplace
        {
            private string _filePath;
            private string _useGUID;
            private string _targetGUID;

            public ManualResetEvent doneEvent;
            public string exception;

            public JobFileTextReplace(string path, string useGUID, string targetGUID)
            {
                _filePath = path;
                _useGUID = useGUID;
                _targetGUID = targetGUID;
                doneEvent = new ManualResetEvent(false);
            }

            public void ThreadPoolCallback(System.Object threadContext)
            {
                try
                {
                    string text = File.ReadAllText(_filePath);
                    StringBuilder sb = new StringBuilder(text, text.Length * 2);
                    sb.Replace(_targetGUID, _useGUID);
                    File.WriteAllText(_filePath, sb.ToString());
                }
                catch (Exception ex)
                {
                    exception = _filePath + "\n" + ex.Message;
                }

                doneEvent.Set();
            }
        }

        protected void ThreadDoFilesTextReplace(Dictionary<string, List<string>> targetFileDic, string usePath)
        {
            List<JobFileTextReplace> jobList = new List<JobFileTextReplace>();
            List<ManualResetEvent> eventList = new List<ManualResetEvent>();

            string useGUID = AssetDatabase.AssetPathToGUID(usePath);

            int numFiles = 0;
            foreach (var pair in targetFileDic)
            {
                numFiles += pair.Value.Count;
            }

            int i = 0;
            foreach (var pair in targetFileDic)
            {
                string targetGUID = AssetDatabase.AssetPathToGUID(pair.Key);
                numFiles = pair.Value.Count;
                for (int j = 0; j < pair.Value.Count; j++)
                {
                    if(!AssetDanshariUtility.ValidFileRepeat(pair.Value[j])) continue;
                    JobFileTextReplace job = new JobFileTextReplace(pair.Value[j], useGUID, targetGUID);
                    jobList.Add(job);
                    eventList.Add(job.doneEvent);
                    ThreadPool.QueueUserWorkItem(job.ThreadPoolCallback);

                    if (eventList.Count >= Environment.ProcessorCount || (i == numFiles - 1 && eventList.Count > 0))
                    {
                        WaitHandle.WaitAll(eventList.ToArray());
                        eventList.Clear();
                        EditorUtility.DisplayProgressBar(AssetDanshariStyle.Get().progressTitle, "替换引用...", (i + 1) * 1f / numFiles);
                    }
                    i++;
                }
            }
            foreach (var job in jobList)
            {
                if (!string.IsNullOrEmpty(job.exception))
                {
                    Debug.LogError(job.exception);
                }
            }
        }

        private class JobFileMD5
        {
            private string _filePath;
            private string[] _fileDic;
            private int _index;
            public ManualResetEvent doneEvent;
            public string exception;

            public JobFileMD5(string path, int index, string[] fileDic)
            {
                _filePath = path;
                _fileDic = fileDic;
                _index = index;
                doneEvent = new ManualResetEvent(false);
            }

            public void ThreadPoolCallback(System.Object threadContext)
            {
                try
                {
                    using (var md5 = MD5.Create())
                    {
                        FileInfo fileInfo = new FileInfo(_filePath);
                        using (var stream = File.OpenRead(fileInfo.FullName))
                        {
                            _fileDic[_index] = BitConverter.ToString(md5.ComputeHash(stream)).ToLower();
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

        protected void ThreadDoFileMD5(List<string> fileList, string[] fileDic)
        {
            List<JobFileMD5> jobList = new List<JobFileMD5>();
            List<ManualResetEvent> eventList = new List<ManualResetEvent>();

            int numFiles = fileList.Count;
            for (int i = 0; i < numFiles; i++)
            {
                string path = fileList[i];
                JobFileMD5 job = new JobFileMD5(path, i, fileDic);
                jobList.Add(job);
                eventList.Add(job.doneEvent);
                ThreadPool.QueueUserWorkItem(job.ThreadPoolCallback);

                if (eventList.Count >= Environment.ProcessorCount || (i == numFiles - 1 && eventList.Count > 0))
                {
                    WaitHandle.WaitAll(eventList.ToArray());
                    eventList.Clear();
                    AssetDanshariUtility.DisplayThreadProgressBar(numFiles, i);
                }
            }
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
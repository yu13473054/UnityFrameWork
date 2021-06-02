using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AssetDanshari
{
    public class AssetRepeatModel : AssetModel
    {
        public override void SetDataPaths(List<string> queryPahts)
        {
            base.SetDataPaths(queryPahts);
            data = FileListToAssetInfos(queryPahts);

            List<string> fileList = AssetDanshariUtility.GetAllFile(AssetDanshariUtility.ValidFile);
            var rstList = AssetDanshariUtility.ResultList(fileList.Count, queryPahts.Count);
            ThreadDoFilesTextSearch(fileList, queryPahts, rstList);

            for (int i = 0; i < fileList.Count; i++)
            {
                for (int j = 0; j < queryPahts.Count; j++)
                {
                    if (rstList[i][j])
                    {
                        AssetInfo info = GenAssetInfo(fileList[i]);
                        info.isRst = true;
                        data[j].AddChild(info);
                    }
                }
            }

            string[] md5Array = new string[queryPahts.Count];
            ThreadDoFileMD5(queryPahts, md5Array);
            Dictionary<string, List<AssetInfo>> dic = new Dictionary<string, List<AssetInfo>>();
            for (int i = 0; i < md5Array.Length; i++)
            {
                string md5 = md5Array[i];
                List<AssetInfo> list = null;
                if (dic.TryGetValue(md5, out list))
                {
                    list.Add(data[i]);
                }
                else
                {
                    dic.Add(md5, new List<AssetInfo>() { data[i] });
                }
            }
            var assetInfos = new List<AssetInfo>();
            foreach (var pair in dic)
            {
                if(pair.Value.Count<2) continue;
                AssetInfo dirInfo = new AssetInfo(GetAutoId(), String.Empty, String.Format(AssetDanshariStyle.Get().duplicateGroup, pair.Value.Count()));
                for (int i = 0; i < pair.Value.Count; i++)
                {
                    dirInfo.AddChild(pair.Value[i]);
                }
                assetInfos.Add(dirInfo);
            }

            if (assetInfos.Count > 0)
            {
                data = assetInfos;
            }
            else
            {
                data = null;
            }
            EditorUtility.ClearProgressBar();
        }

        public void SetUseThis(string useFilePath, Dictionary<string, List<string>> targetFileDic)
        {
            ThreadDoFilesTextReplace(targetFileDic, useFilePath);
            EditorUtility.ClearProgressBar();
        }
    }
}
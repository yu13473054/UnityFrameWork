using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AssetDanshari
{
    public class AssetDepModel : AssetModel
    {
        public override void SetDataPaths(List<string> queryPahts)
        {
            base.SetDataPaths(queryPahts);

            data = FileListToAssetInfos(queryPahts);

            List<string> fileList = AssetDanshariUtility.GetAllFile(AssetDanshariUtility.ValidFile);
            var rstList = AssetDanshariUtility.ResultList(fileList.Count, queryPahts.Count);
            ThreadDoFilesTextSearch(fileList, queryPahts, rstList);
            
            // 根据搜索结果来挂载额外信息
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
            for (int i = data.Count - 1; i >= 0; i--)
            {
                AssetInfo info = data[i];
                if (info.hasChildren)
                    continue;
                data.RemoveAt(i);
            }
            EditorUtility.ClearProgressBar();
        }
    }
}
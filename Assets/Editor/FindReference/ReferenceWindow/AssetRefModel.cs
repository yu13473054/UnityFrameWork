using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AssetDanshari
{
    public class AssetRefModel : AssetModel
    {
        public override void SetDataPaths(List<string> queryPaths)
        {
            base.SetDataPaths(queryPaths);
            data = FileListToAssetInfos(queryPaths);

            List<string> resFileList = GetAllFile(false);
            var searchRetList = AssetDanshariUtility.ResultList(queryPaths.Count, resFileList.Count);
            ThreadDoFilesTextSearchReplace(queryPaths, resFileList, searchRetList);
            
             //隶属信息
            var spritePackingDict = new Dictionary<string, string>();
            // 根据搜索结果来挂载额外信息
            for (int i = 0; i < queryPaths.Count; i++)
            {
                for (int j = 0; j < resFileList.Count; j++)
                {
                    if (searchRetList[i][j])
                    {
                        AssetInfo info = GenAssetInfo(resFileList[j]);
                        info.isRst = true;
                        data[i].AddChild(info);
                        
                        // 隶属
                        string val;
                        if (!spritePackingDict.TryGetValue(info.fileRelativePath, out val))
                        {
                            var assetImporter = AssetImporter.GetAtPath(info.fileRelativePath);
                            TextureImporter textureImporter = assetImporter as TextureImporter;
                            if (textureImporter)
                            {
                                val = textureImporter.spritePackingTag;
                            }
                        }
                        
                        info.bindObj = val;
                    }
                }
            }
            EditorUtility.ClearProgressBar();
        }
    }
}
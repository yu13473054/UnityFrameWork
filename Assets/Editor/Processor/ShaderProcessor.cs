using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// Shader导入位置检查，必须放在Res/Shader文件夹中
/// </summary>
public class ShaderProcessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        for (int i = 0; i < importedAssets.Length; i++)
        {
            string assetPath = importedAssets[i].Replace("\\", "/");
            if(AssetDatabase.IsValidFolder(assetPath)) continue; //忽略文件夹
            if (assetPath.EndsWith(".shader") || assetPath.EndsWith(".cginc"))
            {
                if (!assetPath.Contains("Res/Shader"))
                {
                    Debug.LogError("<导入> Shader相关资源应放入Assets/Res/Shader文件夹中！!"+assetPath);
                }
            }
            else
            {
                if (assetPath.Contains("Res/Shader"))
                {
                    Debug.LogError("<导入> Assets/Res/Shader文件夹中不能放入其他资源！!" + assetPath);
                }
            }
        }
        
    }
}

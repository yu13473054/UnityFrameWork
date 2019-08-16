using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CustomProcessor : AssetPostprocessor
{
    private static bool isValid = true;
    private string[] autoDirs =
    {
        "Assets/Res/FightScene/",
    };

    void OnPostprocessSprites(Texture tex, Sprite[] sprites)
    {
        if (!isValid) return;

        bool canDeal = false;
        //只处理指定文件夹的Sprite
        for (int i = 0; i < autoDirs.Length; i++)
        {
            if (assetPath.StartsWith(autoDirs[i]))
            {
                canDeal = true;
                break;
            }
        }

        if (!canDeal) return;

        TextureImporter texImporter = (TextureImporter)assetImporter;
        if (texImporter.textureType == TextureImporterType.Sprite)
        {
            TextureImporterSettings settings = new TextureImporterSettings();
            texImporter.ReadTextureSettings(settings);
            //设置资源的导入设置为FullRect
            settings.spriteMeshType = SpriteMeshType.FullRect;
            texImporter.SetTextureSettings(settings);
        }
    }

    #region 功能开关
    [MenuItem("Assets/导入设置（停用）", true)]
    static bool IsShowPause()
    {
        return isValid;
    }
    [MenuItem("Assets/导入设置（停用）", false, 73)]
    static void Pause()
    {
        isValid = false;
    }

    [MenuItem("Assets/导入设置（启动）", true)]
    static bool IsShowReStart()
    {
        return !isValid;
    }

    [MenuItem("Assets/导入设置（启动）", false, 74)]
    static void ReStart()
    {
        isValid = true;
    }
#endregion
}

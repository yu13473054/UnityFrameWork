using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

public class TexProcessor : AssetPostprocessor
{
    class AtlasInfo
    {
        public string dir;
        public string prefix_name;
        public bool isRotation;
        public bool isTight;
        public string fullName;

        public AtlasInfo(string dir, bool isRotation, bool isTight)
        {
            this.dir = dir;
            this.isRotation = isRotation;
            this.isTight = isTight;
            if (dir.StartsWith("Assets/Res/"))
                prefix_name = dir.Replace("Assets/Res/", "").Replace("\\", "_").Replace("/", "_");
            else if (dir.StartsWith("Assets/Localization/"))
                prefix_name = dir.Replace("Assets/Localization/", "").Replace("\\", "_").Replace("/", "_");
            fullName = Path.GetFullPath(dir);
        }
    }

    /*
     * 需要自动添加Atlas的文件夹
     * 1，指定文件夹下的新增新文件夹时，自动在文件夹中添加Atlas文件
     * 2，文件夹中的文件夹中新建文件夹时，自动添加，需要将子文件夹放在List前
     *
     * 例如指定Assets/Atlas，则在该文件夹中创建新文件夹DlgLogin时，会在DlgLogin中创建Atlas文件
     * 如果需要在Assets/Atals/Common的子文件夹中也创建，需要在atlasDir中先添加Assets/Atals/Common，再添加Assets/Atlas
     * 
    */
    private static AtlasInfo[] _atlasRuleDir =
    {
        new AtlasInfo("Assets/Res/AssetsUpdate", false, false),
        new AtlasInfo("Assets/Res/Atlas/Common", false, false),
        new AtlasInfo("Assets/Res/Atlas", false, false),
        new AtlasInfo("Assets/Localization/CN/Atlas", false, false),
        new AtlasInfo("Assets/Localization/EN/Atlas", false, false),
        new AtlasInfo("Assets/Localization/JP/Atlas", false, false),
        new AtlasInfo("Assets/Localization/TC/Atlas", false, false),
        new AtlasInfo("Assets/Res/Icon", false, false),
        new AtlasInfo("Assets/Res/FightScene", true, true),
        new AtlasInfo("Assets/Res/Curiousity", true, true),
        new AtlasInfo("Assets/Res/Icon_Sync", false, false),
    };

    //资源尺寸检查：必须为4的整数倍
    static bool ChekcTexture(Texture2D texture, TextureImporter importer)
    {
        if (importer.textureType == TextureImporterType.Sprite)
        {
            if (!importer.assetPath.StartsWith("Assets/Localization") && !importer.assetPath.StartsWith("Assets/Res")) return true;
            //会自动创建SpriteAtlas文件，不需要检查
            for (int j = 0; j < _atlasRuleDir.Length; j++)
            {
                AtlasInfo atlasInfo_Rule = _atlasRuleDir[j];
                if (!importer.assetPath.StartsWith(atlasInfo_Rule.dir)) continue;
                return true;
            }
            //父级文件夹中有SpriteAtlas文件，不需要检查
            if (FindAtlasInParentDir(new FileInfo(importer.assetPath).Directory)) return true;
        }
        if (texture.width % 4 != 0 || texture.height % 4 != 0)
        {
            Debug.LogError("<导入检查> 分辨率必须为4的整数倍！" + importer.assetPath);
            return false;
        }

        return true;
    }

    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith("Assets/Localization/") && !assetPath.StartsWith("Assets/Res/"))
            return;
        TextureImporter importer = (TextureImporter)assetImporter;
        TextureImporterSettings defaultSetting = new TextureImporterSettings();
        importer.ReadTextureSettings(defaultSetting);
        defaultSetting.mipmapEnabled = false;
        defaultSetting.npotScale = TextureImporterNPOTScale.None; //不需要压缩为2的幂次方
        defaultSetting.spriteGenerateFallbackPhysicsShape = false;
        if (importer.textureType == TextureImporterType.Lightmap) //设置lightmap的属性
        {
            defaultSetting.streamingMipmaps = false;
        }
        importer.SetTextureSettings(defaultSetting);

        //IOS平台特殊的压缩设置
        TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings("iPhone");
        settings.overridden = true;
        settings.maxTextureSize = importer.maxTextureSize;
        if (importer.DoesSourceTextureHaveAlpha())
        {
            settings.format = TextureImporterFormat.ASTC_RGBA_6x6;
        }
        else
        {
            settings.format = TextureImporterFormat.ASTC_RGB_6x6;
        }
        importer.SetPlatformTextureSettings(settings);
    }

    void OnPostprocessTexture(Texture2D texture)
    {
        TextureImporter importer = (TextureImporter) assetImporter;
        //资源尺寸检查：必须为4的整数倍
        ChekcTexture(texture, importer);
    }

    static bool FindAtlasInParentDir(DirectoryInfo dirInfo)
    {
        dirInfo.FullName.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (dirInfo.Name.Equals("Assets")) return false;
        FileInfo[] infos = dirInfo.GetFiles("*.spriteatlas");
        if (infos.Length > 0) return true;
        return FindAtlasInParentDir(dirInfo.Parent);
    }

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        for (int i = 0; i < importedAssets.Length; i++)
        {
            string assetPath = importedAssets[i].Replace("\\", "/");
            if(!assetPath.StartsWith("Assets/Localization/") && !assetPath.StartsWith("Assets/Res/"))
                continue;
            if (!AssetDatabase.IsValidFolder(assetPath)) continue; //非文件夹文件
            DirectoryInfo dirAssetInfo = new DirectoryInfo(assetPath);
            for (int j = 0; j < _atlasRuleDir.Length; j++)
            {
                AtlasInfo atlasInfo_Rule = _atlasRuleDir[j];
                if(!assetPath.Equals(atlasInfo_Rule.dir) && !assetPath.StartsWith(atlasInfo_Rule.dir+"/")) continue;
                //命中目标文件夹
                if (dirAssetInfo.Parent.FullName.Equals(atlasInfo_Rule.fullName)) 
                {
                    FileInfo[] atlasFiles = dirAssetInfo.GetFiles("*.spriteatlas", SearchOption.AllDirectories);
                    FileInfo targetAtlas = null; //文件夹下直属Atlas
                    for (int k = 0; k < atlasFiles.Length; k++)
                    {
                        FileInfo file = atlasFiles[k];
                        if (targetAtlas == null)
                        {
                            if (file.Directory.FullName.Equals(dirAssetInfo.FullName))
                                targetAtlas = file;
                        }
                        else
                        {
                            file.Delete(); //删除其他的Atlas
                        }
                    }

                    string targetName =atlasInfo_Rule.prefix_name + "_" + dirAssetInfo.Name + "_Atlas.spriteatlas";
                    if (targetAtlas == null) //没有找到直属Atlas
                    {
                        AssetDatabase.CreateAsset(new SpriteAtlas(), assetPath +"/"+ targetName);
                        targetAtlas = new FileInfo(dirAssetInfo.FullName +"/"+ targetName);
                    }
                    else
                    {
                        AssetDatabase.RenameAsset(assetPath + "/" + targetAtlas.Name, targetName); //改名
                        targetAtlas = new FileInfo(dirAssetInfo.FullName + "/" + targetName);
                    }
                    //对Atlas进行设置
                    SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(assetPath+"/"+targetAtlas.Name);
                    SpriteAtlasPackingSettings packingSettings = atlas.GetPackingSettings();
                    packingSettings.enableRotation = atlasInfo_Rule.isRotation;
                    packingSettings.enableTightPacking = atlasInfo_Rule.isTight;
                    packingSettings.padding = 2;
                    atlas.SetPackingSettings(packingSettings);

                    TextureImporterPlatformSettings platformSettings = atlas.GetPlatformSettings("iPhone");
                    platformSettings.overridden = true;
                    if ( platformSettings.maxTextureSize > 2048 )
                        platformSettings.maxTextureSize = 2048;
                    platformSettings.format = TextureImporterFormat.ASTC_RGBA_6x6;
                    atlas.SetPlatformSettings(platformSettings);

                    atlas.Remove(atlas.GetPackables());
                    atlas.Add(new []{AssetDatabase.LoadAssetAtPath<Object>(assetPath)});
                }
                break;
            }
        }
        
    }

    [MenuItem("Assets/检查Atlas", false, 131)]
    static void Pause()
    {
        DefaultAsset[] selecte = Selection.GetFiltered<DefaultAsset>(SelectionMode.DeepAssets);
        string[] tmpList = new string[selecte.Length];
        for (int i = 0; i < selecte.Length; i++)
        {
            string objPath = AssetDatabase.GetAssetPath(selecte[i]);
            tmpList[i] = objPath;
        }
        OnPostprocessAllAssets(tmpList, null, null, null);
        Debug.Log("Atlas检查完毕！");
    }

    [MenuItem("Assets/检查Texture尺寸，并输出到根目录", false, 132)]
    static void Pause1()
    {
        Texture2D[] selecte = Selection.GetFiltered<Texture2D>(SelectionMode.DeepAssets);
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < selecte.Length; i++)
        {
            string path = AssetDatabase.GetAssetPath(selecte[i]);
            bool isValid = ChekcTexture(selecte[i], (TextureImporter) AssetImporter.GetAtPath(path));
            if (!isValid)
            {
                sb.Append(path).Append("\n");
            }
        }
        string result = sb.ToString().TrimEnd('\n');
        File.WriteAllText(System.Environment.CurrentDirectory+"/TextureChecker.txt", result);
        Debug.Log("Texture尺寸检查完毕！");
    }
}

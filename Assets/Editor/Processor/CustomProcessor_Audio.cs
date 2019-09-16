using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
/// <summary>
/// 设置背景音乐
/// </summary>
public class CustomProcessor_Audio : AssetPostprocessor
{
    void OnPostprocessAudio(AudioClip clip)
    {
        if (clip.length <= 10)
        {
            //音效时长小于10s的，认为不是背景音乐
            return;
        }
        AudioImporter importer = (AudioImporter)assetImporter;
        //默认设置
        AudioImporterSampleSettings defaultSetting = importer.defaultSampleSettings;
        defaultSetting.loadType = AudioClipLoadType.CompressedInMemory;
        importer.defaultSampleSettings = defaultSetting;

        AudioImporterSampleSettings androidSetting = importer.GetOverrideSampleSettings("Android");
        androidSetting.loadType = AudioClipLoadType.Streaming;
        importer.SetOverrideSampleSettings("Android", androidSetting);
    }
}

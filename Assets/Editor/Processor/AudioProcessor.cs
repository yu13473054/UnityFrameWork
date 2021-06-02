using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AudioProcessor : AssetPostprocessor
{
    void OnPostprocessAudio(AudioClip clip)
    {
        AudioImporter importer = (AudioImporter)assetImporter;
        importer.forceToMono = true;
        AudioImporterSampleSettings defSetting = importer.defaultSampleSettings;

        if ( importer.assetPath.Contains( "Stroy" ) || importer.assetPath.Contains( "Voice" ) )
        {
            importer.preloadAudioData = false;
        }

        if (clip.length > 10)
        {
            defSetting.loadType = AudioClipLoadType.CompressedInMemory;
            importer.defaultSampleSettings = defSetting;

            AudioImporterSampleSettings androidSetting = importer.GetOverrideSampleSettings( "Android" );
            androidSetting.loadType = AudioClipLoadType.Streaming;
            importer.SetOverrideSampleSettings( "Android", androidSetting );
        }
        else
        {
            defSetting.loadType = AudioClipLoadType.DecompressOnLoad;
            defSetting.compressionFormat = AudioCompressionFormat.ADPCM;
            importer.defaultSampleSettings = defSetting;
        }
    }
}

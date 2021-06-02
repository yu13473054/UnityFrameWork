using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEditor;
using System.Diagnostics;
using System;
public class 快捷方式
{
    [MenuItem("快捷方式/暂停 _F1")]
    static void Pause()
    {
        EditorApplication.ExecuteMenuItem("Edit/Pause");
    }

    [MenuItem("快捷方式/重启 _F5")]
    static void Reboot()
    {
        SceneManager.LoadScene("Game");
    }

    [MenuItem("快捷方式/手动释放内存 _F6")]
    static void UnloadUnusedAssets()
    {
        Resources.UnloadUnusedAssets();
    }

    [MenuItem("快捷方式/GC _F7")]
    static void GC()
    {
        System.GC.Collect();
    }

    [MenuItem( "快捷方式/重载文字表 _F8" )]
    static void ReloadLocaliztion()
    {
        Localization.Reload();
    }

    [MenuItem("快捷方式/打开缓存目录 _F12")]
    static void OpenPersistentDataPath()
    {
        string path = Application.persistentDataPath;
#if UNITY_EDITOR_WIN
        string winPath = path.Replace("/", "\\"); // windows explorer doesn't like forward slashes
        System.Diagnostics.Process.Start("explorer.exe", ("/root,") + winPath);
#endif
#if UNITY_EDITOR_OSX
        string macPath = path.Replace("\\", "/"); // mac finder doesn't like backward slashes

        if ( !macPath.StartsWith("\"") )
		{
			macPath = "\"" + macPath;
		}
 
		if ( !macPath.EndsWith("\"") )
		{
			macPath = macPath + "\"";
		}
 
		string arguments = ("") + macPath;
        System.Diagnostics.Process.Start("open", arguments);
#endif
    }

}

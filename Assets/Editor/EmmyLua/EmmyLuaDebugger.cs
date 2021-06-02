using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

//调试端口必须为9966
namespace EmmyLua
{
	class EmmyLuaDebugger
    {
		[MenuItem("Tools/EmmyLua/DebuggerOn %_F10", false)]
		static void EnableService()
		{
            PlayerPrefs.SetInt("LuaDebuggerStatus", 1);
            if(!EditorApplication.isPlaying)
		        EditorApplication.ExecuteMenuItem("Edit/Play");
		}
        [MenuItem("Tools/EmmyLua/DebuggerOn %_F10", true)]
        static bool EnableServiceResult()
        {
            return PlayerPrefs.GetInt("LuaDebuggerStatus", 0) == 0;
        }

        [MenuItem("Tools/EmmyLua/DebuggerOff %_F11", false)]
        static void DisableService()
        {
            PlayerPrefs.SetInt("LuaDebuggerStatus", 0);
            if (EditorApplication.isPlaying)
                EditorApplication.ExecuteMenuItem("Edit/Play");
        }
        [MenuItem("Tools/EmmyLua/DebuggerOff %_F11", true)]
        static bool DisableServiceResult()
        {
            return PlayerPrefs.GetInt("LuaDebuggerStatus", 0) == 1;
        }
    }
}
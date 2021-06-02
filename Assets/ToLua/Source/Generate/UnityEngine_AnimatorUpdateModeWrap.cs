﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class UnityEngine_AnimatorUpdateModeWrap
{
	public static void Register(LuaState L)
	{
		L.BeginEnum(typeof(UnityEngine.AnimatorUpdateMode));
		L.RegVar("Normal", get_Normal, null);
		L.RegVar("AnimatePhysics", get_AnimatePhysics, null);
		L.RegVar("UnscaledTime", get_UnscaledTime, null);
		L.RegFunction("IntToEnum", IntToEnum);
		L.EndEnum();
		TypeTraits<UnityEngine.AnimatorUpdateMode>.Check = CheckType;
		StackTraits<UnityEngine.AnimatorUpdateMode>.Push = Push;
	}

	static void Push(IntPtr L, UnityEngine.AnimatorUpdateMode arg)
	{
		ToLua.Push(L, arg);
	}

	static bool CheckType(IntPtr L, int pos)
	{
		return TypeChecker.CheckEnumType(typeof(UnityEngine.AnimatorUpdateMode), L, pos);
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_Normal(IntPtr L)
	{
		ToLua.Push(L, UnityEngine.AnimatorUpdateMode.Normal);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_AnimatePhysics(IntPtr L)
	{
		ToLua.Push(L, UnityEngine.AnimatorUpdateMode.AnimatePhysics);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_UnscaledTime(IntPtr L)
	{
		ToLua.Push(L, UnityEngine.AnimatorUpdateMode.UnscaledTime);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int IntToEnum(IntPtr L)
	{
		int arg0 = (int)LuaDLL.lua_tonumber(L, 1);
		UnityEngine.AnimatorUpdateMode o = (UnityEngine.AnimatorUpdateMode)arg0;
		ToLua.Push(L, o);
		return 1;
	}
}


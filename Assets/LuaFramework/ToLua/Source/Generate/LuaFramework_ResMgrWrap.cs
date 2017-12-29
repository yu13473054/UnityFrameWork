﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class LuaFramework_ResMgrWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(LuaFramework.ResMgr), typeof(UnityEngine.MonoBehaviour));
		L.RegFunction("Init", Init);
		L.RegFunction("GetPrefab", GetPrefab);
		L.RegFunction("LoadAB", LoadAB);
		L.RegFunction("UnloadAB", UnloadAB);
		L.RegFunction("LoadPrefab", LoadPrefab);
		L.RegFunction("__eq", op_Equality);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.RegVar("Inst", get_Inst, null);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Init(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 0);
			LuaFramework.ResMgr.Init();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetPrefab(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			LuaFramework.ResMgr obj = (LuaFramework.ResMgr)ToLua.CheckObject<LuaFramework.ResMgr>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			UnityEngine.GameObject o = obj.GetPrefab(arg0);
			ToLua.PushSealed(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LoadAB(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			LuaFramework.ResMgr obj = (LuaFramework.ResMgr)ToLua.CheckObject<LuaFramework.ResMgr>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			UnityEngine.AssetBundle o = obj.LoadAB(arg0);
			ToLua.PushSealed(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int UnloadAB(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2)
			{
				LuaFramework.ResMgr obj = (LuaFramework.ResMgr)ToLua.CheckObject<LuaFramework.ResMgr>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				obj.UnloadAB(arg0);
				return 0;
			}
			else if (count == 3)
			{
				LuaFramework.ResMgr obj = (LuaFramework.ResMgr)ToLua.CheckObject<LuaFramework.ResMgr>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				bool arg1 = LuaDLL.luaL_checkboolean(L, 3);
				obj.UnloadAB(arg0, arg1);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: LuaFramework.ResMgr.UnloadAB");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LoadPrefab(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 4);
			LuaFramework.ResMgr obj = (LuaFramework.ResMgr)ToLua.CheckObject<LuaFramework.ResMgr>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			string[] arg1 = ToLua.CheckStringArray(L, 3);
			LuaFunction arg2 = ToLua.CheckLuaFunction(L, 4);
			obj.LoadPrefab(arg0, arg1, arg2);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int op_Equality(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			UnityEngine.Object arg0 = (UnityEngine.Object)ToLua.ToObject(L, 1);
			UnityEngine.Object arg1 = (UnityEngine.Object)ToLua.ToObject(L, 2);
			bool o = arg0 == arg1;
			LuaDLL.lua_pushboolean(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_Inst(IntPtr L)
	{
		try
		{
			ToLua.Push(L, LuaFramework.ResMgr.Inst);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}
}


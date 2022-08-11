﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class UIMgrWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(UIMgr), typeof(SingletonMono<UIMgr>));
		L.RegFunction("Open", Open);
		L.RegFunction("Close", Close);
		L.RegFunction("StackBackup", StackBackup);
		L.RegFunction("RevertBackup", RevertBackup);
		L.RegFunction("RevertTopUI", RevertTopUI);
		L.RegFunction("PopBackDlg", PopBackDlg);
		L.RegFunction("World2ScreenPos", World2ScreenPos);
		L.RegFunction("GetUIRoot", GetUIRoot);
		L.RegFunction("GetUICamera", GetUICamera);
		L.RegFunction("GetLayer", GetLayer);
		L.RegFunction("UnloadAllUI", UnloadAllUI);
		L.RegFunction("__eq", op_Equality);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.RegVar("poolRoot", get_poolRoot, null);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Open(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			UIMgr obj = (UIMgr)ToLua.CheckObject<UIMgr>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			UISystem o = obj.Open(arg0);
			ToLua.Push(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Close(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			UIMgr obj = (UIMgr)ToLua.CheckObject<UIMgr>(L, 1);
			UISystem arg0 = (UISystem)ToLua.CheckObject<UISystem>(L, 2);
			obj.Close(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int StackBackup(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2)
			{
				UIMgr obj = (UIMgr)ToLua.CheckObject<UIMgr>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				obj.StackBackup(arg0);
				return 0;
			}
			else if (count == 3)
			{
				UIMgr obj = (UIMgr)ToLua.CheckObject<UIMgr>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				bool arg1 = LuaDLL.luaL_checkboolean(L, 3);
				obj.StackBackup(arg0, arg1);
				return 0;
			}
			else if (count == 4)
			{
				UIMgr obj = (UIMgr)ToLua.CheckObject<UIMgr>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				bool arg1 = LuaDLL.luaL_checkboolean(L, 3);
				bool arg2 = LuaDLL.luaL_checkboolean(L, 4);
				obj.StackBackup(arg0, arg1, arg2);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: UIMgr.StackBackup");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int RevertBackup(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			UIMgr obj = (UIMgr)ToLua.CheckObject<UIMgr>(L, 1);
			obj.RevertBackup();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int RevertTopUI(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			UIMgr obj = (UIMgr)ToLua.CheckObject<UIMgr>(L, 1);
			obj.RevertTopUI();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int PopBackDlg(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			UIMgr obj = (UIMgr)ToLua.CheckObject<UIMgr>(L, 1);
			UnityEngine.GameObject o = obj.PopBackDlg();
			ToLua.PushSealed(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int World2ScreenPos(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 3);
			UIMgr obj = (UIMgr)ToLua.CheckObject<UIMgr>(L, 1);
			UnityEngine.Vector3 arg0 = ToLua.ToVector3(L, 2);
			UnityEngine.Camera arg1 = (UnityEngine.Camera)ToLua.CheckObject(L, 3, typeof(UnityEngine.Camera));
			UnityEngine.Vector3 o = obj.World2ScreenPos(arg0, arg1);
			ToLua.Push(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetUIRoot(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			UIMgr obj = (UIMgr)ToLua.CheckObject<UIMgr>(L, 1);
			UnityEngine.Transform o = obj.GetUIRoot();
			ToLua.Push(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetUICamera(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			UIMgr obj = (UIMgr)ToLua.CheckObject<UIMgr>(L, 1);
			UnityEngine.Camera o = obj.GetUICamera();
			ToLua.PushSealed(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetLayer(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2 && TypeChecker.CheckTypes<int>(L, 2))
			{
				UIMgr obj = (UIMgr)ToLua.CheckObject<UIMgr>(L, 1);
				int arg0 = (int)LuaDLL.lua_tonumber(L, 2);
				UnityEngine.Transform o = obj.GetLayer(arg0);
				ToLua.Push(L, o);
				return 1;
			}
			else if (count == 2 && TypeChecker.CheckTypes<UILayer>(L, 2))
			{
				UIMgr obj = (UIMgr)ToLua.CheckObject<UIMgr>(L, 1);
				UILayer arg0 = (UILayer)ToLua.ToObject(L, 2);
				UnityEngine.Transform o = obj.GetLayer(arg0);
				ToLua.Push(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: UIMgr.GetLayer");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int UnloadAllUI(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			UIMgr obj = (UIMgr)ToLua.CheckObject<UIMgr>(L, 1);
			obj.UnloadAllUI();
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
	static int get_poolRoot(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UIMgr obj = (UIMgr)o;
			UnityEngine.Transform ret = obj.poolRoot;
			ToLua.Push(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index poolRoot on a nil value");
		}
	}
}


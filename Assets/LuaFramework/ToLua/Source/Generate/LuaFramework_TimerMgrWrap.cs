﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class LuaFramework_TimerMgrWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(LuaFramework.TimerMgr), typeof(UnityEngine.MonoBehaviour));
		L.RegFunction("Init", Init);
		L.RegFunction("RegisterCDTimer", RegisterCDTimer);
		L.RegFunction("RegisterFrameTimer", RegisterFrameTimer);
		L.RegFunction("RemoveTimer", RemoveTimer);
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
			LuaFramework.TimerMgr.Init();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int RegisterCDTimer(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 4)
			{
				LuaFramework.TimerMgr obj = (LuaFramework.TimerMgr)ToLua.CheckObject<LuaFramework.TimerMgr>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				float arg1 = (float)LuaDLL.luaL_checknumber(L, 3);
				LuaFramework.TimerEleCallBack arg2 = (LuaFramework.TimerEleCallBack)ToLua.CheckDelegate<LuaFramework.TimerEleCallBack>(L, 4);
				obj.RegisterCDTimer(arg0, arg1, arg2);
				return 0;
			}
			else if (count == 5)
			{
				LuaFramework.TimerMgr obj = (LuaFramework.TimerMgr)ToLua.CheckObject<LuaFramework.TimerMgr>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				float arg1 = (float)LuaDLL.luaL_checknumber(L, 3);
				LuaFramework.TimerEleCallBack arg2 = (LuaFramework.TimerEleCallBack)ToLua.CheckDelegate<LuaFramework.TimerEleCallBack>(L, 4);
				bool arg3 = LuaDLL.luaL_checkboolean(L, 5);
				obj.RegisterCDTimer(arg0, arg1, arg2, arg3);
				return 0;
			}
			else if (count == 6)
			{
				LuaFramework.TimerMgr obj = (LuaFramework.TimerMgr)ToLua.CheckObject<LuaFramework.TimerMgr>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				float arg1 = (float)LuaDLL.luaL_checknumber(L, 3);
				LuaFramework.TimerEleCallBack arg2 = (LuaFramework.TimerEleCallBack)ToLua.CheckDelegate<LuaFramework.TimerEleCallBack>(L, 4);
				bool arg3 = LuaDLL.luaL_checkboolean(L, 5);
				bool arg4 = LuaDLL.luaL_checkboolean(L, 6);
				obj.RegisterCDTimer(arg0, arg1, arg2, arg3, arg4);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: LuaFramework.TimerMgr.RegisterCDTimer");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int RegisterFrameTimer(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 3)
			{
				LuaFramework.TimerMgr obj = (LuaFramework.TimerMgr)ToLua.CheckObject<LuaFramework.TimerMgr>(L, 1);
				float arg0 = (float)LuaDLL.luaL_checknumber(L, 2);
				LuaFramework.TimerEleCallBack arg1 = (LuaFramework.TimerEleCallBack)ToLua.CheckDelegate<LuaFramework.TimerEleCallBack>(L, 3);
				string o = obj.RegisterFrameTimer(arg0, arg1);
				LuaDLL.lua_pushstring(L, o);
				return 1;
			}
			else if (count == 4)
			{
				LuaFramework.TimerMgr obj = (LuaFramework.TimerMgr)ToLua.CheckObject<LuaFramework.TimerMgr>(L, 1);
				float arg0 = (float)LuaDLL.luaL_checknumber(L, 2);
				LuaFramework.TimerEleCallBack arg1 = (LuaFramework.TimerEleCallBack)ToLua.CheckDelegate<LuaFramework.TimerEleCallBack>(L, 3);
				bool arg2 = LuaDLL.luaL_checkboolean(L, 4);
				string o = obj.RegisterFrameTimer(arg0, arg1, arg2);
				LuaDLL.lua_pushstring(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: LuaFramework.TimerMgr.RegisterFrameTimer");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int RemoveTimer(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			LuaFramework.TimerMgr obj = (LuaFramework.TimerMgr)ToLua.CheckObject<LuaFramework.TimerMgr>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			obj.RemoveTimer(arg0);
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
			ToLua.Push(L, LuaFramework.TimerMgr.Inst);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}
}


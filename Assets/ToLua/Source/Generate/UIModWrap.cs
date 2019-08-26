﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class UIModWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(UIMod), typeof(UnityEngine.MonoBehaviour));
		L.RegFunction("OnEvent", OnEvent);
		L.RegFunction("GetSprite", GetSprite);
		L.RegFunction("GetPrefab", GetPrefab);
		L.RegFunction("GetMaterial", GetMaterial);
		L.RegFunction("__eq", op_Equality);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.RegVar("relatedGameObject", get_relatedGameObject, set_relatedGameObject);
		L.RegVar("uiName", get_uiName, set_uiName);
		L.RegVar("resModule", get_resModule, set_resModule);
		L.RegVar("isFirst", get_isFirst, null);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int OnEvent(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 4);
			UIMod obj = (UIMod)ToLua.CheckObject<UIMod>(L, 1);
			int arg0 = (int)LuaDLL.luaL_checknumber(L, 2);
			int arg1 = (int)LuaDLL.luaL_checknumber(L, 3);
			object arg2 = ToLua.ToVarObject(L, 4);
			obj.OnEvent(arg0, arg1, arg2);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetSprite(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			UIMod obj = (UIMod)ToLua.CheckObject<UIMod>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			UnityEngine.Sprite o = obj.GetSprite(arg0);
			ToLua.PushSealed(L, o);
			return 1;
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
			int count = LuaDLL.lua_gettop(L);

			if (count == 2)
			{
				UIMod obj = (UIMod)ToLua.CheckObject<UIMod>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				UnityEngine.GameObject o = obj.GetPrefab(arg0);
				ToLua.PushSealed(L, o);
				return 1;
			}
			else if (count == 3)
			{
				UIMod obj = (UIMod)ToLua.CheckObject<UIMod>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				bool arg1 = LuaDLL.luaL_checkboolean(L, 3);
				UnityEngine.GameObject o = obj.GetPrefab(arg0, arg1);
				ToLua.PushSealed(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: UIMod.GetPrefab");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetMaterial(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			UIMod obj = (UIMod)ToLua.CheckObject<UIMod>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			UnityEngine.Material o = obj.GetMaterial(arg0);
			ToLua.Push(L, o);
			return 1;
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
	static int get_relatedGameObject(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UIMod obj = (UIMod)o;
			UnityEngine.GameObject[] ret = obj.relatedGameObject;
			ToLua.Push(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index relatedGameObject on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_uiName(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UIMod obj = (UIMod)o;
			string ret = obj.uiName;
			LuaDLL.lua_pushstring(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index uiName on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_resModule(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UIMod obj = (UIMod)o;
			string ret = obj.resModule;
			LuaDLL.lua_pushstring(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index resModule on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_isFirst(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UIMod obj = (UIMod)o;
			bool ret = obj.isFirst;
			LuaDLL.lua_pushboolean(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index isFirst on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_relatedGameObject(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UIMod obj = (UIMod)o;
			UnityEngine.GameObject[] arg0 = ToLua.CheckObjectArray<UnityEngine.GameObject>(L, 2);
			obj.relatedGameObject = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index relatedGameObject on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_uiName(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UIMod obj = (UIMod)o;
			string arg0 = ToLua.CheckString(L, 2);
			obj.uiName = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index uiName on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_resModule(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UIMod obj = (UIMod)o;
			string arg0 = ToLua.CheckString(L, 2);
			obj.resModule = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index resModule on a nil value");
		}
	}
}

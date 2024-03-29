﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class UIModWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(UIMod), typeof(UnityEngine.MonoBehaviour));
		L.RegFunction("OnEvent", OnEvent);
		L.RegFunction("Open", Open);
		L.RegFunction("Close", Close);
		L.RegFunction("GetTexture", GetTexture);
		L.RegFunction("GetObject", GetObject);
		L.RegFunction("GetSprite", GetSprite);
		L.RegFunction("GetPrefab", GetPrefab);
		L.RegFunction("GetMaterial", GetMaterial);
		L.RegFunction("__eq", op_Equality);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.RegVar("uiName", get_uiName, set_uiName);
		L.RegVar("resUtility", get_resUtility, set_resUtility);
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
	static int Open(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			UIMod obj = (UIMod)ToLua.CheckObject<UIMod>(L, 1);
			obj.Open();
			return 0;
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
			ToLua.CheckArgsCount(L, 1);
			UIMod obj = (UIMod)ToLua.CheckObject<UIMod>(L, 1);
			obj.Close();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetTexture(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2)
			{
				UIMod obj = (UIMod)ToLua.CheckObject<UIMod>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				UnityEngine.Texture o = obj.GetTexture(arg0);
				ToLua.Push(L, o);
				return 1;
			}
			else if (count == 3)
			{
				UIMod obj = (UIMod)ToLua.CheckObject<UIMod>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				bool arg1 = LuaDLL.luaL_checkboolean(L, 3);
				UnityEngine.Texture o = obj.GetTexture(arg0, arg1);
				ToLua.Push(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: UIMod.GetTexture");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetObject(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2)
			{
				UIMod obj = (UIMod)ToLua.CheckObject<UIMod>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				UnityEngine.Object o = obj.GetObject(arg0);
				ToLua.Push(L, o);
				return 1;
			}
			else if (count == 3)
			{
				UIMod obj = (UIMod)ToLua.CheckObject<UIMod>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				bool arg1 = LuaDLL.luaL_checkboolean(L, 3);
				UnityEngine.Object o = obj.GetObject(arg0, arg1);
				ToLua.Push(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: UIMod.GetObject");
			}
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
			int count = LuaDLL.lua_gettop(L);

			if (count == 2)
			{
				UIMod obj = (UIMod)ToLua.CheckObject<UIMod>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				UnityEngine.Sprite o = obj.GetSprite(arg0);
				ToLua.PushSealed(L, o);
				return 1;
			}
			else if (count == 3)
			{
				UIMod obj = (UIMod)ToLua.CheckObject<UIMod>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				bool arg1 = LuaDLL.luaL_checkboolean(L, 3);
				UnityEngine.Sprite o = obj.GetSprite(arg0, arg1);
				ToLua.PushSealed(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: UIMod.GetSprite");
			}
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
			else if (count == 4)
			{
				UIMod obj = (UIMod)ToLua.CheckObject<UIMod>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				bool arg1 = LuaDLL.luaL_checkboolean(L, 3);
				bool arg2 = LuaDLL.luaL_checkboolean(L, 4);
				UnityEngine.GameObject o = obj.GetPrefab(arg0, arg1, arg2);
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
			int count = LuaDLL.lua_gettop(L);

			if (count == 2)
			{
				UIMod obj = (UIMod)ToLua.CheckObject<UIMod>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				UnityEngine.Material o = obj.GetMaterial(arg0);
				ToLua.Push(L, o);
				return 1;
			}
			else if (count == 3)
			{
				UIMod obj = (UIMod)ToLua.CheckObject<UIMod>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				bool arg1 = LuaDLL.luaL_checkboolean(L, 3);
				UnityEngine.Material o = obj.GetMaterial(arg0, arg1);
				ToLua.Push(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: UIMod.GetMaterial");
			}
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
	static int get_resUtility(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UIMod obj = (UIMod)o;
			ResModuleUtility ret = obj.resUtility;
			ToLua.Push(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index resUtility on a nil value");
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
	static int set_resUtility(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UIMod obj = (UIMod)o;
			ResModuleUtility arg0 = (ResModuleUtility)ToLua.CheckObject<ResModuleUtility>(L, 2);
			obj.resUtility = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index resUtility on a nil value");
		}
	}
}


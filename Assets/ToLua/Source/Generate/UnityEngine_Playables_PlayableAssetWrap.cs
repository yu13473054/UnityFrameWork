﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class UnityEngine_Playables_PlayableAssetWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(UnityEngine.Playables.PlayableAsset), typeof(UnityEngine.ScriptableObject));
		L.RegFunction("CreatePlayable", CreatePlayable);
		L.RegFunction("__eq", op_Equality);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.RegVar("duration", get_duration, null);
		L.RegVar("outputs", get_outputs, null);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CreatePlayable(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 3);
			UnityEngine.Playables.PlayableAsset obj = (UnityEngine.Playables.PlayableAsset)ToLua.CheckObject<UnityEngine.Playables.PlayableAsset>(L, 1);
			UnityEngine.Playables.PlayableGraph arg0 = StackTraits<UnityEngine.Playables.PlayableGraph>.Check(L, 2);
			UnityEngine.GameObject arg1 = (UnityEngine.GameObject)ToLua.CheckObject(L, 3, typeof(UnityEngine.GameObject));
			UnityEngine.Playables.Playable o = obj.CreatePlayable(arg0, arg1);
			ToLua.PushValue(L, o);
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
	static int get_duration(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Playables.PlayableAsset obj = (UnityEngine.Playables.PlayableAsset)o;
			double ret = obj.duration;
			LuaDLL.lua_pushnumber(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index duration on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_outputs(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Playables.PlayableAsset obj = (UnityEngine.Playables.PlayableAsset)o;
			System.Collections.Generic.IEnumerable<UnityEngine.Playables.PlayableBinding> ret = obj.outputs;
			ToLua.PushObject(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index outputs on a nil value");
		}
	}
}


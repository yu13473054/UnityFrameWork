﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class UnityEngine_CanvasWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(UnityEngine.Canvas), typeof(UnityEngine.Behaviour));
		L.RegFunction("GetDefaultCanvasMaterial", GetDefaultCanvasMaterial);
		L.RegFunction("GetETC1SupportedCanvasMaterial", GetETC1SupportedCanvasMaterial);
		L.RegFunction("ForceUpdateCanvases", ForceUpdateCanvases);
		L.RegFunction("New", _CreateUnityEngine_Canvas);
		L.RegFunction("__eq", op_Equality);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.RegVar("renderMode", get_renderMode, set_renderMode);
		L.RegVar("isRootCanvas", get_isRootCanvas, null);
		L.RegVar("pixelRect", get_pixelRect, null);
		L.RegVar("scaleFactor", get_scaleFactor, set_scaleFactor);
		L.RegVar("referencePixelsPerUnit", get_referencePixelsPerUnit, set_referencePixelsPerUnit);
		L.RegVar("overridePixelPerfect", get_overridePixelPerfect, set_overridePixelPerfect);
		L.RegVar("pixelPerfect", get_pixelPerfect, set_pixelPerfect);
		L.RegVar("planeDistance", get_planeDistance, set_planeDistance);
		L.RegVar("renderOrder", get_renderOrder, null);
		L.RegVar("overrideSorting", get_overrideSorting, set_overrideSorting);
		L.RegVar("sortingOrder", get_sortingOrder, set_sortingOrder);
		L.RegVar("targetDisplay", get_targetDisplay, set_targetDisplay);
		L.RegVar("sortingLayerID", get_sortingLayerID, set_sortingLayerID);
		L.RegVar("cachedSortingLayerValue", get_cachedSortingLayerValue, null);
		L.RegVar("additionalShaderChannels", get_additionalShaderChannels, set_additionalShaderChannels);
		L.RegVar("sortingLayerName", get_sortingLayerName, set_sortingLayerName);
		L.RegVar("rootCanvas", get_rootCanvas, null);
		L.RegVar("worldCamera", get_worldCamera, set_worldCamera);
		L.RegVar("normalizedSortingGridSize", get_normalizedSortingGridSize, set_normalizedSortingGridSize);
		L.RegVar("willRenderCanvases", get_willRenderCanvases, set_willRenderCanvases);
		L.RegFunction("WillRenderCanvases", UnityEngine_Canvas_WillRenderCanvases);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreateUnityEngine_Canvas(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 0)
			{
				UnityEngine.Canvas obj = new UnityEngine.Canvas();
				ToLua.PushSealed(L, obj);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to ctor method: UnityEngine.Canvas.New");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetDefaultCanvasMaterial(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 0);
			UnityEngine.Material o = UnityEngine.Canvas.GetDefaultCanvasMaterial();
			ToLua.Push(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetETC1SupportedCanvasMaterial(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 0);
			UnityEngine.Material o = UnityEngine.Canvas.GetETC1SupportedCanvasMaterial();
			ToLua.Push(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ForceUpdateCanvases(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 0);
			UnityEngine.Canvas.ForceUpdateCanvases();
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
	static int get_renderMode(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			UnityEngine.RenderMode ret = obj.renderMode;
			ToLua.Push(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index renderMode on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_isRootCanvas(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			bool ret = obj.isRootCanvas;
			LuaDLL.lua_pushboolean(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index isRootCanvas on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_pixelRect(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			UnityEngine.Rect ret = obj.pixelRect;
			ToLua.PushValue(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index pixelRect on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_scaleFactor(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			float ret = obj.scaleFactor;
			LuaDLL.lua_pushnumber(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index scaleFactor on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_referencePixelsPerUnit(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			float ret = obj.referencePixelsPerUnit;
			LuaDLL.lua_pushnumber(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index referencePixelsPerUnit on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_overridePixelPerfect(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			bool ret = obj.overridePixelPerfect;
			LuaDLL.lua_pushboolean(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index overridePixelPerfect on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_pixelPerfect(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			bool ret = obj.pixelPerfect;
			LuaDLL.lua_pushboolean(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index pixelPerfect on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_planeDistance(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			float ret = obj.planeDistance;
			LuaDLL.lua_pushnumber(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index planeDistance on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_renderOrder(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			int ret = obj.renderOrder;
			LuaDLL.lua_pushinteger(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index renderOrder on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_overrideSorting(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			bool ret = obj.overrideSorting;
			LuaDLL.lua_pushboolean(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index overrideSorting on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_sortingOrder(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			int ret = obj.sortingOrder;
			LuaDLL.lua_pushinteger(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index sortingOrder on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_targetDisplay(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			int ret = obj.targetDisplay;
			LuaDLL.lua_pushinteger(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index targetDisplay on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_sortingLayerID(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			int ret = obj.sortingLayerID;
			LuaDLL.lua_pushinteger(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index sortingLayerID on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_cachedSortingLayerValue(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			int ret = obj.cachedSortingLayerValue;
			LuaDLL.lua_pushinteger(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index cachedSortingLayerValue on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_additionalShaderChannels(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			UnityEngine.AdditionalCanvasShaderChannels ret = obj.additionalShaderChannels;
			ToLua.Push(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index additionalShaderChannels on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_sortingLayerName(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			string ret = obj.sortingLayerName;
			LuaDLL.lua_pushstring(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index sortingLayerName on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_rootCanvas(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			UnityEngine.Canvas ret = obj.rootCanvas;
			ToLua.PushSealed(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index rootCanvas on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_worldCamera(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			UnityEngine.Camera ret = obj.worldCamera;
			ToLua.PushSealed(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index worldCamera on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_normalizedSortingGridSize(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			float ret = obj.normalizedSortingGridSize;
			LuaDLL.lua_pushnumber(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index normalizedSortingGridSize on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_willRenderCanvases(IntPtr L)
	{
		ToLua.Push(L, new EventObject(typeof(UnityEngine.Canvas.WillRenderCanvases)));
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_renderMode(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			UnityEngine.RenderMode arg0 = (UnityEngine.RenderMode)ToLua.CheckObject(L, 2, typeof(UnityEngine.RenderMode));
			obj.renderMode = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index renderMode on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_scaleFactor(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			float arg0 = (float)LuaDLL.luaL_checknumber(L, 2);
			obj.scaleFactor = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index scaleFactor on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_referencePixelsPerUnit(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			float arg0 = (float)LuaDLL.luaL_checknumber(L, 2);
			obj.referencePixelsPerUnit = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index referencePixelsPerUnit on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_overridePixelPerfect(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			bool arg0 = LuaDLL.luaL_checkboolean(L, 2);
			obj.overridePixelPerfect = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index overridePixelPerfect on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_pixelPerfect(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			bool arg0 = LuaDLL.luaL_checkboolean(L, 2);
			obj.pixelPerfect = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index pixelPerfect on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_planeDistance(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			float arg0 = (float)LuaDLL.luaL_checknumber(L, 2);
			obj.planeDistance = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index planeDistance on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_overrideSorting(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			bool arg0 = LuaDLL.luaL_checkboolean(L, 2);
			obj.overrideSorting = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index overrideSorting on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_sortingOrder(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			int arg0 = (int)LuaDLL.luaL_checknumber(L, 2);
			obj.sortingOrder = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index sortingOrder on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_targetDisplay(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			int arg0 = (int)LuaDLL.luaL_checknumber(L, 2);
			obj.targetDisplay = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index targetDisplay on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_sortingLayerID(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			int arg0 = (int)LuaDLL.luaL_checknumber(L, 2);
			obj.sortingLayerID = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index sortingLayerID on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_additionalShaderChannels(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			UnityEngine.AdditionalCanvasShaderChannels arg0 = (UnityEngine.AdditionalCanvasShaderChannels)ToLua.CheckObject(L, 2, typeof(UnityEngine.AdditionalCanvasShaderChannels));
			obj.additionalShaderChannels = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index additionalShaderChannels on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_sortingLayerName(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			string arg0 = ToLua.CheckString(L, 2);
			obj.sortingLayerName = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index sortingLayerName on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_worldCamera(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			UnityEngine.Camera arg0 = (UnityEngine.Camera)ToLua.CheckObject(L, 2, typeof(UnityEngine.Camera));
			obj.worldCamera = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index worldCamera on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_normalizedSortingGridSize(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Canvas obj = (UnityEngine.Canvas)o;
			float arg0 = (float)LuaDLL.luaL_checknumber(L, 2);
			obj.normalizedSortingGridSize = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index normalizedSortingGridSize on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_willRenderCanvases(IntPtr L)
	{
		try
		{
			EventObject arg0 = null;

			if (LuaDLL.lua_isuserdata(L, 2) != 0)
			{
				arg0 = (EventObject)ToLua.ToObject(L, 2);
			}
			else
			{
				return LuaDLL.luaL_throw(L, "The event 'UnityEngine.Canvas.willRenderCanvases' can only appear on the left hand side of += or -= when used outside of the type 'UnityEngine.Canvas'");
			}

			if (arg0.op == EventOp.Add)
			{
				UnityEngine.Canvas.WillRenderCanvases ev = (UnityEngine.Canvas.WillRenderCanvases)arg0.func;
				UnityEngine.Canvas.willRenderCanvases += ev;
			}
			else if (arg0.op == EventOp.Sub)
			{
				UnityEngine.Canvas.WillRenderCanvases ev = (UnityEngine.Canvas.WillRenderCanvases)arg0.func;
				UnityEngine.Canvas.willRenderCanvases -= ev;
			}

			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int UnityEngine_Canvas_WillRenderCanvases(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);
			LuaFunction func = ToLua.CheckLuaFunction(L, 1);

			if (count == 1)
			{
				Delegate arg1 = DelegateTraits<UnityEngine.Canvas.WillRenderCanvases>.Create(func);
				ToLua.Push(L, arg1);
			}
			else
			{
				LuaTable self = ToLua.CheckLuaTable(L, 2);
				Delegate arg1 = DelegateTraits<UnityEngine.Canvas.WillRenderCanvases>.Create(func, self);
				ToLua.Push(L, arg1);
			}
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}
}


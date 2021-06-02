using UnityEngine;
using System.Collections;
using LuaInterface;
using System.IO;
using System;
using System.Collections.Generic;

public class LuaMgr : MonoBehaviour
{
    protected LuaState  _lua = null;
    public LuaState lua { get { return _lua; } }

    protected LuaLoader _loader;

    public static LuaMgr Inst;

    public const bool IsEncode = true;
    public string cpuTypeStr = "32_";
    protected void Awake()
    {
        // 实例化Lua State
        Inst = this;
    }

    public void Init()
    {
        _loader = new LuaLoader();
        _lua = new LuaState();
        OpenLibs();
        lua.LuaSetTop(0);
        LuaBinder.Bind( _lua );
        DelegateFactory.Init();
        LuaCoroutine.Register( _lua, this );

        // Lua读取路径
        _loader.beZip = GameMain.Inst.ResourceMode != 0;

        // 如果是打包模式，读AB，否则读Lua目录
        if (_loader.beZip)
        {
            if (IsEncode)
            {
#if UNITY_ANDROID
                if (IntPtr.Size == 8) //64位
                {
                    cpuTypeStr = "64_";
                }
#elif UNITY_IOS
                cpuTypeStr = "64_";
#endif
            }
            _loader.AddBundle("lua_" + cpuTypeStr + "logic");
            _loader.AddBundle("lua_" + cpuTypeStr + "module");
            _loader.AddBundle("lua_" + cpuTypeStr + "tolua");
            _loader.AddBundle("lua_" + cpuTypeStr + "ui");
            _loader.AddBundle("lua_" + cpuTypeStr + "utils");
        }
        else
        {
            _lua.AddSearchPath(LuaConst.luaDir);
            _lua.AddSearchPath(LuaConst.luaDir + "/ToLua");
        }

        //启动LuaVM
        _lua.Start();

        // Lua计时
        gameObject.AddComponent<LuaLooper>().luaState = _lua;

        // 添加lua之间的引用关系
        DoFile("logic/Main");
    }

    //cjson 比较特殊，只new了一个table，没有注册库，这里注册一下
    protected void OpenCJson()
    {
        lua.LuaGetField(LuaIndexes.LUA_REGISTRYINDEX, "_LOADED");
        lua.OpenLibs(LuaDLL.luaopen_cjson);
        lua.LuaSetField(-2, "cjson");

        lua.OpenLibs(LuaDLL.luaopen_cjson_safe);
        lua.LuaSetField(-2, "cjson.safe");
    }
    /// <summary>
    /// 初始化加载第三方库
    /// </summary>
    protected void OpenLibs()
    {
        _lua.OpenLibs( LuaDLL.luaopen_pb );
        _lua.OpenLibs( LuaDLL.luaopen_lpeg );
        _lua.OpenLibs( LuaDLL.luaopen_bit );
        lua.OpenLibs(LuaDLL.luaopen_socket_core);

        OpenCJson();
    }

    public void DoFile( string filename )
    {
        _lua.DoFile( filename );
    }

    public void LuaGC()
    {
        _lua.LuaGC( LuaGCOptions.LUA_GCCOLLECT );
        GC.Collect();
    }

    void OnDestroy()
    {
        if (_lua == null) return;
        _lua.Dispose();
        _lua = null;
        Debug.Log("<LuaMgr> OnDestroy!");
    }

    // 封装了下常用的Lua方法调用
    public void Call( string func )
    {
        if (_lua == null) return;
        _lua.GetFunction( func ).Call();
    }
    public void Call<T>( string func, T param )
    {
        if (_lua == null) return;
        _lua.GetFunction( func ).Call<T>( param );
    }
    public void Call<T1,T2>( string func, T1 param1, T2 param2 )
    {
        if ( _lua == null ) return;
        _lua.GetFunction( func ).Call<T1,T2>( param1, param2 );
    }
    public void Call<T1,T2,T3>( string func, T1 param1, T2 param2, T3 param3 )
    {
        if ( _lua == null ) return;
        _lua.GetFunction( func ).Call<T1,T2,T3>( param1, param2, param3 );
    }
    public R Call<R>( string func )
    {
        if (_lua == null) return default(R);
        return _lua.GetFunction( func ).Invoke<R>();
    }
    public R Call<T,R>( string func, T param )
    {
        if (_lua == null) return default(R);
        return _lua.GetFunction( func ).Invoke<T,R>( param );
    }
    public R Call<T1,T2,R>( string func, T1 param1, T2 param2 )
    {
        if ( _lua == null ) return default(R);
        return _lua.GetFunction( func ).Invoke<T1,T2,R>( param1, param2 );
    }
    public R Call<T1, T2, T3, R>( string func, T1 param1, T2 param2, T3 param3 )
    {
        if ( _lua == null ) return default( R );
        return _lua.GetFunction( func ).Invoke<T1, T2, T3, R>( param1, param2, param3 );
    }
    public LuaFunction GetFunction( string func, bool beLogMiss = true )
    {
        if (_lua == null) return null;
        return _lua.GetFunction( func, beLogMiss );
    }

    /*******************************************************************/
    // 传送一些全局事件到Lua
    LuaFunction onFocus;
    LuaFunction onPause;
    LuaFunction onQuit;
    LuaFunction onBackClick;
    public void LuaEventInit()
    {
        onFocus = GetFunction( "LuaOnFocus" );
        onQuit = GetFunction("LuaOnQuit");
        onPause = GetFunction("LuaOnPause");
        onBackClick = GetFunction("LuaOnBackClick");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) //返回键调用
        {
            if (onBackClick != null)
                onBackClick.Call();
        }
#if UNITY_EDITOR
        else if (Input.GetKeyDown(KeyCode.Home))
        {
            OnApplicationPause(true);
        }
#endif
    }

    void OnApplicationFocus( bool hasFocus )
    {
        if( onFocus != null )
            onFocus.Call( hasFocus );
    }

    void OnApplicationPause(bool pause)
    {
        if (onPause != null)
            onPause.Call(pause);
    }

    void OnApplicationQuit()
    {
        if (onQuit != null)
            onQuit.Call();
    }
}

public class LuaLoader : LuaFileUtils
{
    public LuaLoader()
    {
        instance = this;
    }

    /// <summary>
    /// 添加打入Lua代码的AssetBundle
    /// </summary>
    /// <param name="bundle"></param>
    public void AddBundle(string bundleName)
    {
        AssetBundle bundle = ResMgr.Inst.LoadAB(bundleName, "lua");
        if (bundle != null)
        {
            bundleName = bundleName.Replace("lua/", "");
            base.AddSearchBundle(bundleName.ToLower(), bundle);
        }
    }
    public override void Dispose()
    {
        if (instance != null)
        {
            instance = null;
            searchPaths.Clear();
            zipMap.Clear();
        }
    }
}
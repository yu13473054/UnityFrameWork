using UnityEngine;
using System.Collections;
using LuaInterface;
using System.IO;
using System;
using System.Collections.Generic;

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
    public void AddBundle( string bundleName )
    {
        AssetBundle bundle = ResMgr.Inst.LoadAB( bundleName, "Lua" );
        if( bundle != null )
        {
            bundleName = bundleName.Replace( "lua/", "" );
            base.AddSearchBundle( bundleName.ToLower(), bundle );
        }
    }
}

public class LuaMgr : MonoBehaviour
{
    protected LuaState  _lua = null;
    public LuaState lua { get { return _lua; } }

    protected LuaLoader _loader;

    public static LuaMgr Inst;

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

        LuaBinder.Bind( _lua );
        DelegateFactory.Register();
        LuaCoroutine.Register( _lua, this );

        // Lua读取路径
        _loader.beZip = GameMain.Inst.ResourceMode != 0;

        // 如果是打包模式，读AB，否则读Lua目录
        if (_loader.beZip)
        {
            _loader.AddBundle("lua_logic");
            _loader.AddBundle("lua_tolua");
            _loader.AddBundle("lua_module");
            _loader.AddBundle("lua_ui");
            _loader.AddBundle("lua_utils");
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

    /// <summary>
    /// 初始化加载第三方库
    /// </summary>
    protected void OpenLibs()
    {
        _lua.OpenLibs( LuaDLL.luaopen_pb );
        _lua.OpenLibs( LuaDLL.luaopen_lpeg );
        _lua.OpenLibs( LuaDLL.luaopen_bit );
        //_lua.OpenLibs( LuaDLL.luaopen_socket_core );

        //cjson 比较特殊，只new了一个table，没有注册库，这里注册一下
        _lua.LuaGetField( LuaIndexes.LUA_REGISTRYINDEX, "_LOADED" );
        _lua.OpenLibs( LuaDLL.luaopen_cjson );
        _lua.LuaSetField( -2, "cjson" );
        _lua.OpenLibs( LuaDLL.luaopen_cjson_safe );
        _lua.LuaSetField( -2, "cjson.safe" );

        _lua.LuaSetTop( 0 );
    }

    public void DoFile( string filename )
    {
        _lua.DoFile( filename );
    }

    public void LuaGC()
    {
        _lua.LuaGC( LuaGCOptions.LUA_GCCOLLECT );
    }

    void OnDestroy()
    {
#if !UNITY_EDITOR
        _lua.Dispose();
#endif
        Debug.Log("<LuaMgr> OnDestroy!");
    }

    // 封装了下常用的Lua方法调用
    public void Call( string func )
    {
        _lua.GetFunction( func ).Call();
    }
    public void Call<T>( string func, T param )
    {
        _lua.GetFunction( func ).Call<T>( param );
    }
    public R Call<R>( string func )
    {
        return _lua.GetFunction( func ).Invoke<R>();
    }
    public R Call<T,R>( string func, T param )
    {
        return _lua.GetFunction( func ).Invoke<T,R>( param );
    }
    public LuaFunction GetFunction( string func )
    {
        return _lua.GetFunction( func );
    }
}
using UnityEngine;
using System.Collections;
using LuaInterface;

public class LuaMgr : MonoBehaviour
{
    #region 初始化
    private static LuaMgr _inst;
    public static LuaMgr Inst
    {
        get { return _inst; }
    }
    #endregion

    private LuaState _lua;
    private LuaLoader _loader;
    private LuaLooper _loop;

    // Use this for initialization
    void Awake()
    {
        _inst = this;
        DontDestroyOnLoad(gameObject);

        _loader = new LuaLoader();
        _loader.beZip = GameMain.Inst.ResourceMode != 0;

        _lua = new LuaState();
        OpenLibs();
        _lua.LuaSetTop(0);

        LuaBinder.Bind(_lua);
        DelegateFactory.Init();
        LuaCoroutine.Register(_lua, this);

        InitLuaPath();
        _lua.Start();    //启动LUAVM
        _lua.DoFile("logic/Main.lua");
        _loop = gameObject.AddComponent<LuaLooper>();
        _loop.luaState = _lua;
    }

    public void InitStart()
    {
        LuaFunction main = _lua.GetFunction("LuaStart");
        main.Call();
        main.Dispose();
    }

    /// <summary>
    /// 初始化加载第三方库
    /// </summary>
    void OpenLibs()
    {
        _lua.OpenLibs(LuaDLL.luaopen_pb);
        _lua.OpenLibs(LuaDLL.luaopen_lpeg);
        _lua.OpenLibs(LuaDLL.luaopen_bit);
        _lua.OpenLibs(LuaDLL.luaopen_socket_core);

        //cjson 比较特殊，只new了一个table，没有注册库，这里注册一下
        _lua.LuaGetField(LuaIndexes.LUA_REGISTRYINDEX, "_LOADED");
        _lua.OpenLibs(LuaDLL.luaopen_cjson);
        _lua.LuaSetField(-2, "cjson");

        _lua.OpenLibs(LuaDLL.luaopen_cjson_safe);
        _lua.LuaSetField(-2, "cjson.safe");
    }

    /// <summary>
    /// 初始化Lua代码加载路径
    /// </summary>
    void InitLuaPath()
    {
        if (_loader.beZip)
        {
            _loader.AddBundle("lua_logic");
            _loader.AddBundle("lua_protocols");
            _loader.AddBundle("lua_tolua");
            _loader.AddBundle("lua_ui");
            _loader.AddBundle("lua_utils");
        }
        else
        {
            _lua.AddSearchPath(LuaConst.luaDir);
            _lua.AddSearchPath(LuaConst.luaDir+"/ToLua");
        }
    }

    public void DoFile(string filename)
    {
        _lua.DoFile(filename);
    }

    public LuaFunction GetFunction(string func)
    {
        return _lua.GetFunction(func);
    }

    public void LuaGC()
    {
        _lua.LuaGC(LuaGCOptions.LUA_GCCOLLECT);
    }

    void OnDestroy()
    {
        if (_loop)
        {
            _loop.Destroy();
            _loop = null;
        }

        if (_lua != null)
        {
            _lua.Dispose();
            _lua = null;
        }

        _loader = null;
        _inst = null;
    }
}

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace LuaFramework
{
    public class AppConst
    {
        public const int AudioClipLimit = 6; //默认同时能播放的最大音效数

        public const string ExtName = ".ab";                   //素材包扩展名
        public const string AssetDir = "StreamingAssets";           //素材包打包目录 

        #region 资源路径管理
        public const string DataDir = "assetdata/";            //数据文件夹
        public const string DataABName = "assetdata.ab";         //数据ab包

        public const string PrefabDir = "prefab/";            //预制路径
        public const string PrefabABName = "prefab.ab";         //预制ab包

        #endregion

        public const int GameFrameRate = 30;                        //游戏帧频
        public const bool LuaBundleMode = false;                    //Lua代码AssetBundle模式

        /// <summary>
        /// 如果开启更新模式，前提必须启动框架自带服务器端。
        /// 否则就需要自己将StreamingAssets里面的所有内容
        /// 复制到自己的Webserver上面，并修改下面的WebUrl。
        /// </summary>
        public const bool UpdateMode = false;                       //更新模式-默认关闭 
        public const string WebUrl = "http://localhost:6688/";      //测试更新地址





        public const bool LuaByteMode = false;                       //Lua字节码模式-默认关闭 

        public const int TimerInterval = 1;

        public const string AppName = "LuaFramework";               //应用程序名称
        public const string LuaTempDir = "Lua/";                    //临时目录

        public static string UserId = string.Empty;                 //用户ID
        public static int SocketPort = 0;                           //Socket服务器端口
        public static string SocketAddress = string.Empty;          //Socket服务器地址

        public static string FrameworkRoot
        {
            get
            {
                return Application.dataPath + "/" + AppName;
            }
        }
    }
}
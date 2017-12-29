using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LuaFramework {
    public class DatabaseMgr : MonoBehaviour {
        #region 初始化
        private static DatabaseMgr _inst;
        public static DatabaseMgr Inst
        {
            get { return _inst; }
        }

        public static void Init()
        {
            if (_inst)
            {
                return;
            }
            GameObject go = new GameObject("DatabaseMgr");
            go.AddComponent<DatabaseMgr>();
        }
        #endregion

        void Awake()
        {
            _inst = this;
            DontDestroyOnLoad(gameObject);

//            ab = ResMgr.Inst.getAB("cfgdata.ab");

            CacahData();
        }

        /// <summary>
        /// 事先缓存一些数据，防止在第一次获取到数据时，有卡顿
        /// </summary>
        void CacahData()
        {
            
        }


        void OnDestroy()
        {
//            ab = null;
        }



    }
}
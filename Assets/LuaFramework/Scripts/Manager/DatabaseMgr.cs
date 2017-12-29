using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LuaFramework {
    public class DatabaseMgr : MonoBehaviour {
        #region ��ʼ��
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
        /// ���Ȼ���һЩ���ݣ���ֹ�ڵ�һ�λ�ȡ������ʱ���п���
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
using UnityEngine;

namespace LuaFramework {

    /// <summary>
    /// </summary>
    public class Boot : MonoBehaviour
    {

        void Start() {

            SoundMgr.Init();
            TimerMgr.Init();
        }

        public void ReBoot()
        {
            if (TimerMgr.Inst!=null)
            {
                DestroyImmediate(TimerMgr.Inst);
            }
            if (SoundMgr.Inst != null)
            {
                DestroyImmediate(SoundMgr.Inst);
            }
        }
    }


}


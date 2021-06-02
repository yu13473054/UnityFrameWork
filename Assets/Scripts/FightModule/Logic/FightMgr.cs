using UnityEngine;
using System.Collections;

public class FightMgr
{
    private static FightMgr _inst;
    public static FightMgr Inst
    {
        get
        {
            if(_inst == null)
            {
                _inst = new FightMgr();
            }
            return _inst;
        }
    }

    //初始化函数
    public void Init()
    {
        PoolMgr.Inst.CreateObjPool<BulletLauncher>(null, (obj) => { obj.Release(); });
        PoolMgr.Inst.CreateObjPool<Bullet>(null, (obj) => { obj.Release(); });
        PoolMgr.Inst.CreateObjPool<BulletEffect>(null, (obj) => { obj.Release(); });
    }

    //析构函数
    private void OnDestroy()
    {
        PoolMgr.Inst.ClearObjPool<Bullet>();
        PoolMgr.Inst.ClearObjPool<BulletLauncher>();
        PoolMgr.Inst.ClearObjPool<BulletEffect>();
    }
}

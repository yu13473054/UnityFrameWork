using UnityEngine;
using System.Collections;

public class FightMgr : Singleton<FightMgr>
{
    public ObjectPool<BulletLauncher> blPool
    {
        get;
        private set;
    }
    public ObjectPool<Bullet> bulletPool {
        get;
        private set;
    }
    public ObjectPool<BulletEffect> bePool
    {
        get;
        private set;
    }
    //初始化函数
    public void Init()
    {
        blPool = new ObjectPool<BulletLauncher>(null, (obj) => { obj.Release(); });
        bulletPool = new ObjectPool<Bullet>(null, (obj) => { obj.Release(); });
        bePool = new ObjectPool<BulletEffect>(null, (obj) => { obj.Release(); });
    }

    //析构函数
    private void OnDestroy()
    {
        blPool.Clear();
        bulletPool.Clear();
        bePool.Clear();
    }
}

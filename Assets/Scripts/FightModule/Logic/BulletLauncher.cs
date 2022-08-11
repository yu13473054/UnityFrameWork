using System;
/// <summary>
/// 发射器逻辑：一个技能可能有多个发射器，所有的发射器都放置在发射者上
/// </summary>
public class BulletLauncher
{
    private Skill _skill;
    private Fight_LauncherParser _launcherParser;

    public void Init(Skill skill, int sn)
    {
        _skill = skill;

        _launcherParser = Data.Inst.Fight_Launcher(sn);
        //注册时间计时器
        TimerMgr.Inst.RegisterCDTimer(this, _launcherParser.interval, SpawnBullet, true, true, -1, _launcherParser.life, LifeEnd);
    }


    //对象被回收时调用
    public void Release()
    {
        _skill = null;
        _launcherParser = null;
    }

    //CD周期：产生子弹
    private void SpawnBullet(TimerElement element)
    {
        int type = _skill.skillParser.type;
        for(int i = 0; i< _launcherParser.num; i++)
        {
            Bullet bullet = FightMgr.Inst.bulletPool.Spawn();
            if (type == 1) //指向性技能：直接将产生的子弹放在目标位置
            {
                bullet.Init(_launcherParser.bulletSn, _skill.targetUnit.pos, _skill);
            }
            else if (type == 2) //非指向性技能：在发射器位置产生子弹
            {
                bullet.Init(_launcherParser.bulletSn, _skill.activeUnit.pos, _skill);
            }

        }
    }

    //生命周期结束
    private void LifeEnd(TimerElement element)
    {
        //回收Launcher对象。注：计时器对象不需要回收，当生命周期达到后，TimerMgr会自己回收
        FightMgr.Inst.blPool.Despawn(this);
    }
}
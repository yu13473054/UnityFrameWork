using UnityEngine;

/// <summary>
/// 子弹实体，一个发射器能产生一到多个子弹
/// </summary>
public class Bullet
{
    private Fight_BulletParser _bulletParser;
    private Vector3 _pos;
    private Skill _skill;
    private bool _isDie;

    public void Init(int sn, Vector3 startPos, Skill skill)
    {
        _pos = startPos;
        _skill = skill;
        _bulletParser = Data.Inst.Fight_Bullet(sn);
        if(_bulletParser.life < 0)
        {
            Debug.LogError("<Bullet> 子弹的生命周期不能小于0，请检查！Sn = " + sn);
            PoolMgr.Inst.DespawnObj(this);
            return;
        }
        TimerMgr.Inst.RegisterFrameTimer(Update, -1, true, true, _bulletParser.life, LifeEnd);
    }

    //对象被回收时调用
    public void Release()
    {
        _bulletParser = null;
        _skill = null;
        _isDie = false;
    }

    //更新方法：更新子弹位置等信息、检查碰撞
    private void Update(TimerElement element)
    {
        //更新子弹位置
        if (!Mathf.Approximately(_bulletParser.speed, 0))
        {

        }
        //检查碰撞
        if(_bulletParser.trigger == 1 || _bulletParser.trigger == 3)
        {
            bool result = true;//碰撞结果
            if (result)
                BulletValid();
        }
    }

    //子弹生命周期结束时调用
    private void LifeEnd(TimerElement element)
    {
        if(_bulletParser.trigger == 2 || _bulletParser.trigger == 3) //子弹生命周期结束时生效
        {
            BulletValid();
        }
    }

    //子弹生效
    public void BulletValid()
    {
        if (_isDie) return;
        PoolMgr.Inst.DespawnObj(this);//回收子弹对象
        _isDie = true; //穿透弹需要另外处理：比如碰撞后，沿该方向继续发射一个子弹

        //选择目标
        if(_bulletParser.checkType == 0) //自己
        {
            ActiveEffect(_skill.activeUnit);
        }
        else if(_bulletParser.checkType == 1)//友方AOE
        {
            float range = float.MaxValue;
            if (_bulletParser.checkValues.Length > 0)
                range = _bulletParser.checkValues[0];
            //遍历阵营中全体单位，在范围内的单位就生效
        }
        else if(_bulletParser.checkType == 2)//敌方AOE
        {
            float range = float.MaxValue;
            if (_bulletParser.checkValues.Length > 0)
                range = _bulletParser.checkValues[0];
            //遍历阵营中全体单位，在范围内的单位就生效
        }
        else if (_bulletParser.checkType == 3)//敌方单体
        {
            ActiveEffect(_skill.targetUnit);
        }
    }

    /// <summary>
    /// 激活效果
    /// </summary>
    /// <param name="target">挂效果的对象.</param>
    private void ActiveEffect(Unit target)
    {
        for (int i = 0; i < _bulletParser.effectList.Length; i++)
        {
            BulletEffect effect = PoolMgr.Inst.SpawnObj<BulletEffect>();
            effect.Init(_bulletParser.effectList[i], _skill.activeUnit, target);
        }
    }
}
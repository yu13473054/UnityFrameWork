using System.Collections.Generic;

/// <summary>
/// 技能逻辑
/// </summary>
public class Skill
{
    public enum Type
    {
        Towards,
        NotTowards
    }

    public SkillParser skillParser;
    public int cdTime;

    public Unit activeUnit { get; private set; }
    public Unit targetUnit { get; private set; }
    public float direction { get; private set; }

    public Skill(int skillSn)
    {
        skillParser = Data.Inst.Skill(skillSn);
        cdTime = skillParser.cd; //初始为未cd状态
    }

    /// <summary>
    /// 激活技能
    /// </summary>
    public void Active(Unit unit, float direction, Unit targetUnit)
    {
        this.activeUnit = unit;
        this.targetUnit = targetUnit;
        this.direction = direction;

        for (int i = 0; i < skillParser.launcherList.Length; i++)
        {
            BulletLauncher launcher = FightMgr.Inst.blPool.Spawn();
            launcher.Init(this, skillParser.launcherList[i]);
        }
    }
}
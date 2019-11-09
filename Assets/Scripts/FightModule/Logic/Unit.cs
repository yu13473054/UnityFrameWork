using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单位：只承载逻辑模块
/// 单位-》技能-》子弹发射器-》子弹-》子弹效果-》buff。从前往后，都是一对多的关系
/// </summary>
public class Unit
{
    public enum Status
    {
        Normal,
        Dizzy, //眩晕
        Death
    }

    public UnitParser unitParser;
    public Vector3 pos { get; set; }

    private UnitAttr _baseAttr, _attr;
    private Status _status;
    private List<Skill> _skills;

    public Unit(int unitSn)
    {
        unitParser = Data.Inst.Unit(unitSn);

        //基础属性：作为基础值，后续中不会修改
        _baseAttr = new UnitAttr()
        {
            hp = unitParser.hp,
            def = unitParser.def,
            atk = unitParser.atk
        };
        //当前属性
        _baseAttr = new UnitAttr(_baseAttr);
        _status = Status.Normal; //单位状态

        //初始化技能对象
        _skills = new List<Skill>();
        for(int i = 0; i< unitParser.skillList.Length; i++)
        {
            Skill skill = new Skill(unitParser.skillList[i]);
            _skills.Add(skill);
        }
    }

    /// <summary>
    /// 激活技能
    /// </summary>
    /// <param name="index">第几个技能.</param>
    public void SkillActive(int index, float direction, Unit targetUnit)
    {
        if(index >= _skills.Count)
        {
            Debug.LogErrorFormat("<Unit> 激活的技能不正确，技能个数为{0}，需要激活第{1}个技能！", _skills.Count, index + 1);
            return;
        }
        _skills[index].Active(this, direction, targetUnit);
    }
}

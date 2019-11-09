using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAttr
{
    public int atk { get; set; }
    public int hp { get; set; }
    public int def { get; set; }

    public UnitAttr() { }

    public UnitAttr(UnitAttr attr)
    {
        atk = attr.atk;
        hp = attr.hp;
        def = attr.def;
    }

    public void ChangeHP(int value)
    {
        hp += value;
    }

    public void ChangeDef(int value)
    {
        def += value;
    }

    public void ChangeAtk(int value)
    {
        atk += value;
    }
}

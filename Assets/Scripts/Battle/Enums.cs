using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Element
{
    Anemo, // 风 = 0
    Geo, // 岩 = 1
    Hydro, // 水 = 2
    Pyro, //火 = 3
    Cryo, // 冰 = 4
    Electro, //雷 = 6
    Dendro, // 草 = 6
    Physical, // 物理 = 7
    Count
}

public enum ElementBuff
{
    Frozen, // 冻结 = 0
    Catalyze, // 激化 = 1
    Count
}

public enum CharacterAttribute
{
    CriticalRate = CommonAttribute.Count,
    CriticalDmg,
    HealBonus,
    HealedBonus,
    EnergyRecharge,
    GeneralBonus,
    AnemoBonus,
    GeoBonus,
    HydroBonus,
    PyroBonus,
    CryoBonus,
    ElectroBonus,
    DendroBonus,
    PhysicalBonus,
    Taunt,
    Count
}

public enum CommonAttribute
{
    ATK,
    DEF,
    MaxHP,
    Speed,
    GeneralResist,
    AnemoResist,
    GeoResist,
    HydroResist,
    PyroResist,
    CryoResist,
    ElectroResist,
    DendroResist,
    PhysicalResist,
    Count
}

public enum EnemyAttribute
{
    Count = CommonAttribute.Count
}

public enum ValueType
{
    Percentage,
    InstantNumber,
    Count
}

public enum DamageType
{
    Attack,
    Skill,
    Burst,
    Count
}

public enum ActionType
{
    DealDamage, // 造成伤害，0
    DealHeal, // 造成治疗，1
    DealElement, // 附着元素，2
    AddBuff, // 增加 buff，3
    Count
}

public class Enums
{

}

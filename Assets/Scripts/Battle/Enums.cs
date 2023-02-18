using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Element
{
    Physical, // 物理 = 0
    Pyro,     //火 = 1
    Cryo,     // 冰 = 2
    Electro,  //雷 = 3
    Anemo,    // 风 = 4
    Quantus,  // 量子 = 5
    Imaginary,// 虚数 = 6
    Count
}

public enum Career
{
    Destroy,  // 毁灭 = 0
    Hunter,   // 巡猎 = 1
    Wisdom,   // 智识 = 2
    Resonate, // 同谐 = 3
    Void,     // 虚无 = 4
    Shield,   // 存护 = 5
    Fertile,  // 丰饶 = 6
    Count
}

public enum ElementBuff
{
    Frozen,   // 冻结 = 0
    Catalyze, // 激化 = 1
    Count
}

public enum ArtifactPosition
{
    Head,  // 头
    Hand,  // 手
    Body,  // 身体
    Foot,  // 脚
    Neck,  // 颈
    Stuff, // 饰品
    Count
}


public enum CommonAttribute
{
    ATK, // = 0
    DEF,
    MaxHP,
    Speed,
    CriticalRate,
    CriticalDamage,
    HealBonus,
    HealedBonus,
    EnergyRecharge,
    Taunt,
    BuffHit, // = 10
    BuffResist,
    GeneralBonus,
    PhysicalBonus,
    PyroBonus,
    CryoBonus,
    ElectroBonus,
    AnemoBonus,
    QuantusBonus,
    ImaginaryBonus,
    GeneralResist, // = 20
    PhysicalResist,
    PyroResist,
    CryoResist,
    ElectroResist,
    AnemoResist,
    QuantusResist,
    ImaginaryResist,
    Count // = 28
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
    DealDamage,  // 造成伤害   = 0
    DealHeal,    // 造成治疗   = 1
    DealElement, // 附着元素   = 2
    AddBuff,     // 增加 buff = 3
    Count
}

public class Enums
{

}

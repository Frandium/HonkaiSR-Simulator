using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Element
{
    Physical, // 物理 = 0
    Pyro, //火 = 1
    Cryo, // 冰 = 2
    Electro, //雷 = 3
    Anemo, // 风 = 4
    Quantus,  // 量子 = 5
    Imaginary,  // 虚数 = 6
    Count
}

public enum Career
{
    Destroy, // 毁灭
    Hunter,  // 巡猎
    Wisdom,  // 智识
    Resonate,  // 同谐
    Void,  // 虚无
    Shield,  // 存护
    Fertile,  // 丰饶
    Count
}

public enum ElementBuff
{
    Frozen, // 冻结 = 0
    Catalyze, // 激化 = 1
    Count
}

public enum ArtifactPosition
{
    Head,
    Hand,
    Body,
    Foot,
    Neck,
    Stuff,
    Count
}

public enum CharacterAttribute
{
    Count = CommonAttribute.Count
}

public enum CommonAttribute
{
    ATK,
    DEF,
    MaxHP,
    Speed,
    CriticalRate,
    CriticalDamage,
    HealBonus,
    HealedBonus,
    EnergyRecharge,
    Taunt,
    BuffHit,
    BuffResist,
    GeneralBonus,
    PhysicalBonus,
    PyroBonus,
    CryoBonus,
    ElectroBonus,
    AnemoBonus,
    QuantusBonus,
    ImaginaryBonus,
    GeneralResist,
    PhysicalResist,
    PyroResist,
    CryoResist,
    ElectroResist,
    AnemoResist,
    QuantusResist,
    ImaginaryResist,
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

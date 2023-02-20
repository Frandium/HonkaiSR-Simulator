using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Element
{
    Physical, // 物理 = 0
    Pyro,     // 火 = 1
    Cryo,     // 冰 = 2
    Electro,  // 雷 = 3
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
    ATK, // 攻击 = 0
    DEF, // 防御 = 1
    MaxHP, // 生命上限 = 2
    Speed, // 速度 = 3
    CriticalRate, // 暴击率 = 4
    CriticalDamage,  //暴击伤害 = 5
    HealBonus,  // 治疗加成 = 6
    HealedBonus, // 受治疗加成 = 7
    EnergyRecharge, // 充能效率 = 8
    Taunt, // 嘲讽权重 = 9
    BuffHit, // 效果命中 = 10
    BuffResist, // 效果抵抗 = 11
    GeneralBonus, // 通用伤害加成 = 12
    PhysicalBonus, // 物理伤害加成 = 13
    PyroBonus, // 火元素伤害加成 = 14
    CryoBonus, // 冰元素伤害加成 = 15
    ElectroBonus, // 雷元素伤害加成 = 16
    AnemoBonus, // 风元素伤害加成 = 17
    QuantusBonus, // 量子伤害加成 = 18
    ImaginaryBonus,  // 虚数伤害加成 = 19
    GeneralResist, // 通用抗性 = 20
    PhysicalResist, // 物理抗性 = 21
    PyroResist, // 火抗性 = 22
    CryoResist,  // 冰抗 = 23
    ElectroResist,  // 雷抗 = 24
    AnemoResist, // 风抗 = 25
    QuantusResist, // 量子抗性 = 26
    ImaginaryResist, // 虚数抗性 = 27
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

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


public enum StateType
{
    Frozen,   // 冻结 = 0
    Restricted, // 禁锢 = 1
    Burning, // 灼烧
    Count
}


public enum ArtifactPosition
{
    Head,  // 头
    Hand,  // 手
    Body,  // 驱赶
    Foot,  // 脚
    Rope,  // 连结绳
    Ball, // 位面球
    Count
}


public enum CommonAttribute
{
    ATK, // 攻击 = 0
    DEF, // 防御 = 1
    MaxHP, // 生命上限 = 2
    Speed, // 速度 = 3
    Taunt, // 嘲讽权重 = 4
    InstantNumberPercentageDividing, // 分界线
    CriticalRate, // 暴击率 = 5
    CriticalDamage,  //暴击伤害 = 6
    BreakBonus, // 击破特攻 = 7
    HealBonus,  // 治疗加成 = 8
    HealedBonus, // 受治疗加成 = 9
    ShieldBonus, // 造成护盾强效 = 10
    EnergyRecharge, // 充能效率 = 11
    EffectHit, // 效果命中 = 12
    EffectResist, // 效果抵抗 = 12
    GeneralBonus, // 通用伤害加成 = 13
    PhysicalBonus, // 物理伤害加成 = 14
    PyroBonus, // 火元素伤害加成 = 15
    CryoBonus, // 冰元素伤害加成 = 16
    ElectroBonus, // 雷元素伤害加成 = 17
    AnemoBonus, // 风元素伤害加成 = 18
    QuantusBonus, // 量子伤害加成 = 19
    ImaginaryBonus,  // 虚数伤害加成 = 20
    GeneralResist, // 通用抗性 = 21
    PhysicalResist, // 物理抗性 = 22
    PyroResist, // 火抗性 = 23
    CryoResist,  // 冰抗 = 24
    ElectroResist,  // 雷抗 = 25
    AnemoResist, // 风抗 = 26
    QuantusResist, // 量子抗性 = 27
    ImaginaryResist, // 虚数抗性 = 28
    PhysicalPenetrate, // 物理穿透 = 29
    PyroPenetrate,// 火元素穿透 = 30
    CryoPenetrate,// 冰元素穿透 = 31
    ElectroPenetrate,// 雷元素穿透 = 32
    AmenoPenetrate,// 风元素穿透 = 33
    QuantusPenetrate,// 量子穿透 = 34
    ImaginaryPenetrate,// 虚数穿透 = 35
    DEFIgnore, // 无视防御
    DmgDown, // 减伤
    DmgUp, // 易伤
    Count // = 28
}


public enum SelectionType
{
    Self, // 自身 = 0
    One,  // 单体 = 1
    OneExceptSelf, // 除自身外的一个 = 2
    All, // 全体 = 3
    AllExceptSelf, //除自身外全体 = 4
    OneAndNeighbour, // 单体和邻居
    Count
}


public enum TurnStage
{
    Instruction,
    Animation,
    GameEnd,
    Count
}


public enum ValueType
{
    Percentage,
    InstantNumber,
    Count
}


public enum DamageType
{
    Attack, // 普攻伤害
    Skill,  // 技能伤害
    Burst,  // 爆发伤害
    Continue, // 持续伤害
    Additional, // 追加攻击伤害
    All,
    CoAttack, // 附加伤害
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

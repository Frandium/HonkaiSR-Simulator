using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Element
{
    Physical, // ���� = 0
    Pyro,     //�� = 1
    Cryo,     // �� = 2
    Electro,  //�� = 3
    Anemo,    // �� = 4
    Quantus,  // ���� = 5
    Imaginary,// ���� = 6
    Count
}

public enum Career
{
    Destroy,  // ���� = 0
    Hunter,   // Ѳ�� = 1
    Wisdom,   // ��ʶ = 2
    Resonate, // ͬг = 3
    Void,     // ���� = 4
    Shield,   // �滤 = 5
    Fertile,  // ���� = 6
    Count
}

public enum ElementBuff
{
    Frozen,   // ���� = 0
    Catalyze, // ���� = 1
    Count
}

public enum ArtifactPosition
{
    Head,  // ͷ
    Hand,  // ��
    Body,  // ����
    Foot,  // ��
    Neck,  // ��
    Stuff, // ��Ʒ
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
    DealDamage,  // ����˺�   = 0
    DealHeal,    // �������   = 1
    DealElement, // ����Ԫ��   = 2
    AddBuff,     // ���� buff = 3
    Count
}

public class Enums
{

}

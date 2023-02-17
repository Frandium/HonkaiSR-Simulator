using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Element
{
    Physical, // ���� = 0
    Pyro, //�� = 1
    Cryo, // �� = 2
    Electro, //�� = 3
    Anemo, // �� = 4
    Quantus,  // ���� = 5
    Imaginary,  // ���� = 6
    Count
}

public enum Career
{
    Destroy, // ����
    Hunter,  // Ѳ��
    Wisdom,  // ��ʶ
    Resonate,  // ͬг
    Void,  // ����
    Shield,  // �滤
    Fertile,  // ����
    Count
}

public enum ElementBuff
{
    Frozen, // ���� = 0
    Catalyze, // ���� = 1
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
    DealDamage, // ����˺���0
    DealHeal, // ������ƣ�1
    DealElement, // ����Ԫ�أ�2
    AddBuff, // ���� buff��3
    Count
}

public class Enums
{

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Element
{
    Physical, // ���� = 0
    Pyro,     // �� = 1
    Cryo,     // �� = 2
    Electro,  // �� = 3
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
    ATK, // ���� = 0
    DEF, // ���� = 1
    MaxHP, // �������� = 2
    Speed, // �ٶ� = 3
    CriticalRate, // ������ = 4
    CriticalDamage,  //�����˺� = 5
    HealBonus,  // ���Ƽӳ� = 6
    HealedBonus, // �����Ƽӳ� = 7
    EnergyRecharge, // ����Ч�� = 8
    Taunt, // ����Ȩ�� = 9
    BuffHit, // Ч������ = 10
    BuffResist, // Ч���ֿ� = 11
    GeneralBonus, // ͨ���˺��ӳ� = 12
    PhysicalBonus, // �����˺��ӳ� = 13
    PyroBonus, // ��Ԫ���˺��ӳ� = 14
    CryoBonus, // ��Ԫ���˺��ӳ� = 15
    ElectroBonus, // ��Ԫ���˺��ӳ� = 16
    AnemoBonus, // ��Ԫ���˺��ӳ� = 17
    QuantusBonus, // �����˺��ӳ� = 18
    ImaginaryBonus,  // �����˺��ӳ� = 19
    GeneralResist, // ͨ�ÿ��� = 20
    PhysicalResist, // ������ = 21
    PyroResist, // ���� = 22
    CryoResist,  // ���� = 23
    ElectroResist,  // �׿� = 24
    AnemoResist, // �翹 = 25
    QuantusResist, // ���ӿ��� = 26
    ImaginaryResist, // �������� = 27
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

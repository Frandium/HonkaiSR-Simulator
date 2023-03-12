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


public enum StateType
{
    Frozen,   // ���� = 0
    Count
}


public enum ArtifactPosition
{
    Head,  // ͷ
    Neck,  // ��
    Body,  // ����
    Hand,  // ��
    Foot,  // ��
    Stuff, // ��Ʒ
    Count
}


public enum CommonAttribute
{
    ATK, // ���� = 0
    DEF, // ���� = 1
    MaxHP, // �������� = 2
    Speed, // �ٶ� = 3
    Taunt, // ����Ȩ�� = 4
    InstantNumberPercentageDividing, // �ֽ���
    CriticalRate, // ������ = 5
    CriticalDamage,  //�����˺� = 6
    HealBonus,  // ���Ƽӳ� = 7
    HealedBonus, // �����Ƽӳ� = 8
    ShieldBonus, // ��ɻ���ǿЧ = 9
    EnergyRecharge, // ����Ч�� = 10
    EffectHit, // Ч������ = 11
    EffectResist, // Ч���ֿ� = 12
    GeneralBonus, // ͨ���˺��ӳ� = 13
    PhysicalBonus, // �����˺��ӳ� = 14
    PyroBonus, // ��Ԫ���˺��ӳ� = 15
    CryoBonus, // ��Ԫ���˺��ӳ� = 16
    ElectroBonus, // ��Ԫ���˺��ӳ� = 17
    AnemoBonus, // ��Ԫ���˺��ӳ� = 18
    QuantusBonus, // �����˺��ӳ� = 19
    ImaginaryBonus,  // �����˺��ӳ� = 20
    GeneralResist, // ͨ�ÿ��� = 21
    PhysicalResist, // ������ = 22
    PyroResist, // ���� = 23
    CryoResist,  // ���� = 24
    ElectroResist,  // �׿� = 25
    AnemoResist, // �翹 = 26
    QuantusResist, // ���ӿ��� = 27
    ImaginaryResist, // �������� = 28
    PhysicalPenetrate, // ����͸ = 29
    PyroPenetrate,// ��Ԫ�ش�͸ = 30
    CryoPenetrate,// ��Ԫ�ش�͸ = 31
    ElectroPenetrate,// ��Ԫ�ش�͸ = 32
    AmenoPenetrate,// ��Ԫ�ش�͸ = 33
    QuantusPenetrate,// ���Ӵ�͸ = 34
    ImaginaryPenetrate,// ������͸ = 35
    Count // = 28
}


public enum SelectionType
{
    Self, // ���� = 0
    One,  // ���� = 1
    OneExceptSelf, // ���������һ�� = 2
    All, // ȫ�� = 3
    AllExceptSelf, //��������ȫ�� = 4
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
    Attack,
    Skill,
    Burst,
    Continue,
    All,
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Element
{
    Anemo, // �� = 0
    Geo, // �� = 1
    Hydro, // ˮ = 2
    Pyro, //�� = 3
    Cryo, // �� = 4
    Electro, //�� = 6
    Dendro, // �� = 6
    Physical, // ���� = 7
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
    HP,
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

public class Enums
{

}

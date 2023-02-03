using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttributeType
{
    Attack,
    Defend,
    MaxHP,
    Speed,
    GeneralBonus,
    PhysicalBonus,
    AnemoBonus,
    HydroBonus,
    CryoBonus,
    GeoBonus,
    GeneralResist,
    PhysicalResist,
    AnemoResist,
    HydroResist,
    CryoResist,
    GeoResist,
    Count
}

public class ValueBuff
{
    public AttributeType attributeType { get; protected set; } = AttributeType.Count;
    public BuffType buffType { get; protected set; } = BuffType.Debuff;
    public float value { get; protected set; } = 0;
    public int duration { get; protected set; } = 0;

    public ValueBuff(BuffType type, AttributeType att, float _value, int _duration)
    {
        buffType = type;
        attributeType = att;
        value = _value;
        duration = _duration;
    }

    public bool Progress()
    {
        duration -= 1;
        return duration <= 0;
    }
}

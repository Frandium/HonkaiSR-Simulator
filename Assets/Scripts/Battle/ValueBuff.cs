using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueBuff
{
    public int attributeType { get; protected set; } = -1;
    public BuffType buffType { get; protected set; } = BuffType.Debuff;
    public ValueType valueType { get; protected set; } = ValueType.Percentage;
    public float value { get; protected set; } = 0;
    public int duration { get; protected set; } = 0;

    public ValueBuff()
    {

    }

    public ValueBuff(BuffType type, ValueType vt, int att, float _value, int _duration)
    {
        buffType = type;
        valueType = vt;
        attributeType = att;
        value = _value;
        duration = _duration;
    }

    public ValueBuff Set(BuffType type, ValueType vt, int att, float _value, int _duration)
    {
        buffType = type;
        valueType = vt;
        attributeType = att;
        duration = _duration;
        if(valueType == ValueType.Percentage)
        {
            value = _value / 100.0f;
        }
        else
        {
            value = _value;
        }
        return this;
    }

    public bool Progress()
    {
        duration -= 1;
        return duration <= 0;
    }
}

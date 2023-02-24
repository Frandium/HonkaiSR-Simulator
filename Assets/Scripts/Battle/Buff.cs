using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuffType
{
    Debuff,
    Buff,
    Permanent,
    Count
}

public class Buff : AProgressWithTurn
{
    // 可以做到高度可配置，一个 value buff 应该包含 收益属性、源属性、收益数值、源 min、源 max，目标 min，目标 max，过滤器、持续回合。
    public CommonAttribute targetAttribute { get; protected set; } = CommonAttribute.Count; // 收益属性
    public BuffType buffType { get; protected set; } = BuffType.Debuff;
    public int stack { get; protected set; } = 0; // 叠加次数

    public BuffContent content;
    
    public delegate float BuffContent(Creature source, Creature target, DamageType damageType);

    public Buff(): base("default", int.MaxValue)
    {

    }

    public Buff Set(SimpleValueBuff s)
    {
        buffType = BuffType.Permanent;
        targetAttribute = s.attribute;
        content = (c, e, t) =>
        {
            if (s.type == ValueType.InstantNumber)
                return s.value;
            float b = c.GetBaseAttr(s.attribute);
            return b * s.value;
        };
        return this;
    }

    public Buff Set(string _tag, BuffType type, CommonAttribute target_att, int _duration, BuffContent c)
    {
        tag = _tag;
        buffType = type;
        targetAttribute = target_att;
        times = _duration;
        content = c;
        return this;
    }


    public override bool CountDown()
    {
        if (buffType == BuffType.Permanent)
            return false;
        return base.CountDown();
    }


    public float CalBuffValue(Creature source, Creature target, CommonAttribute attr, DamageType damageType)
    {
        if (targetAttribute != attr)
            return 0;
        return content(source, target, damageType);
    }

}

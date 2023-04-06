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

public class DamageConfig
{
    public DamageType type;
    public Element element;
    public StateType state;
    public DamageConfig(DamageType type =DamageType.All, Element element = Element.Count, StateType state = StateType.Count)
    {
        this.type = type;
        this.element = element;
        this.state = state;
    }

    public static DamageConfig defaultDC = new DamageConfig(DamageType.All, Element.Count);
}

public class Buff : ACountDownBehaviour
{
    // ���������߶ȿ����ã�һ�� value buff Ӧ�ð��� �������ԡ�Դ���ԡ�������ֵ��Դ min��Դ max��Ŀ�� min��Ŀ�� max���������������غϡ�
    public CommonAttribute targetAttribute { get; protected set; } = CommonAttribute.Count; // ��������
    public BuffType buffType { get; protected set; } = BuffType.Debuff;
    public int stack { get; protected set; } = 0; // ���Ӵ���

    public BuffFilter filter;
    public BuffContent content;
    public OnBuffRemove onRemove;
    public delegate void OnBuffRemove(Creature host);
    public delegate bool BuffFilter(Creature source, Creature target, DamageConfig damageAttr);
    public delegate float BuffContent(Creature source, Creature target, DamageConfig damageAttr);

    public Buff(): base("default", CountDownType.All, int.MaxValue, int.MaxValue)
    {

    }

    public Buff Set(PhraseConfig p)
    {
        buffType = BuffType.Permanent;
        targetAttribute = p.attr;
        content = (c, e, t) =>
        {
            if (p.type == ValueType.InstantNumber)
                return (float)p.value;
            float b = c.GetBaseAttr(p.attr);
            return b * (float)p.value;
        };
        return this;
    }

    public Buff Set(string _tag, BuffType type, CommonAttribute target_att, BuffContent c, BuffFilter f = null, int turntime = int.MaxValue,
        CountDownType _ctype = CountDownType.Turn, int triggertime = int.MaxValue, OnBuffRemove remove = null)
    {
        tag = _tag;
        buffType = type;
        targetAttribute = target_att;

        ctype = _ctype;
        _turnTimes = turntime;
        _triggerTimes = triggertime;

        filter = f;
        content = c;
        return this;
    }


    public override bool CountDown(CountDownType ct)
    {
        if (buffType == BuffType.Permanent)
            return false;
        return base.CountDown(ct);
    }


    public float CalBuffValue(Creature source, Creature target, CommonAttribute attr, DamageConfig damageAttr)
    {
        if (targetAttribute != attr || 
            (filter!=null && !filter(source, target, damageAttr))
            )
            return 0;
        float res = content(source, target, damageAttr);
        return res;
    }
    
    
}

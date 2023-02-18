using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Buff
{
    // ���������߶ȿ����ã�һ�� value buff Ӧ�ð��� �������ԡ�Դ���ԡ�������ֵ��Դ min��Դ max��Ŀ�� min��Ŀ�� max���������������غϡ�
    public CommonAttribute targetAttribute { get; protected set; } = CommonAttribute.Count; // ��������
    public BuffType buffType { get; protected set; } = BuffType.Debuff;
    public int duration { get; protected set; } = 0; // ��
    public int stack { get; protected set; } = 0; // ���Ӵ���

    public BuffContent content;

    public CreatureBase host { get; protected set; }

    public delegate float BuffContent(CreatureBase source, CreatureBase target);

    public Buff()
    {

    }

    public Buff Set(SimpleValueBuff s)
    {
        buffType = BuffType.Buff;
        targetAttribute = s.attribute;
        duration = 2;
        content = (c, e) =>
        {
            if (s.type == ValueType.InstantNumber)
                return s.value;
            float b = c.GetBaseAttr(s.attribute);
            return b * s.value;
        };
        return this;
    }

    public Buff Set(BuffType type, CommonAttribute target_att, int _duration, BuffContent c)
    {
        buffType = type;
        targetAttribute = target_att;
        duration = _duration;
        content = c;
        return this;
    }


    public virtual void CountDown()
    {
        duration -= 1;
        if (duration <= 0)
            RemoveMe();
    }

    void RemoveMe()
    {
//        host.RemoveBuff(this);
    }

    public float CalBuffValue(CreatureBase source, CreatureBase target, CommonAttribute attr)
    {
        if (targetAttribute != attr)
            return 0;
        return content(source, target);
    }

}

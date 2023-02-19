using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuffTriggerMoment
{
    OnTurnBegin,
    OnTurnEnd,
    OnTakingDamage,
    OnDealingDamage,
    OnHealing,
    OnHealed,
    Count
}

public enum BuffType
{
    Debuff,
    Buff,
    Permanent,
    Count
}

public class TriggerBuff
{
    public TriggerBuff()
    {

    }
    public TriggerBuff(BuffTriggerMoment moment, BuffType type, int _duration, BuffAction _action)
    {
        triggerMoment = moment;
        buffType = type;
        duration = _duration;
        action = _action;
    }
    public TriggerBuff Set(BuffTriggerMoment moment, BuffType type, int _duration, BuffAction _action)
    {
        triggerMoment = moment;
        buffType = type;
        duration = _duration;
        action = _action;
        return this;
    }

    // ����ʱ��
    public BuffTriggerMoment triggerMoment { get; protected set; } = BuffTriggerMoment.Count;

    // ���ͣ�buff �� debuff
    public BuffType buffType { get; protected set; } = BuffType.Debuff;

    // �����غϣ����� duration �κ���ʧ
    public int duration;
    public int times;

    public delegate void BuffAction(CreatureMono c);
    
    BuffAction action;

    public bool Trigger(CreatureMono c)
    {
        action(c);
        times -= 1;
        return times <= 0;
    }
}

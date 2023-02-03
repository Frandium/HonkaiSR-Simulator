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
    Count
}

public class TriggerBuff
{

    public TriggerBuff(BuffTriggerMoment moment, BuffType type, int _duration, BuffAction _action)
    {
        triggerMoment = moment;
        buffType = type;
        duration = _duration;
        action = _action;
    }

    // ����ʱ��
    public BuffTriggerMoment triggerMoment { get; protected set; } = BuffTriggerMoment.Count;

    // ���ͣ�buff �� debuff
    public BuffType buffType { get; protected set; } = BuffType.Debuff;

    // �����غϣ����� duration �κ���ʧ
    public int duration;

    public delegate void BuffAction(Creature c);
    
    BuffAction action;

    public void Trigger(Creature c)
    {
        action(c);
    }
}

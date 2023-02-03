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

    // 触发时机
    public BuffTriggerMoment triggerMoment { get; protected set; } = BuffTriggerMoment.Count;

    // 类型，buff 或 debuff
    public BuffType buffType { get; protected set; } = BuffType.Debuff;

    // 持续回合，持续 duration 次后消失
    public int duration;

    public delegate void BuffAction(Creature c);
    
    BuffAction action;

    public void Trigger(Creature c)
    {
        action(c);
    }
}

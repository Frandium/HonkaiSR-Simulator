using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : ACountDownBehaviour
{
    public float maxHp { get; protected set; }
    public float hp { get; protected set; }

    public Shield(string tag, float max, CountDownType cdt, int turnTimes, int triggerTimes): base(tag, CountDownType.Turn, turnTimes, triggerTimes)
    {
        maxHp = max;
        hp = maxHp;
    }

    public float TakeDamage(Damage d)
    {
        hp -= d.value;
        return hp;
    }

    public static Shield MakeShield(string tag, Creature source, CommonAttribute attr, float rate, float offset, int turnTimes, 
        CountDownType cdt = CountDownType.Turn, int triggerTimes = int.MaxValue)
    {
        float hp = source.GetFinalAttr(attr) * rate + offset;
        hp *= 1 + source.GetFinalAttr(CommonAttribute.ShieldBonus);
        Shield shield = new Shield(tag, hp, cdt, turnTimes, triggerTimes);
        return shield;
    }
}

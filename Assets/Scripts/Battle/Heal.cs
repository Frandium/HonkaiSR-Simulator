using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heal
{
    public float fullValue { get; set; }
    public float realValue { get; set; }
    
    public Heal(float v)
    {
        fullValue = v;
        realValue = v;
    }

    public static Heal NormalHeal(Creature source, Creature target, CommonAttribute attr, float rate, float offset = 0)
    {
        float heal_base = source.GetFinalAttr(source, target, attr, DamageConfig.defaultDC) * rate + offset;
        // ÷Œ¡∆º”≥…
        float healbonues = source.GetFinalAttr(source, target, CommonAttribute.HealBonus, DamageConfig.defaultDC);
        heal_base *= (1 + healbonues);
        return new Heal(heal_base);
    }
}

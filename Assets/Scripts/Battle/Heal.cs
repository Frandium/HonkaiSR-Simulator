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
        float heal_base = source.GetFinalAttr(source, target, attr, DamageType.All) * rate + offset;
        // ÷Œ¡∆º”≥…
        float healbonues = source.GetFinalAttr(source, target, CommonAttribute.HealBonus, DamageType.All);
        heal_base *= (1 + healbonues);
        return new Heal(heal_base);
    }
}

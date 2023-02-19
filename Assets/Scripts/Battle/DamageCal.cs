using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCal
{
    public static float DamageCharacter(Creature source, Creature target, CommonAttribute attr, Element element, float rate, float offset = 0)
    {
        float dmgBase = source.GetFinalAttr(source, target, attr) * rate / 100.0f + offset;
        float elebonus = source.GetFinalAttr(source, target, CommonAttribute.AnemoBonus + (int)element);
        float genebonus = source.GetFinalAttr(source, target, CommonAttribute.GeneralBonus);
        float overallBonus = 1 + Mathf.Max(0, elebonus + genebonus); // 伤害加成下限 0，无上限
        float dmg = dmgBase * overallBonus;
        if (Random.Range(0, 1000) < source.GetFinalAttr(source, target, CommonAttribute.CriticalRate) * 1000)
        {
            dmg *= source.GetFinalAttr(source, target, CommonAttribute.CriticalDamage);
        }
        return dmg;
    }

    //public static float ResistDamage(float value, Element element, Creature target)
    //{
    //    int type = (int)element;
    //    float defRate = 1 - target.GetFinalAttr((int)CommonAttribute.DEF) / (target.GetFinalAttr((int)CommonAttribute.DEF) + 2000);
    //    float overallResist = 1 - (target.GetFinalAttr((int)CommonAttribute.AnemoResist + (int)element) + target.GetFinalAttr((int)CommonAttribute.GeneralResist));
    //    if (overallResist < .05f) overallResist = .05f; // 抗性上限 95%，无下限，但 0 以下折半
    //    if (overallResist > 1) overallResist = 1 + (overallResist - 1) * .5f;
    //    value *= overallResist * defRate;
    //    return value;
    //}

    public static float Heal(CreatureMono source, CreatureMono target, CommonAttribute attr, float rate, float offset)
    {
        return 0;
        //float heal_base = source.GetFinalAttr(source, target, attr) * rate / 100 + offset;
        //// 治疗加成
        //return heal_base;
    }
}

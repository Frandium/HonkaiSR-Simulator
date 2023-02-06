using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCal
{
    public static float ATKDamageCharacter(Character source, Element element, float rate)
    {
        int type = (int)element;
        float dmgBase = source.atk * rate / 100.0f;
        float overallBonus = 1 + Mathf.Max(0, source.GetFinalAttr((int)CharacterAttribute.AnemoBonus + (int)element)  + source.GetFinalAttr((int)CharacterAttribute.GeneralBonus)); // 伤害加成下限 0，无上限
        float dmg = dmgBase * overallBonus;
        if (Random.Range(0, 1000) < source.GetFinalAttr((int)CharacterAttribute.CriticalRate) * 1000)
        {
            dmg *= source.GetFinalAttr((int)CharacterAttribute.CriticalDmg);
        }
        return dmg;
    }
    public static float ATKDamageEnemy(Enemy source, Element element, float rate)
    {
        int type = (int)element;
        float dmgBase = source.atk * rate / 100.0f;
        float overallBonus = 1; // + Mathf.Max(0, source.GetFinalAttr((int)CharacterAttribute.AnemoBonus + (int)element) + source.GetFinalAttr((int)CharacterAttribute.GeneralBonus)); // 伤害加成下限 0，无上限
        float dmg = dmgBase * overallBonus;
        //if (Random.Range(0, 1000) < source.GetFinalAttr((int)CharacterAttribute.CriticalRate) * 1000)
        //{
        //    dmg *= source.GetFinalAttr((int)CharacterAttribute.CriticalDmg);
        //}
        return dmg;
    }

    public static float ResistDamage(float value, Element element, Creature target)
    {
        int type = (int)element;
        float defRate = target.def / (target.def + 2000);
        float overallResist = 1 - (target.GetFinalAttr((int)CommonAttribute.AnemoResist + (int)element) + target.GetFinalAttr((int)CommonAttribute.GeneralResist));
        if (overallResist < .05f) overallResist = .05f; // 抗性上限 95%，无下限，但 0 以下折半
        if (overallResist > 1) overallResist = 1 + (overallResist - 1) * .5f;
        value *= overallResist * defRate;
        return value;
    }

    public static float MaxHPHeal(Creature source, float rate, float offset)
    {
        float heal_base = source.GetFinalAttr((int)CommonAttribute.MaxHP) * rate / 100 + offset;
        // 治疗加成
        return heal_base;
    }
}

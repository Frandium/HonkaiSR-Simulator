using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCal
{
    public static float ATKDamage(Creature source, Element element, float rate)
    {
        int type = (int)element;
        float dmgBase = (source.atk + source.atkBuff) * rate / 100.0f;
        float overallBonus = 1 + Mathf.Max(0, source.elementalBonus[type] + source.elementalBonusBuff[type] + source.generalBonus); // �˺��ӳ����� 0��������
        float dmg = dmgBase * overallBonus;
        /*if (Random.Range(0, 1000) < source.critical_rate)
        {
            dmg *= source.critical_dmg;
        }*/
        return dmg;
    }

    public static float ResistDamage(float value, Element element, Creature target)
    {
        int type = (int)element;
        float defRate = target.def / (target.def + 2000);
        float overallResist = 1 - (target.elementalResist[type] + target.elementalResistBuff[type] + target.generalResist + target.generalResistBuff);
        if (overallResist < .05f) overallResist = .05f; // �������� 95%�������ޣ��� 0 �����۰�
        if (overallResist > 1) overallResist = 1 + (overallResist - 1) * .5f;
        value *= overallResist * defRate;
        return value;
    }

    public static float MaxHPHeal(Creature source, float rate, float offset)
    {
        float heal_base = source.maxHp * rate / 100 + offset;
        // ���Ƽӳ�
        return heal_base;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage
{
    public float fullValue { get; set; }
    public float realValue { get; set; }
    public Element element { get; protected set; }
    public DamageType type { get; set; }
    public bool isCritical { get; protected set; }

    public Damage(float v, Element e, DamageType t, bool b)
    {
        fullValue = v;
        element = e;
        type = t;
        isCritical = b;
    }

    public static Damage NormalDamage(Creature source, Creature target, CommonAttribute attr, 
        float rate, DamageConfig damageType, float offset = 0)
    {
        float dmgBase = source.GetFinalAttr(source, target, attr, damageType) * rate+ offset;
        float elebonus = source.GetFinalAttr(source, target, CommonAttribute.PhysicalBonus + (int)damageType.element, damageType);
        float genebonus = source.GetFinalAttr(source, target, CommonAttribute.GeneralBonus, damageType);
        float overallBonus = 1 + Mathf.Max(0, elebonus + genebonus); // 伤害加成下限 0，无上限
        float dmg = dmgBase * overallBonus;
        bool critical = Utils.TwoRandom(source.GetFinalAttr(source, target, CommonAttribute.CriticalRate, damageType));
        if (critical)
        {
            dmg *= (1 + source.GetFinalAttr(source, target, CommonAttribute.CriticalDamage, damageType));
        }

        float def = target.GetFinalAttr(source, target, CommonAttribute.DEF, damageType);
        float defIgnore = source.GetFinalAttr(source, target, CommonAttribute.DEFIgnore, damageType);
        def *= (1 - defIgnore);
        float defRate = 1 - def / (def + 200 + source.level * 10);
        float overallResist = 1 
            - target.GetFinalAttr(source, target, CommonAttribute.PhysicalResist + (int)damageType.element, damageType) 
            - target.GetFinalAttr(source, target, CommonAttribute.GeneralResist, damageType);
        if (overallResist < .05f) overallResist = .05f; // 抗性上限 95%，无下限，但 0 以下折半
        if (overallResist > 1) overallResist = 1 + (overallResist - 1) * .5f;
        overallResist -= source.GetFinalAttr(CommonAttribute.PhysicalPenetrate + (int)damageType.element);
        dmg *= overallResist * defRate * (1 - target.GetFinalAttr(CommonAttribute.DmgDown));
        return new Damage(dmg, damageType.element, damageType.type, critical);
    }

    public static float Heal(CreatureMono source, CreatureMono target, CommonAttribute attr, float rate, float offset)
    {
        return 0;
        //float heal_base = source.GetFinalAttr(source, target, attr) * rate / 100 + offset;
        //// 治疗加成
        //return heal_base;
    }
}

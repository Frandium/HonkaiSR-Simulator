using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreetFighter : AArtifactTalent
{
    public StreetFighter(int c) : base(c)
    {

    }

    public override void OnEquiping(Character character)
    {
        if (count < 2)
            return;
        character.AddBuff("streetFighter2", BuffType.Permanent, CommonAttribute.PhysicalBonus, ValueType.InstantNumber, .1f);
        if (count < 4)
            return;
        character.afterDealingDamage.Add(new TriggerEvent<Creature.DamageEvent>("streetFighter4Dealdmg", (s, d) =>
        {
            character.AddBuff("streetFighter4ATKUp", BuffType.Buff, CommonAttribute.ATK, ValueType.Percentage, .05f, cdtype: CountDownType.Permanent, maxStack: 5);
            return d;
        }));
        character.beforeTakingDamage.Add(new TriggerEvent<Creature.DamageEvent>("streetFighter4Takedmg", (s, d) =>
        {
            character.AddBuff("streetFighter4ATKUp", BuffType.Buff, CommonAttribute.ATK, ValueType.Percentage, .05f, cdtype: CountDownType.Permanent, maxStack: 5);
            return d;
        }));
        
    }


    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("streetFighter2");
        character.afterDealingDamage.RemoveAll(t => t.tag == "streetFighter4Dealdmg");
        character.beforeTakingDamage.RemoveAll(t => t.tag == "streetFighter4Takedmg");
    }
}
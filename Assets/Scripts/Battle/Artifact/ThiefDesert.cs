using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThiefDesert : AArtifactTalent
{
    public ThiefDesert(int c) : base(c)
    {

    }

    public override void OnEquiping(Character character)
    {
        if (count < 2)
            return;
        character.AddBuff("thiefDesert2", BuffType.Permanent, CommonAttribute.ImaginaryBonus, ValueType.InstantNumber, .1f);
        if (count < 4)
            return;
        character.AddBuff("thiefDesert4CrtRate", BuffType.Permanent, CommonAttribute.CriticalRate, ValueType.InstantNumber, .1f, (s, t, d) => {
            return t.buffs.Find(b => b.buffType == BuffType.Debuff) != null ||
            t.IsUnderControlledDebuff();
        });
        character.AddBuff("thiefDesert4CrtDmg", BuffType.Permanent, CommonAttribute.CriticalDamage, ValueType.InstantNumber, .2f, (s, t, d) => {
            return t.states.Find(s => s.state == StateType.Restricted) != null;
        });
    }


    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("thiefDesert2");
        character.RemoveBuff("thiefDesert4CrtRate");
        character.RemoveBuff("thiefDesert4CrtDmg");
    }
}
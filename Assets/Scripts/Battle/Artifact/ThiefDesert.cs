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
        character.AddBuff(Utils.valueBuffPool.GetOne().Set("thiefDesert4CrtRate", BuffType.Buff, CommonAttribute.CriticalRate, (s, t, d) => {
            if(t.buffs.Find(b => b.buffType == BuffType.Debuff) != null || t.states.Find(s => s.state == StateType.Frozen || s.state==StateType.Restricted) != null)
            {
                return .1f;
            }
            return 0;
        }));
        character.AddBuff(Utils.valueBuffPool.GetOne().Set("thiefDesert4CrtDmg", BuffType.Buff, CommonAttribute.CriticalDamage, (s, t, d) => {
            if (t.states.Find(s => s.state == StateType.Restricted) != null)
            {
                return .2f;
            }
            return 0;
        }));
    }


    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("thiefDesert2");
        character.RemoveBuff("thiefDesert4CrtRate");
        character.RemoveBuff("thiefDesert4CrtDmg");
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThiefCountry : AArtifactTalent
{
    public ThiefCountry(int c) : base(c)
    {

    }

    public override void OnEquiping(Character character)
    {
        if (count < 2)
            return;
        character.AddBuff("thiefCountry2", BuffType.Permanent, CommonAttribute.BreakBonus, ValueType.InstantNumber, .2f);
        character.AddBuff(Utils.valueBuffPool.GetOne().Set("thiefCountry2Cond", BuffType.Permanent, CommonAttribute.BreakBonus, (s, t, d) => {
            if (character.GetFinalAttr(CommonAttribute.Speed) >= 145)
            {
                return .28f;
            }
            return 0;
        }, _ctype: CountDownType.Permanent));
    }


    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("thiefCountry2");
        character.RemoveBuff("thiefCountry2Cond");
    }
}
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
        character.AddBuff("thiefCountry2Cond", BuffType.Permanent, CommonAttribute.BreakBonus, ValueType.InstantNumber, .28f, (s, t, d) => {
        return character.GetFinalAttr(CommonAttribute.Speed) >= 145;
        },cdtype: CountDownType.Permanent);
    }


    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("thiefCountry2");
        character.RemoveBuff("thiefCountry2Cond");
    }
}
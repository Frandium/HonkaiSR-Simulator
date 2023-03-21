using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarVariation : AArtifactTalent
{
    public StarVariation(int c) : base(c)
    {

    }

    public override void OnEquiping(Character character)
    {
        if (count < 2)
            return;
        character.AddBuff("starVariation2", BuffType.Permanent, CommonAttribute.CriticalRate, ValueType.InstantNumber, .08f);
        character.AddBuff(Utils.valueBuffPool.GetOne().Set("starVariation2Cond", BuffType.Permanent, CommonAttribute.GeneralBonus, (s, t, d) => {
            if (character.GetFinalAttr(CommonAttribute.Speed) >= .8f && (d== DamageType.Attack || d == DamageType.Skill))
            {
                return .2f;
            }
            return 0;
        }));
    }


    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("starVariation2");
        character.RemoveBuff("starVariation2Cond");
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoppedSRST : AArtifactTalent
{
    public StoppedSRST(int c) : base(c)
    {

    }

    public override void OnEquiping(Character character)
    {
        if (count < 2)
            return;
        character.AddBuff("stoppedSRST2", BuffType.Permanent, CommonAttribute.CriticalRate, ValueType.InstantNumber, .08f);
        character.AddBuff(Utils.valueBuffPool.GetOne().Set("stoppedSRST2Cond", BuffType.Permanent, CommonAttribute.GeneralBonus, (s, t, d) => {
            if (character.GetFinalAttr(CommonAttribute.Speed) >= .5f && (d == DamageType.Burst || d == DamageType.Additional))
            {
                return .15f;
            }
            return 0;
        }));
    }


    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("stoppedSRST2");
        character.RemoveBuff("stoppedSRST2Cond");
    }
}

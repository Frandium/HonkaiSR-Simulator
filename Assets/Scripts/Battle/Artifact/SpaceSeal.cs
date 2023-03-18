using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceSeal : AArtifactTalent
{
    public SpaceSeal(int c) : base(c)
    {

    }

    public override void OnEquiping(Character character)
    {
        if (count < 2)
            return;
        character.AddBuff("spaceSeal2", BuffType.Permanent, CommonAttribute.ATK, ValueType.Percentage, .12f);
        character.AddBuff(Utils.valueBuffPool.GetOne().Set("spaceSeal2Cond", BuffType.Permanent, CommonAttribute.ATK, (s, t, d) => {
            if (character.GetFinalAttr(CommonAttribute.Speed) >= 120)
            {
                return .12f * character.GetBaseAttr(CommonAttribute.ATK);
            }
            return 0;
        }));
    }


    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("spaceSeal2");
        character.RemoveBuff("spaceSeal2Cond");
    }
}

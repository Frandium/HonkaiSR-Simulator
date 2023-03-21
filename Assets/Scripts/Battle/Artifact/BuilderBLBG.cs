using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuilderBLBG : AArtifactTalent
{
    public BuilderBLBG(int c) : base(c)
    {

    }

    public override void OnEquiping(Character character)
    {
        if (count < 2)
            return;
        character.AddBuff("builderBLBG2", BuffType.Permanent, CommonAttribute.DEF, ValueType.Percentage, .15f);
        character.AddBuff(Utils.valueBuffPool.GetOne().Set("builderBLBG2Cond", BuffType.Permanent, CommonAttribute.DEF, (s, t, d) => {
            if (character.GetFinalAttr(CommonAttribute.EffectHit) >= .5f)
            {
                return character.GetBaseAttr(CommonAttribute.DEF) * .15f;
            }
            return 0;
        }));
    }


    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("builderBLBG2");
        character.RemoveBuff("builderBLBG2Cond");
    }
}

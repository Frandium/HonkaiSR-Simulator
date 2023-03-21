using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyBussiness : AArtifactTalent
{
    public GalaxyBussiness(int c) : base(c)
    {

    }

    public override void OnEquiping(Character character)
    {
        if (count < 2)
            return;
        character.AddBuff("galaxyBussiness2EffectHit", BuffType.Permanent, CommonAttribute.EffectHit, ValueType.InstantNumber, .1f);
        character.AddBuff(Utils.valueBuffPool.GetOne().Set("galaxyBussiness2ATK", BuffType.Permanent, CommonAttribute.ATK, (s, t, d) => {
            return Mathf.Min(character.GetFinalAttr(CommonAttribute.EffectHit), 1) * .25f * character.GetBaseAttr(CommonAttribute.ATK);
        }));
    }


    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("galaxyBussiness2EffectHit");
        character.RemoveBuff("galaxyBussiness2ATK");
    }
}
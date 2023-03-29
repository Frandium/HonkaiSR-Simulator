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
        character.AddBuff("spaceSeal2Cond", BuffType.Permanent, CommonAttribute.ATK, ValueType.Percentage, .12f,
            (s, t, d) => { return character.GetFinalAttr(CommonAttribute.Speed) >= 120; });
    }


    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("spaceSeal2");
        character.RemoveBuff("spaceSeal2Cond");
    }
}

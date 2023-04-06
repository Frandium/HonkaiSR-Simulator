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
        character.AddBuff("starVariation2Cond", BuffType.Permanent, CommonAttribute.GeneralBonus, ValueType.InstantNumber, .2f,
            (s, t, d) => {
                return character.GetFinalAttr(CommonAttribute.Speed) >= .8f && (d.type == DamageType.Attack || d.type == DamageType.Skill);
        });
    }


    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("starVariation2");
        character.RemoveBuff("starVariation2Cond");
    }
}
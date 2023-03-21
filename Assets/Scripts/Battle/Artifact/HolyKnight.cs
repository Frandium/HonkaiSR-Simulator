using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HolyKnight : AArtifactTalent
{
    public HolyKnight(int c) : base(c)
    {

    }

    public override void OnEquiping(Character character)
    {
        if (count >= 2)
        {
            character.AddBuff("holyKnight2", BuffType.Permanent, CommonAttribute.DEF, ValueType.Percentage, .12f);
            if (count >= 4)
            {
                 character.AddBuff("holyKnight4", BuffType.Permanent, CommonAttribute.ShieldBonus, ValueType.InstantNumber, .20f);
            }
        }
    }

    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("holyKnight2");
        character.afterBurst.RemoveAll(t => t.tag == "holyKnight4");
    }
}

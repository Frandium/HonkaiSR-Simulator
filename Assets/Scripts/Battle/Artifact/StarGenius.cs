using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarGenius : AArtifactTalent
{
    public StarGenius(int c) : base(c)
    {

    }

    public override void OnEquiping(Character character)
    {
        if (count < 2)
            return;
        character.AddBuff("starGenius2", BuffType.Permanent, CommonAttribute.QuantusBonus, ValueType.InstantNumber, .1f);
        
        if (count < 4)
            return;
        character.AddBuff(Utils.valueBuffPool.GetOne().Set("starGenius4", BuffType.Permanent, CommonAttribute.DEFIgnore, (s, t, dt) =>
        {
            if (t is Enemy)
            {
                Enemy e = t as Enemy;
                if (e.weakPoint.Contains(Element.Quantus))
                    return .25f;
            }
            return 0;
        }
        ));
    }

    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("starGenius2");
        character.RemoveBuff("starGenius4");
    }
}

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
        character.AddBuff("starGenius4", BuffType.Permanent, CommonAttribute.DEFIgnore, ValueType.InstantNumber, .25f, 
            (s, t, dt) =>{
            return t is Enemy && (t as Enemy).weakPoint.Contains(Element.Quantus);
        });
    }

    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("starGenius2");
        character.RemoveBuff("starGenius4");
    }
}

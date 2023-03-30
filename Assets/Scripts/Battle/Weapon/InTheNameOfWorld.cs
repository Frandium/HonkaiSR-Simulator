using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InTheNameOfWorld : AWeaponTalent
{
    public InTheNameOfWorld(JsonData config, int r) : base(config, r)
    {
    }

    float dmgUp, effHit, atk;
    public override void OnEquiping(Character character)
    {
        dmgUp = (float)(double)config["effect"]["dmgUp"]["value"][refine];
        effHit = (float)(double)config["effect"]["effHit"]["value"][refine];
        atk = (float)(double)config["effect"]["atk"]["value"][refine];
        character.AddBuff("inTheNameOfWorlddmgUp", BuffType.Permanent, CommonAttribute.GeneralBonus, ValueType.InstantNumber, dmgUp, (_, t, _) =>
        {
            return t.IsUnderNegativeState();
        });

        character.AddBuff("inTheNameOfWorldEffHit", BuffType.Permanent, CommonAttribute.EffectHit, ValueType.InstantNumber, effHit, (_, _, d) =>
        {
            return d.type == DamageType.Skill;
        });
        character.AddBuff("inTheNameOfWorldAtk", BuffType.Permanent, CommonAttribute.ATK, ValueType.InstantNumber, atk, (_, _, d) =>
        {
            return d.type == DamageType.Skill;
        });
    }

    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("inTheNameOfWorlddmgUp");
        character.RemoveBuff("inTheNameOfWorldEffHit");
        character.RemoveBuff("inTheNameOfWorldAtk");
    }
}

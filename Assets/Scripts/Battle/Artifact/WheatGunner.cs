using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheatGunner : AArtifactTalent
{
    public WheatGunner(int c) : base(c)
    {

    }

    public override void OnEquiping(Character character)
    {
        if (count < 2)
            return;
        character.AddBuff("wheatGunner2", BuffType.Permanent, CommonAttribute.ATK, ValueType.Percentage, .1f);
        if (count < 4)
            return;
        character.AddBuff("wheatGunner4Speed", BuffType.Permanent, CommonAttribute.Speed, ValueType.Percentage, .06f);
        character.AddBuff("wheatGunner4NATK", BuffType.Permanent, CommonAttribute.GeneralBonus, ValueType.InstantNumber, .1f, damageType: DamageType.Attack);
    }


    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("wheatGunner2");
        character.RemoveBuff("wheatGunner4Speed");
        character.RemoveBuff("wheatGunner4NATK");
    }
}
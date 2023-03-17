using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireSmith : AArtifactTalent
{
    public FireSmith(int c) : base(c)
    {

    }

    public override void OnEquiping(Character character)
    {
        if (count >= 2)
        {
            character.AddBuff("fireSmith2", BuffType.Permanent, CommonAttribute.PyroBonus, ValueType.InstantNumber, .1f);
            if (count >= 4)
            {
                character.AddBuff("fireSmith4", BuffType.Permanent, CommonAttribute.GeneralBonus, ValueType.InstantNumber, .12f, damageType: DamageType.Skill);
                character.onBurst.Add(new TriggerEvent<Character.TalentUponTarget>("fireSmith4", e =>
                {
                    character.AddBuff("fireSmith4PyroDmg", BuffType.Buff, CommonAttribute.PyroBonus, ValueType.InstantNumber, .12f, cdtype: CountDownType.Trigger, triggertime: 1);
                }));
            }
        }
    }

    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("fireSmith2");
        character.RemoveBuff("fireSmith4");
        character.onBurst.RemoveAll(t => t.tag == "fireSmith4");
    }
}

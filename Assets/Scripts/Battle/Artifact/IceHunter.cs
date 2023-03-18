using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceHunter : AArtifactTalent
{
    public IceHunter(int c): base(c)
    {

    }

    public override void OnEquiping(Character character)
    {
        if(count >= 2)
        {
            character.AddBuff("iceHunter2", BuffType.Permanent, CommonAttribute.CryoBonus, ValueType.InstantNumber, .1f);
            if(count >= 4)
            {
                character.afterBurst.Add(new TriggerEvent<Character.TalentUponTarget>("iceHunter4", t =>
                {
                    character.AddBuff("iceHunter4CrtDmg", BuffType.Buff, CommonAttribute.CriticalDamage, ValueType.InstantNumber, .25f, 3);
                }));
            }
        }
    }

    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("iceHunter2");
        character.afterBurst.RemoveAll(t => t.tag == "iceHunter4");
    }
}


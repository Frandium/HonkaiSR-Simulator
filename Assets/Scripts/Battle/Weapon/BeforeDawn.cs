using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class BeforeDawn : AWeaponTalent
{
    public BeforeDawn(JsonData d, int r):base(d, r)
    {

    }

    float crtdmg, dmgup, add;
    public override void OnEquiping(Character character)
    {
        crtdmg = (float)(double)config["effect"]["crtdmg"]["value"][refine];
        add = (float)(double)config["effect"]["add"]["value"][refine];
        dmgup = (float)(double)config["effect"]["dmgup"]["value"][refine];
        character.AddBuff("beforeDawnCrtDmg", BuffType.Permanent, CommonAttribute.CriticalDamage, ValueType.InstantNumber, crtdmg);
        character.AddBuff("beforeDawnSkillDmgUp", BuffType.Permanent, CommonAttribute.GeneralBonus, ValueType.InstantNumber, dmgup, damageType: DamageType.Skill);
        character.AddBuff("beforeDawnSkillDmgUp", BuffType.Permanent, CommonAttribute.GeneralBonus, ValueType.InstantNumber, dmgup, damageType: DamageType.Burst);
        character.afterBurst.Add(new TriggerEvent<Character.TalentUponTarget>("beforeDawn", t =>
        {
            character.AddBuff("MengShen", BuffType.Buff, CommonAttribute.GeneralBonus, ValueType.InstantNumber, add, 
                int.MaxValue, DamageType.Additional, CountDownType.Trigger, 1);
        }));

        character.afterSkill.Add(new TriggerEvent<Character.TalentUponTarget>("beforeDawn", t =>
        {
            character.AddBuff("MengShen", BuffType.Buff, CommonAttribute.GeneralBonus, ValueType.InstantNumber, add,
                int.MaxValue, DamageType.Additional, CountDownType.Trigger, 1);
        }));

    }

    public override void OnTakingOff(Character character)
    {
        // write code to remove all buffs added in OnEquiping
        character.RemoveBuff("beforeDawnCrtDmg");
        character.RemoveBuff("beforeDawnSkillDmgUp");
    }
}

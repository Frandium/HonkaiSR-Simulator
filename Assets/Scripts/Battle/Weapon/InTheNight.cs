using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class InTheNight : AWeaponTalent
{
    public InTheNight(JsonData d, int r): base(d, r)
    {

    }

    float crit, atkSkill, burst;
    public override void OnEquiping(Character character)
    {
        crit = (float)(double)config["effect"]["crit"]["value"][refine];
        atkSkill = (float)(double)config["effect"]["atkSkill"]["value"][refine];
        burst = (float)(double)config["effect"]["burst"]["value"][refine];
        character.AddBuff("inTheNightCritRate", BuffType.Permanent, CommonAttribute.CriticalRate, ValueType.InstantNumber, crit);
        character.AddBuff("inTheNightAtkSkill", BuffType.Permanent, CommonAttribute.GeneralBonus, (c, e, t) =>
        {
            float additionalSpeed = c.GetFinalAttr(CommonAttribute.Speed) - 100;
            if (additionalSpeed <= 0) return 0;
            int times = (int)additionalSpeed / 10;
            if (times > 8) times = 8;
            return atkSkill * times;
        }, (s, d, t) => { return t == DamageType.Attack || t == DamageType.Skill; });
        character.AddBuff("inTheNightBurst", BuffType.Permanent, CommonAttribute.GeneralBonus, (c, e, t) =>
        {
            float additionalSpeed = c.GetFinalAttr(CommonAttribute.Speed) - 100;
            if (additionalSpeed <= 0) return 0;
            int times = (int)additionalSpeed / 10;
            if (times > 8) times = 8;
            return burst * times;
        }, (s, t, d) => { return d == DamageType.Attack || d == DamageType.Skill; }
            );
    }

    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("inTheNightCritRate");
        character.RemoveBuff("inTheNightAtkSkill");
        character.RemoveBuff("inTheNightBurst");
    }
}

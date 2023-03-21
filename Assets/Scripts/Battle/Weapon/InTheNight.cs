using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class InTheNight : AWeaponTalent
{
    int refine = 0;
    public InTheNight(JsonData d, int r): base(d)
    {
        refine = r;
    }
    float crit, atkSkill, burst;
    public override void OnEquiping(Character character)
    {
        crit = (float)(double)config["effect"]["crit"]["value"][refine];
        atkSkill = (float)(double)config["effect"]["atkSkill"]["value"][refine];
        burst = (float)(double)config["effect"]["burst"]["value"][refine];
        character.AddBuff("inTheNightCritRate", BuffType.Permanent, CommonAttribute.CriticalRate, ValueType.InstantNumber, crit);
        character.AddBuff(Utils.valueBuffPool.GetOne().Set("inTheNightAtkSkill", BuffType.Permanent, CommonAttribute.GeneralBonus, (c, e, t) =>
        {
            if (t == DamageType.Attack || t == DamageType.Skill)
            {
                float additionalSpeed = c.GetFinalAttr(CommonAttribute.Speed) - 100;
                if (additionalSpeed <= 0) return 0;
                int times = (int)additionalSpeed / 10;
                if (times > 8) times = 8;
                return atkSkill * times;
            }
            return 0;
        })
            );
        character.AddBuff(Utils.valueBuffPool.GetOne().Set("inTheNightBurst", BuffType.Permanent, CommonAttribute.GeneralBonus, (c, e, t) =>
        {
            if (t == DamageType.Attack || t == DamageType.Skill)
            {
                float additionalSpeed = c.GetFinalAttr(CommonAttribute.Speed) - 100;
                if (additionalSpeed <= 0) return 0;
                int times = (int)additionalSpeed / 10;
                if (times > 8) times = 8;
                return burst * times;
            }
            return 0;
        })
            );
    }

    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("inTheNightCritRate");
        character.RemoveBuff("inTheNightAtkSkill");
        character.RemoveBuff("inTheNightBurst");
    }
}

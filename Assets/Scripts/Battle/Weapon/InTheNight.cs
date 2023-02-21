using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class InTheNight : AEquipmentTalents
{
    int refine = 0;
    public InTheNight(JsonData d, int r): base(d)
    {
        refine = r;
    }

    public override void OnEquiping(Character character)
    {
        character.AddBuff("inTheNightCritRate", BuffType.Permanent, CommonAttribute.CriticalRate, int.MaxValue, ValueType.InstantNumber, (float)(double)config["crit"][refine]);
        character.AddBuff("inTheNightAtkSkill", Utils.valueBuffPool.GetOne().Set(BuffType.Permanent, CommonAttribute.GeneralBonus, int.MaxValue, (c, e, t) =>
        {
            if (t == DamageType.Attack || t == DamageType.Skill)
            {
                float additionalSpeed = c.GetFinalAttr(CommonAttribute.Speed) - 100;
                if (additionalSpeed <= 0) return 0;
                int times = (int)additionalSpeed / 10;
                if (times > 8) times = 8;
                return (float)(double)config["atkskill"][refine] * times;
            }
            return 0;
        })
            );
        character.AddBuff("inTheNightBurst", Utils.valueBuffPool.GetOne().Set(BuffType.Permanent, CommonAttribute.GeneralBonus, int.MaxValue, (c, e, t) =>
        {
            if (t == DamageType.Attack || t == DamageType.Skill)
            {
                float additionalSpeed = c.GetFinalAttr(CommonAttribute.Speed) - 100;
                if (additionalSpeed <= 0) return 0;
                int times = (int)additionalSpeed / 10;
                if (times > 8) times = 8;
                return (float)(double)config["burst"][refine] * times;
            }
            return 0;
        })
            );
    }

    public override void OnTakingOff(Character character)
    {
        throw new System.NotImplementedException();
    }
}

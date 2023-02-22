using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class VictoryMoment : AEquipmentTalents
{
    int refine;
    public VictoryMoment(JsonData c, int r) : base(c)
    {
        refine = r;
    }

    public override void OnEquiping(Character character)
    {
        character.AddBuff("victoryMomentDEF", BuffType.Permanent, CommonAttribute.DEF, ValueType.Percentage, (float)(double)config["def1"][refine]);
        character.AddBuff("victoryMomentEffectHit", BuffType.Permanent, CommonAttribute.EffectHit, ValueType.InstantNumber, (float)(double)config["effHit"][refine]);
        character.AddBuff("victoryMomentTaunt", BuffType.Permanent, CommonAttribute.Taunt, ValueType.InstantNumber, 40);

        character.onTakingDamage.Add(new TriggerEvent<Creature.DamageEvent>("victoryMomentDEF2", (s, v, e, d) =>
        {
            character.AddBuff("victoryMomentDEF2", BuffType.Buff, CommonAttribute.DEF, ValueType.Percentage, (float)(double)config["def2"][refine], 1);
            return v;
        }));
    }

    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("victoryMomentDEF");
        character.RemoveBuff("victoryMomentEffectHit");
        character.RemoveBuff("victoryMomentTaunt");
        character.onTakingDamage.RemoveAll(r => r.tag == "victoryMomentDEF2");
        throw new System.NotImplementedException();
    }
}
